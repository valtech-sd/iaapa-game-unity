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
	[SerializeField] private string gameStateQueueName = "qu.iaapa-unity-gamestate";
	[SerializeField] private string gameStateRoutingKey = "#.gamestate";
	[SerializeField] private string turnStartQueueName = "qu.iaapa-unity-turnstart";
	[SerializeField] private string turnStartRoutingKey = "#.turnstart";

	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	[Header("Flow Controller")]
	[SerializeField] private GameObject flowController;

	private FlowController _flowControllerComponent;

	/* WE ARE USING FLOW CONTROLLER ABOVE DIRECTLY INSTEAD BECAUSE DOOZY BUTTON EVENTS CANNOT BE INVOKED.
	THE ISSUE WITH THIS IS WE CAN'T AUTOMATICALLY CALL THE CORRECT FLOW IF THE BUTTON IS UPDATED.
	WE ALSO HAVE TO DUPLICATE ALL EVENTS CALLED BY THE BUTTON.
	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	[Header("Game State Button Objects")]
	[SerializeField] private GameObject idleButton;
	[SerializeField] private GameObject loadButton;
	[SerializeField] private GameObject runButton;
	[SerializeField] private GameObject endButton;

	// UI Button components from the above button objects
	private UIButton _idleButtonComponent;
	private UIButton _loadButtonComponent;
	private UIButton _runButtonComponent;
	private UIButton _endButtonComponent;
	*/

	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	[Header("Game Run Text Objects")]
	[SerializeField] private GameObject[] runPlayerNames;
	[SerializeField] private GameObject[] runPlayerScores;

	//[Header("Game Run Current Turns")]
	//[SerializeField] bool[] runPlayerHasCurrentTurns;
	[Header("Game Run Active Turn")]
	[SerializeField] private int _activeSlot = 0; //1-6 for player slots, 0 to clear
	public int activeSlot {
		get => _activeSlot;
		private set {
			_activeSlot = value;
			Debug.Log("activeSlot has been set to " + value);
		}
	}
	[SerializeField] private Color activeColor = Color.green;
	[SerializeField] private Color inactiveColor = Color.white;

	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler
	[Header("Game End Text Objects")]
	[SerializeField] private GameObject[] endPlayerNames;
	[SerializeField] private GameObject[] endPlayerScores;

	// List of text components from the above text objects
	private List<TextMeshProUGUI> _runPlayerNameTextComponents = new List<TextMeshProUGUI>();
	private List<TextMeshProUGUI> _runPlayerScoreTextComponents = new List<TextMeshProUGUI>();
	private List<TextMeshProUGUI> _endPlayerNameTextComponents = new List<TextMeshProUGUI>();
	private List<TextMeshProUGUI> _endPlayerScoreTextComponents = new List<TextMeshProUGUI>();

	// The current text element to update
	private TextMeshProUGUI _playerNameTextElement;
	private TextMeshProUGUI _playerScoreTextElement;

	// Reference to the ShowControl class instance in the game
	private ShowControl _showControl;

	// The latest game state message from Rabbit MQ
	private GameStateMessage _currentGameStateMessage;
	public GameStateMessage currentGameStateMessage {
		get => _currentGameStateMessage;
		private set {
			if (_currentGameStateMessage is not null && _currentGameStateMessage.Data is not null) {
				previousGameState = _currentGameStateMessage.Data.GameStatus;
			}

			_currentGameStateMessage = value;
			Debug.Log("currentGameStateMessage has been set to " + value);

			needsUpdate = true;
		}
	}

	// Whether the text elements need updating due to a new game state message
	private bool _needsUpdate = false;
	public bool needsUpdate {
		get => _needsUpdate;
		private set {
			_needsUpdate = value;
			Debug.Log("Game.needsUpdate has been set to " + value);
		}
	}

	public string currentGameState {
		get => (currentGameStateMessage is not null && currentGameStateMessage.Data is not null)
			? currentGameStateMessage.Data.GameStatus
			: "idle";
	}

	private string _previousGameState;
	public string previousGameState {
		get => _previousGameState;
		private set {
			_previousGameState = value;
			Debug.Log("previousGameState has been set to " + value);
		}
	}

	// The latest turn start message from Rabbit MQ
	private TurnStartMessage _currentTurnStartMessage;
	public TurnStartMessage currentTurnStartMessage {
		get => _currentTurnStartMessage;
		private set {
			_currentTurnStartMessage = value;
			Debug.Log("currentTurnStartData has been set to " + value);
			_turnSwitched = true;
		}
	}

	// Whether the player whose turn it is has switched
	private bool _turnSwitched = false;
	private bool turnSwitched {
		get => _turnSwitched;
		set {
			_turnSwitched = value;
			Debug.Log("turnSwitched has been set to " + value);
		}
	}

	// Reference to Timer class instance in the game
	private Timer _timer;

	// Start is called before the first frame update
	void Start() {
		_showControl = FindObjectOfType<ShowControl>();
		_showControl.RegisterConsumer(gameStateQueueName, gameStateRoutingKey, HandleGameStateMessage);
		_showControl.RegisterConsumer(turnStartQueueName, turnStartRoutingKey, HandleTurnStartMessage);

		_timer = FindObjectOfType<Timer>();

		// We need to find and set components here because will get "can only be run in main thread" error from RabbitMQ handler

		// Set Text Components
		AddTextComponentToList(runPlayerNames, _runPlayerNameTextComponents);
		AddTextComponentToList(runPlayerScores, _runPlayerScoreTextComponents);
		AddTextComponentToList(endPlayerNames, _endPlayerNameTextComponents);
		AddTextComponentToList(endPlayerScores, _endPlayerScoreTextComponents);

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
		_flowControllerComponent = flowController.GetComponent<FlowController>();

		//TriggerFlowControl(); //idle
	}
	void OnDestroy() {
		//showControl.UnRegisterConsumer(HandleGameStateMessage);
	}

	void Update() {
		if (_needsUpdate) {
			Debug.Log("PreviousGameState: " + previousGameState);
			Debug.Log("CurrentGameState: " + currentGameState);
			if (previousGameState != currentGameState || previousGameState is null) TriggerFlowControl();
			switch (currentGameState) {
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

			needsUpdate = false;
		}

		if (_turnSwitched) {
			SwitchActivePlayer();
			turnSwitched = false;
		}
	}
	//void HandleGameStateMessage(object obj, BasicDeliverEventArgs eventArgs) {
	void HandleGameStateMessage(IBasicConsumer obj, BasicDeliverEventArgs eventArgs) {
		// NOTE: Unity is single-threaded and does not allow direct game object updates from delegates.
		// Update a variable we can read from the main thread instead.
		// https://answers.unity.com/questions/1327573/how-do-i-resolve-get-isactiveandenabled-can-only-b.html
		currentGameStateMessage = _showControl.GetMessageData<GameStateMessage>(eventArgs);
	}


	void HandleTurnStartMessage(IBasicConsumer obj, BasicDeliverEventArgs eventArgs) {
		// NOTE: Unity is single-threaded and does not allow direct game object updates from delegates.
		// Update a variable we can read from the main thread instead.
		// https://answers.unity.com/questions/1327573/how-do-i-resolve-get-isactiveandenabled-can-only-b.html
		currentTurnStartMessage = _showControl.GetMessageData<TurnStartMessage>(eventArgs);
	}

	private void ClearPlayerDataForSeats() {
		for (var i = 0; i < runPlayerNames.Length; i++) {
			Debug.Log($"Clearing data for index: {i}");
			_playerNameTextElement = _runPlayerNameTextComponents[i];
			_playerScoreTextElement = _runPlayerScoreTextComponents[i];

			_playerNameTextElement.text = "";
			_playerNameTextElement.color = inactiveColor;

			_playerScoreTextElement.text = "0";
			_playerScoreTextElement.color = inactiveColor;
		}
	}

	private void SetPlayerDataForSeats() {
		if (currentGameStateMessage is not null && currentGameStateMessage.Data is not null  &&
		currentGameStateMessage.Data.Locations is not null) {
			// The slots players are displayed in the scene are either:
			// their seat location (during game load/run)
			// or their score ranking (during game end).
			// Sort the seat list in desired order before we update texts.
			List<Seat> seats = (currentGameState == "end")
				? currentGameStateMessage.Data.Locations.OrderByDescending(s => s.Score).ToList()
				: currentGameStateMessage.Data.Locations.OrderBy(s => s.Location).ToList();

			for (var i=0; i<seats.Count; i++) {
				Debug.Log($"Updating data for seat {i}: {seats[i]}");
				SetPlayerDataForSeat(seats[i], i);
			}
		}
	}

	private void SetPlayerDataForSeat(Seat playerData, int index) {
		// Use index passed in to determine which slot to populate with data
		if (currentGameState == "load" || currentGameState == "run") {
			_playerNameTextElement = _runPlayerNameTextComponents[index];
			_playerScoreTextElement = _runPlayerScoreTextComponents[index];
		} else if (currentGameState == "end") {
			_playerNameTextElement = _endPlayerNameTextComponents[index];
			_playerScoreTextElement = _endPlayerScoreTextComponents[index];
		}

		var color = (playerData.Location == _activeSlot && currentGameState != "end") ? activeColor : inactiveColor;

		if (!_playerNameTextElement.Equals(null)) {
			_playerNameTextElement.text = playerData.PlayerName;
			_playerNameTextElement.color = color;
		}

		if (!_playerScoreTextElement.Equals(null)) {
			_playerScoreTextElement.text = playerData.Score.ToString();
			_playerScoreTextElement.color = color;
		}

	}

	private void AddTextComponentToList(GameObject[] gameObjects, List<TextMeshProUGUI> textComponents) {
		foreach (var obj in gameObjects) {
			textComponents.Add(obj.GetComponent<TextMeshProUGUI>());
		}
	}

	private void TriggerFlowControl() {
		Debug.Log($"Triggering flow control for game state: {currentGameState}");

		// The UIButton component uses FlowController to animate navigation to another page.
		// We want to leverage the flow already set up on the button component by invoking it.
		// HOWEVER, DOOZY BUTTON EVENTS DO NOT SEEM TO WORK FROM SCRIPT AT ALL :-(
		// We are currently duplicating the events on the buttons here.
		// If the events in the scene changes, it will also have to manually updated here!
		switch (currentGameState) {
			case "idle":
				/*
				Debug.Log("onClickEvent" + JsonConvert.SerializeObject(idleButtonComponent.onClickEvent));
				Debug.Log("onSelectedEvent" + JsonConvert.SerializeObject(idleButtonComponent.onSelectedEvent));
				//idleButtonComponent.onClickEvent.Invoke();
				//idleButtonComponent.Click(true);
				//idleButtonComponent.onSelectedEvent.Invoke();
				*/
				_flowControllerComponent.SetActiveNodeByName("Idle");
				break;
			case "load":
				ClearPlayerDataForSeats();
				/*
				Debug.Log("onClickEvent" + JsonConvert.SerializeObject(loadButtonComponent.onClickEvent));
				Debug.Log("onSelectedEvent" + JsonConvert.SerializeObject(loadButtonComponent.onSelectedEvent));
				//loadButtonComponent.onClickEvent.Invoke();
				//loadButtonComponent.Click(true);
				//loadButtonComponent.onSelectedEvent.Invoke();
				*/
				_flowControllerComponent.SetActiveNodeByName("Game");
				break;
			case "run":
				//
				//runButtonComponent.onClickEvent.Invoke();
				_flowControllerComponent.SetActiveNodeByName("Game");
				_timer.startTimer();
				break;
			case "end":
				//
				//endButtonComponent.onClickEvent.Invoke();
				_flowControllerComponent.SetActiveNodeByName("GameEnd");
				break;
		}
	}

	private void SwitchActivePlayer() {
		if (currentTurnStartMessage is not null && currentTurnStartMessage.Data is not null &&
			currentGameStateMessage is not null && currentGameStateMessage.Data is not null && currentGameStateMessage.Data
			.Locations is not null) {

			string activePlayerId = currentTurnStartMessage.Data.PlayerId;
			Debug.Log("activePlayerId: " + activePlayerId);
			int activeIndex = currentGameStateMessage.Data.Locations.FindIndex(i => i.PlayerId == activePlayerId);
			activeSlot = currentGameStateMessage.Data.Locations[activeIndex].Location;

			/*for (var i=0; i < runPlayerHasCurrentTurns.Length; i++) {
				runPlayerHasCurrentTurns[i] = (i == activeSlot - 1);
			}*/

			needsUpdate = true;
		}
	}
}
