using Doozy.Runtime.Nody;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Leaderboard : MonoBehaviour {
	[Header("Message Broker")]
	[SerializeField] string leaderboardQueueName = "qu.iaapa-unity-leaderboard";
	[SerializeField] string leaderboardRoutingKey = "#.leaderboard";

	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	// WE ARE USING FLOW CONTROLLER DIRECTLY INSTEAD OF ACCESSING THE BUTTON EVENTS DYNAMICALLY
	// BECAUSE DOOZY BUTTON EVENTS CANNOT BE INVOKED :-(
	[Header("Flow Controller")]
	[SerializeField] GameObject flowController;

	private FlowController flowControllerComponent;

	// Reference to the player cell prefab and parent containers
	[Header("Player Cell")]
	[SerializeField] GameObject playerCell;
	[SerializeField] GameObject playerCellParentTop24;
	[SerializeField] GameObject playerCellParentRemainder;

	private GameObject playerCellParent;

	// Reference to the ShowControl class instance in the game
	private ShowControl showControl;

	// The latest leaderboard message from Rabbit MQ
	private LeaderboardMessage currentLeaderboardData;
	private LeaderboardMessage CurrentLeaderboardData {
		get => currentLeaderboardData;
		set {
			currentLeaderboardData = value;
			Debug.Log("CurrentLeaderboardData has been set to " + value);
			NeedsUpdate = true;
		}
	}

	// Whether the text elements need updating due to a new game state message
	private bool needsUpdate = false;
	private bool NeedsUpdate {
		get => needsUpdate;
		set {
			needsUpdate = value;
			Debug.Log("Leaderboard.NeedsUpdate has been set to " + value);
		}
	}

	// Party state is NOT currently being sent from Rabbit MQ.
	// Leverage game state.  If "idle", display idle screen.  Else, display the leaderboard.
	private string partyState {
		get => game.CurrentGameStateData is not null
			? game.CurrentGameStateData.Data.GameStatus
			: "idle";
	}

	// To access the game state we need
	// Reference to the Game class instance in the game
	private Game game;

	// Start is called before the first frame update
	void Start() {
		showControl = FindObjectOfType<ShowControl>();
		showControl.RegisterConsumer(leaderboardQueueName, leaderboardRoutingKey, HandleLeaderboardMessage);

		game = FindObjectOfType<Game>();

		// Set Flow Controller Component
		flowControllerComponent = flowController.GetComponent<FlowController>();

		// Destroy all existing player cells
		//DestroyCells(playerCellParentTop24);
		//DestroyCells(playerCellParentRemainder);

		//TriggerFlowControl(); "idle"
	}

	void OnDestroy() {
		//showControl.UnRegisterConsumer(HandleLeaderboardMessage);
	}

	void Update() {
		if (needsUpdate || game.NeedsUpdate) {
			TriggerFlowControl();
			switch (partyState) {
				case "idle":
					break;
				default:
					SetPlayerCells();
					break;
			}

			NeedsUpdate = false;
		}
	}

	//void HandleLeaderboardMessage(object obj, BasicDeliverEventArgs eventArgs) {
	void HandleLeaderboardMessage(IBasicConsumer obj, BasicDeliverEventArgs eventArgs) {
		// NOTE: Unity is single-threaded and does not allow direct game object updates from delegates.
		// Update a variable we can read from the main thread instead.
		// https://answers.unity.com/questions/1327573/how-do-i-resolve-get-isactiveandenabled-can-only-b.html
		CurrentLeaderboardData = showControl.GetMessageData<LeaderboardMessage>(eventArgs);;
	}

	private void DestroyCells(GameObject parent) {
		foreach (Transform child in parent.transform) {
			// NOTE: The following all say that the PlayerCell instances are all not prefab
			// so we cannot use them to determine the objects to delete:
			// - PrefabUtility.GetPrefabAssetType(child)
			// - PrefabUtility.GetPrefabParent(child) != null && PrefabUtility.GetPrefabObject(child) != null
			// - PrefabUtility.IsPartOfAnyPrefab(child)
			// - PrefabUtility.GetPrefabInstanceStatus(child)
			// For now, use the name instead.  This way, we can keep the child Header object
			// while deleting everything else whose name begins with "PlayerCell...".
			if (child.name.StartsWith("PlayerCell")) {
				//Debug.Log("Will destroy: " + child.name);
				Destroy(child.gameObject);
			}
			else {
				//Debug.Log("Will NOT destroy: " + child.name);
			}
		}
	}

	private void SetPlayerCells() {
		if (CurrentLeaderboardData is not null && CurrentLeaderboardData.Data is not null) {
			// Destroy all existing player cells
			DestroyCells(playerCellParentTop24);
			DestroyCells(playerCellParentRemainder);

			foreach (var entry in CurrentLeaderboardData.Data.Leaderboard) {
				Debug.Log($"Updating data for rank: {entry.Rank}");
				SetPlayerCell(entry);
			}
		}
	}

	private void SetPlayerCell(LeaderboardEntry leaderboardEntry) {
		playerCellParent = (leaderboardEntry.Rank <= 24) ? playerCellParentTop24 : playerCellParentRemainder;

		var newCell = Instantiate(playerCell, new Vector3 (0,0,0), Quaternion.identity, playerCellParent.transform);

		/*var rankObject = showControl.FindChildWithTag(newCell, "PlayerRank");
		var nameObject = showControl.FindChildWithTag(newCell, "PlayerName");
		var scoreObject = showControl.FindChildWithTag(newCell, "PlayerScore");
		rankObject.GetComponent<TextMeshProUGUI>().text = leaderboardEntry.Rank.ToString();
		nameObject.GetComponent<TextMeshProUGUI>().text = leaderboardEntry.PlayerName;
		scoreObject.GetComponent<TextMeshProUGUI>().text = leaderboardEntry.Score.ToString();*/

		foreach (Transform child in newCell.transform) {
			var textComponent = child.GetComponent<TextMeshProUGUI>();
			if (child.name == "Rank") {
				textComponent.text = leaderboardEntry.Rank.ToString();
			} else if (child.name == "Player Name") {
				textComponent.text = leaderboardEntry.PlayerName;
			} else if (child.name == "Score") {
				textComponent.text = leaderboardEntry.Score.ToString();
			}
		}
		Debug.Log($"Adding to {playerCellParent.name}: {leaderboardEntry.Rank.ToString()} {leaderboardEntry.PlayerName} {leaderboardEntry.Score.ToString()}");

		//newCell.transform.SetParent(playerCellParent.transform);

	}

	private void TriggerFlowControl() {
		Debug.Log($"Triggering flow control for party state: {partyState}");

		// The UIButton component uses FlowController to animate navigation to another page.
		// We want to leverage the flow already set up on the button component by invoking it.
		// HOWEVER, DOOZY BUTTON EVENTS DO NOT SEEM TO WORK FROM SCRIPT AT ALL :-(
		// We are currently duplicating the events on the buttons here.
		// If the events in the scene changes, it will also have to manually updated here!
		switch (partyState) {
			case "idle":
				flowControllerComponent.SetActiveNodeByName("Idle");
				break;
			default:
				flowControllerComponent.SetActiveNodeByName("Leaderboard");
				break;
		}
	}
}
