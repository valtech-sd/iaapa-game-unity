using Doozy.Runtime.Nody;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour {
	private string leaderboardQueueName = "qu.iaapa-unity-leaderboard";
	private string leaderboardRoutingKey = "#.leaderboard";

	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	// WE ARE USING FLOW CONTROLLER DIRECTLY INSTEAD OF ACCESSING THE BUTTON EVENTS DYNAMICALLY
	// BECAUSE DOOZY BUTTON EVENTS CANNOT BE INVOKED :-(
	[Header("Flow Controller")]
	[SerializeField] private GameObject flowController;

	private FlowController _flowControllerComponent;

	// Reference to the player cell prefab and parent containers
	[Header("Player Cell")]
	[SerializeField] private GameObject playerCell;
	[SerializeField] private GameObject playerCellParentTop;
	[SerializeField] private GameObject playerCellParentRemainder;

	private GameObject _playerCellParent;

	// Number for always-On e
	[Header("Top Players To Always Display")]
	[SerializeField] int numberOfTopPlayers = 12;


	// Reference to the ShowControl class instance in the game
	private ShowControl _showControl;

	// The latest leaderboard message from Rabbit MQ
	private LeaderboardMessage _currentLeaderboardMessage;
	private LeaderboardMessage currentLeaderboardMessage {
		get => _currentLeaderboardMessage;
		set {
			_currentLeaderboardMessage = value;
			Debug.Log("currentLeaderboardMessage has been set to " + value);
			needsUpdate = true;
		}
	}

	// Whether the text elements need updating due to a new game state message
	private bool _needsUpdate = false;
	private bool needsUpdate {
		get => _needsUpdate;
		set {
			_needsUpdate = value;
			Debug.Log("Leaderboard.needsUpdate has been set to " + value);
		}
	}


	// Party state is NOT currently being sent from Rabbit MQ.
	// Leverage game state.  If "idle", display idle screen.  Else, display the leaderboard.
	private string currentPartyState {
		get => _game.currentGameStateMessage is not null
			? _game.currentGameStateMessage.Data.GameStatus
			: "idle";
	}
	private string previousPartyState {
		get => _game.previousGameState;
	}

	// To access the game state we need
	// Reference to the Game class instance in the game
	private Game _game;

	// Start is called before the first frame update
	void Start() {
		_showControl = FindObjectOfType<ShowControl>();
		_showControl.RegisterConsumer(leaderboardQueueName, leaderboardRoutingKey, HandleLeaderboardMessage);

		_game = FindObjectOfType<Game>();

		// Set Flow Controller Component
		_flowControllerComponent = flowController.GetComponent<FlowController>();

		// Destroy all existing player cells
		DestroyCells(playerCellParentTop);
		DestroyCells(playerCellParentRemainder);

		//TriggerFlowControl(); "idle"
	}

	void OnDestroy() {
		//showControl.UnRegisterConsumer(HandleLeaderboardMessage);
	}

	void Update() {
		if (_showControl.isConnected && (needsUpdate || _game.needsUpdate)) {
			//Debug.Log("previousPartyState: " + previousPartyState);
			//Debug.Log("currentPartyState: " + currentPartyState);
			if (currentPartyState != previousPartyState  || previousPartyState is null) TriggerFlowControl();

			switch (currentPartyState) {
				/*case "idle":
					break;*/
				default:
					SetPlayerCells();
					break;
			}

			needsUpdate = false;
		}
	}

	//void HandleLeaderboardMessage(object obj, BasicDeliverEventArgs eventArgs) {
	void HandleLeaderboardMessage(IBasicConsumer obj, BasicDeliverEventArgs eventArgs) {
		// NOTE: Unity is single-threaded and does not allow direct game object updates from delegates.
		// Update a variable we can read from the main thread instead.
		// https://answers.unity.com/questions/1327573/how-do-i-resolve-get-isactiveandenabled-can-only-b.html
		currentLeaderboardMessage = _showControl.GetMessageData<LeaderboardMessage>(eventArgs);
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
		if (currentLeaderboardMessage is not null && currentLeaderboardMessage.Data is not null) {
			// Destroy all existing player cells
			DestroyCells(playerCellParentTop);
			DestroyCells(playerCellParentRemainder);

			foreach (var entry in currentLeaderboardMessage.Data.Leaderboard) {
				Debug.Log($"Updating data for rank: {entry.Rank}");
				SetPlayerCell(entry);
			}
		}
	}

	private void SetPlayerCell(LeaderboardEntry leaderboardEntry) {
		_playerCellParent = (leaderboardEntry.Rank <= numberOfTopPlayers) ? playerCellParentTop : playerCellParentRemainder;

		// Instantiate new playerCell prefab instance.
		// NOTE: With show control now in overlay mode while the rest are in camera, we need to make sure we are NOT instantiating in world space.
		// Even with position (0,0,0), z will get changed to -10800.
		//var newCell = Instantiate(playerCell, new Vector3 (0,0,0), Quaternion.identity,_playerCellParent.transform);
		var newCell = Instantiate(playerCell, _playerCellParent.transform, false);

		foreach (Transform child in newCell.transform) {
			var textComponent = child.GetComponent<TextMeshProUGUI>();
			if (child.name == "Rank") {
				textComponent.text = "#" + leaderboardEntry.Rank;
			} else if (child.name == "Player Name") {
				textComponent.text = leaderboardEntry.PlayerName;
			} else if (child.name == "Score") {
				textComponent.text = leaderboardEntry.Score.ToString();
			}
		}
		Debug.Log($"Added to {_playerCellParent.name}: {leaderboardEntry.Rank.ToString()} {leaderboardEntry.PlayerName} {leaderboardEntry.Score.ToString()}");
	}

	private void TriggerFlowControl() {
		Debug.Log($"Changing party state to {currentPartyState}");

		// The UIButton component uses FlowController to animate navigation to another page.
		// We want to leverage the flow already set up on the button component by invoking it.
		// HOWEVER, DOOZY BUTTON EVENTS DO NOT SEEM TO WORK FROM SCRIPT AT ALL :-(
		// We are currently duplicating the events on the buttons here.
		// If the events in the scene changes, it will also have to manually updated here!
		switch (currentPartyState) {
			/*case "idle":
				_flowControllerComponent.SetActiveNodeByName("Idle");
				break;*/
			default:
				_flowControllerComponent.SetActiveNodeByName("Leaderboard");
				break;
		}
	}
}
