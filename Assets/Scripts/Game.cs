using Doozy.Runtime.Nody;
using Doozy.Runtime.UIManager.Components;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour {
	[Header("Message Broker")]
	[SerializeField] string gameStateQueueName = "qu.iaapa-unity-gamestate";
	[SerializeField] string gameStateRoutingKey = "#.gamestate";
	[SerializeField] string turnStartQueueName = "qu.iaapa-unity-turnstart";
	[SerializeField] string turnStartRoutingKey = "#.turnstart";

	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	[Header("Flow Controller")]
	[SerializeField] GameObject flowController;

	private FlowController flowControllerComponent;

	/* WE ARE USING FLOW CONTROLLER ABOVE DIRECTLY INSTEAD BECAUSE DOOZY BUTTON EVENTS CANNOT BE INVOKED.
	THE ISSUE WITH THIS IS WE CAN'T AUTOMATICALLY CALL THE CORRECT FLOW IF THE BUTTON IS UPDATED.
	WE ALSO HAVE TO DUPLICATE ALL EVENTS CALLED BY THE BUTTON.
	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	[Header("Game State Button Objects")]
	[SerializeField] GameObject idleButton;
	[SerializeField] GameObject loadButton;
	[SerializeField] GameObject runButton;
	[SerializeField] GameObject endButton;

	// UI Button components from the above button objects
	private UIButton idleButtonComponent;
	private UIButton loadButtonComponent;
	private UIButton runButtonComponent;
	private UIButton endButtonComponent;
	*/

	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	[Header("Game Run Text Objects")]
	[SerializeField] GameObject[] runPlayerNames;
	[SerializeField] GameObject[] runPlayerScores;

	//[Header("Game Run Current Turns")]
	//[SerializeField] bool[] runPlayerHasCurrentTurns;
	[Header("Game Run Active Turn")]
	[SerializeField] int  activeSlot;
	public int ActiveSlot {
		get => activeSlot;
		private set {
			activeSlot = value;
			Debug.Log("ActiveSlot has been set to " + value);
		}
	}
	[SerializeField] Color activeColor = Color.green;
	[SerializeField] Color inactiveColor = Color.white;

	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	[Header("Game End Text Objects")]
	[SerializeField] GameObject[] endPlayerNames;
	[SerializeField] GameObject[] endPlayerScores;

	// List of text components from the above text objects
	private List<TextMeshProUGUI> runPlayerNameTextComponents = new List<TextMeshProUGUI>();
	private List<TextMeshProUGUI> runPlayerScoreTextComponents = new List<TextMeshProUGUI>();
	private List<TextMeshProUGUI> endPlayerNameTextComponents = new List<TextMeshProUGUI>();
	private List<TextMeshProUGUI> endPlayerScoreTextComponents = new List<TextMeshProUGUI>();

	// The current text element to update
	private TextMeshProUGUI playerNameTextElement;
	private TextMeshProUGUI playerScoreTextElement;

	// Reference to the ShowControl class instance in the game
	private ShowControl showControl;

	// The latest game state message from Rabbit MQ
	private GameStateMessage currentGameStateData;
	public GameStateMessage CurrentGameStateData {
		get => currentGameStateData;
		private set {
			currentGameStateData = value;
			Debug.Log("CurrentGameStateData has been set to " + value);
			NeedsUpdate = true;
		}
	}

	// Whether the text elements need updating due to a new game state message
	private bool needsUpdate = false;
	public bool NeedsUpdate {
		get => needsUpdate;
		private set {
			needsUpdate = value;
			Debug.Log("Game.NeedsUpdate has been set to " + value);
		}
	}

	private string gameState {
		get => (CurrentGameStateData is not null && CurrentGameStateData.Data is not null)
			? CurrentGameStateData.Data.GameStatus
			: "idle";
	}

	// The latest turn start message from Rabbit MQ
	private TurnStartMessage currentTurnStartData;
	public TurnStartMessage CurrentTurnStartData {
		get => currentTurnStartData;
		private set {
			currentTurnStartData = value;
			Debug.Log("CurrentTurnStartData has been set to " + value);
			turnSwitched = true;
		}
	}

	// Whether the player whose turn it is has switched
	private bool turnSwitched = false;
	private bool TurnSwitched {
		get => turnSwitched;
		set {
			turnSwitched = value;
			Debug.Log("TurnSwitched has been set to " + value);
		}
	}

	// Reference to Timer class instance in the game
	private Timer timer;

	// Start is called before the first frame update
	void Start() {
		showControl = FindObjectOfType<ShowControl>();
		showControl.RegisterConsumer(gameStateQueueName, gameStateRoutingKey, HandleGameStateMessage);
		showControl.RegisterConsumer(turnStartQueueName, turnStartRoutingKey, HandleTurnStartMessage);

		timer = FindObjectOfType<Timer>();

		// We need to find and set components here because will get "can only be run in main thread" error from RabbitMQ handler

		// Set Text Components
		AddTextComponentToList(runPlayerNames, runPlayerNameTextComponents);
		AddTextComponentToList(runPlayerScores, runPlayerScoreTextComponents);
		AddTextComponentToList(endPlayerNames, endPlayerNameTextComponents);
		AddTextComponentToList(endPlayerScores, endPlayerScoreTextComponents);

		/* WE ARE USING FLOW CONTROLLER DIRECTLY INSTEAD BECAUSE DOOZY BUTTON EVENTS CANNOT BE INVOKED.
		THE ISSUE WITH THIS IS WE CAN'T AUTOMATICALLY CALL THE CORRECT FLOW IF THE BUTTON IS UPDATED.
		WE ALSO HAVE TO DUPLICATE ALL EVENTS CALLED BY THE BUTTON.
		// Set Button Components
		idleButtonComponent = idleButton.GetComponent<UIButton>();
		loadButtonComponent = idleButton.GetComponent<UIButton>();
		runButtonComponent = idleButton.GetComponent<UIButton>();
		endButtonComponent = idleButton.GetComponent<UIButton>();
		*/

		// Set Flow Controller Component
		flowControllerComponent = flowController.GetComponent<FlowController>();

		//TriggerFlowControl(); //idle
	}
	void OnDestroy() {
		//showControl.UnRegisterConsumer(HandleGameStateMessage);
	}

	void Update() {
		if (needsUpdate) {
			TriggerFlowControl();
			switch (gameState) {
				case "idle":
					break;
				case "load":
					SetPlayerDataForSeats();
					break;
				case "run":
					SetPlayerDataForSeats();
					break;
				case "end":
					SetPlayerDataForSeats();
					break;
				default:
					break;
			}

			NeedsUpdate = false;
		}

		if (turnSwitched) {
			SwitchActivePlayer();
			TurnSwitched = false;
		}
	}
	//void HandleGameStateMessage(object obj, BasicDeliverEventArgs eventArgs) {
	void HandleGameStateMessage(IBasicConsumer obj, BasicDeliverEventArgs eventArgs) {
		// NOTE: Unity is single-threaded and does not allow direct game object updates from delegates.
		// Update a variable we can read from the main thread instead.
		// https://answers.unity.com/questions/1327573/how-do-i-resolve-get-isactiveandenabled-can-only-b.html
		CurrentGameStateData = showControl.GetMessageData<GameStateMessage>(eventArgs);
	}


	void HandleTurnStartMessage(IBasicConsumer obj, BasicDeliverEventArgs eventArgs) {
		// NOTE: Unity is single-threaded and does not allow direct game object updates from delegates.
		// Update a variable we can read from the main thread instead.
		// https://answers.unity.com/questions/1327573/how-do-i-resolve-get-isactiveandenabled-can-only-b.html
		CurrentTurnStartData = showControl.GetMessageData<TurnStartMessage>(eventArgs);
	}

	private void SetPlayerDataForSeats() {
		foreach (var seat in CurrentGameStateData.Data.Locations) {
			Debug.Log($"Updating data for seat: {seat}");
			SetPlayerDataForSeat(seat);
		}
	}

	private void SetPlayerDataForSeat(Seat playerData) {
		// Use seat location and game state to determine which slot to populate with data
		if (gameState == "load" || gameState == "run") {
			playerNameTextElement = runPlayerNameTextComponents[playerData.Location - 1];
			playerScoreTextElement = runPlayerScoreTextComponents[playerData.Location - 1];
		} else if (gameState == "end") {
			playerNameTextElement = endPlayerNameTextComponents[playerData.Location - 1];
			playerScoreTextElement = endPlayerScoreTextComponents[playerData.Location - 1];
		}

		var color = (playerData.Location == activeSlot) ? activeColor : inactiveColor;

		if (!playerNameTextElement.Equals(null)) {
			playerNameTextElement.text = playerData.PlayerName;
			playerNameTextElement.color = color;
		}

		if (!playerScoreTextElement.Equals(null)) {
			playerScoreTextElement.text = playerData.Score.ToString();
			playerScoreTextElement.color = color;
		}

	}

	private void AddTextComponentToList(GameObject[] gameObjects, List<TextMeshProUGUI> textComponents) {
		foreach (var obj in gameObjects) {
			textComponents.Add(obj.GetComponent<TextMeshProUGUI>());
		}
	}

	private void TriggerFlowControl() {
		Debug.Log($"Triggering flow control for game state: {gameState}");

		// The UIButton component uses FlowController to animate navigation to another page.
		// We want to leverage the flow already set up on the button component by invoking it.
		// HOWEVER, DOOZY BUTTON EVENTS DO NOT SEEM TO WORK FROM SCRIPT AT ALL :-(
		// We are currently duplicating the events on the buttons here.
		// If the events in the scene changes, it will also have to manually updated here!
		switch (gameState) {
			case "idle":
				/*
				Debug.Log("onClickEvent" + JsonConvert.SerializeObject(idleButtonComponent.onClickEvent));
				Debug.Log("onSelectedEvent" + JsonConvert.SerializeObject(idleButtonComponent.onSelectedEvent));
				//idleButtonComponent.onClickEvent.Invoke();
				//idleButtonComponent.Click(true);
				//idleButtonComponent.onSelectedEvent.Invoke();
				*/
				flowControllerComponent.SetActiveNodeByName("Idle");
				break;
			case "load":
				/*
				Debug.Log("onClickEvent" + JsonConvert.SerializeObject(loadButtonComponent.onClickEvent));
				Debug.Log("onSelectedEvent" + JsonConvert.SerializeObject(loadButtonComponent.onSelectedEvent));
				//loadButtonComponent.onClickEvent.Invoke();
				//loadButtonComponent.Click(true);
				//loadButtonComponent.onSelectedEvent.Invoke();
				*/
				flowControllerComponent.SetActiveNodeByName("Game");
				break;
			case "run":
				//
				//runButtonComponent.onClickEvent.Invoke();
				flowControllerComponent.SetActiveNodeByName("Game");
				timer.startTimer();
				break;
			case "end":
				//
				//endButtonComponent.onClickEvent.Invoke();
				flowControllerComponent.SetActiveNodeByName("GameEnd");
				break;
		}
	}

	private void SwitchActivePlayer() {
		string activePlayerId = currentTurnStartData.Data.PlayerId;
		int activeIndex = currentGameStateData.Data.Locations.FindIndex(i => i.PlayerId == activePlayerId);
		activeSlot = currentGameStateData.Data.Locations[activeIndex].Location;

		/*for (var i=0; i < runPlayerHasCurrentTurns.Length; i++) {
			runPlayerHasCurrentTurns[i] = (i == activeSlot - 1);
		}*/

		NeedsUpdate = true;
	}
}
