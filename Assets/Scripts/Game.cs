using Doozy.Runtime.Nody;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.Reactor.Animators;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour {
	readonly string _gameStateQueueName = "qu.iaapa-unity-gamestate";
	readonly string _gameStateRoutingKey = "#.gamestate";
	readonly string _turnStartQueueName = "qu.iaapa-unity-turnstart";
	readonly string _turnStartRoutingKey = "#.turnstart";

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
	[Header("Slides")]
	[SerializeField] private GameObject[] slides;

	[Header("Timer Container")]
	[SerializeField] private GameObject timerContainer;

	// We need this because we are not allowed to find objects outside of main thread from the RabbitMQ handler

	[Header("Player Seats")]
	[SerializeField] private GameObject[] playerSeats;

	private int _activeSlot = 0; //1-6 for player slots, 0 to clear

	// Number of slides counting down to game start (1 second per slide)
	// Use 5 to include the "Go!" slide
	// Use 4 to exclude the "Go!" slide
	readonly int _numberOfCountdownSlides = 4;

	// List of objects we activate/deactivate
	readonly List<GameObject> _playerColors = new List<GameObject>();
	readonly List<GameObject> _playerColorOutlines = new List<GameObject>();
	readonly List<GameObject> _playerPointsBg = new List<GameObject>();
	readonly List<GameObject> _rank1 = new List<GameObject>();
	readonly List<GameObject> _rank2 = new List<GameObject>();
	readonly List<GameObject> _rank3 = new List<GameObject>();

	// List of color animators
	readonly List<ColorAnimator> _playerColorAnimators = new List<ColorAnimator>();

	// List of player texts we update
	readonly List<TextMeshProUGUI> _playerNameTextComponents = new List<TextMeshProUGUI>();
	readonly List<TextMeshProUGUI> _playerScoreTextComponents = new List<TextMeshProUGUI>();

	// The current player text element to update
	private TextMeshProUGUI _playerNameTextElement;
	private TextMeshProUGUI _playerScoreTextElement;

	// Reference to the ShowControl class instance in the game
	private ShowControl _showControl;

	// List of received GameStateMessage messages that need processing
	readonly List<(UInt64,GameStateMessage)> _gameStateMessages = new List<(ulong, GameStateMessage)>();

	// The latest game state message from Rabbit MQ
	private GameStateMessage _currentGameStateMessage;
	private GameStateMessage currentGameStateMessage {
		get => _currentGameStateMessage;
		set {
			if (currentGameStateMessage is not null && currentGameStateMessage.Data is not null) {
				_previousGameState = currentGameStateMessage.Data.GameStatus;
			}

			_currentGameStateMessage = value;
			Debug.Log("currentGameStateMessage has been set to " + value);
		}
	}

	private string currentGameState  => (currentGameStateMessage is not null && currentGameStateMessage.Data is not null)
			? currentGameStateMessage.Data.GameStatus
			: "idle";

	private string _previousGameState;

	// List of received TurnStartMessage messages that need processing
	readonly List<(UInt64,TurnStartMessage)> _turnStartMessages = new List<(ulong, TurnStartMessage)>();

	// The latest turn start message from Rabbit MQ
	private TurnStartMessage _currentTurnStartMessage;

	// Reference to Timer class instance in the game
	private Timer _timer;

	// Start is called before the first frame update
	void Start() {
		_showControl = FindObjectOfType<ShowControl>();
		_showControl.RegisterConsumer(_gameStateQueueName, _gameStateRoutingKey, HandleGameStateMessage);
		_showControl.RegisterConsumer(_turnStartQueueName, _turnStartRoutingKey, HandleTurnStartMessage);

		// We need to find and set components here because will get "can only be run in main thread" error from RabbitMQ handler

		// Set Needed Components
		SetComponents();

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
	}
	void OnDestroy() {
		//showControl.UnRegisterConsumer(HandleGameStateMessage);
	}

	void Update() {
		if (_showControl.isConnected) {
			// Process game state messages from Unity main thread
			if (_gameStateMessages.Count > 0) {
				currentGameStateMessage = _gameStateMessages[0].Item2;

				Debug.Log("PreviousGameState: " + _previousGameState);
				Debug.Log("CurrentGameState: " + currentGameState);
				if (_previousGameState != currentGameState || _previousGameState is null) TriggerFlowControl();

				switch (currentGameState) {
					case "idle":
						break;
					default:
						SetPlayerDataForSeats();
						break;
				}

				_showControl.channel.BasicAck(_gameStateMessages[0].Item1, false);
				_gameStateMessages.RemoveAt(0);
			}

			// Process turn start messages from Unity main thread
			if (_turnStartMessages.Count > 0) {
				_currentTurnStartMessage = _turnStartMessages[0].Item2;
				SwitchActivePlayer();
				SetPlayerDataForSeats();

				_showControl.channel.BasicAck(_turnStartMessages[0].Item1, false);
				_turnStartMessages.RemoveAt(0);
			}
		}

	}
	//void HandleGameStateMessage(object obj, BasicDeliverEventArgs eventArgs) {
	void HandleGameStateMessage(IBasicConsumer obj, BasicDeliverEventArgs eventArgs) {
		// NOTE: Unity is single-threaded and does not allow direct game object updates from delegates.
		// Update a variable we can read from the main thread instead.
		// https://answers.unity.com/questions/1327573/how-do-i-resolve-get-isactiveandenabled-can-only-b.html
		_gameStateMessages.Add((eventArgs.DeliveryTag, _showControl.GetMessageData<GameStateMessage>(eventArgs)));
	}


	void HandleTurnStartMessage(IBasicConsumer obj, BasicDeliverEventArgs eventArgs) {
		// NOTE: Unity is single-threaded and does not allow direct game object updates from delegates.
		// Update a variable we can read from the main thread instead.
		// https://answers.unity.com/questions/1327573/how-do-i-resolve-get-isactiveandenabled-can-only-b.html
		_turnStartMessages.Add((eventArgs.DeliveryTag, _showControl.GetMessageData<TurnStartMessage>(eventArgs)));
	}

	private void ResetSeats() {
		timerContainer.SetActive(false);
		slides[0].SetActive(true);
		slides[1].SetActive(false);
		slides[2].SetActive(false);
		slides[3].SetActive(false);
		slides[4].SetActive(false);
		slides[5].SetActive(false);

		for (var i = 0; i < playerSeats.Length; i++) {
			Debug.Log($"Resetting seat index {i}");
			_playerNameTextElement = _playerNameTextComponents[i];
			_playerScoreTextElement = _playerScoreTextComponents[i];
			_playerNameTextElement.text = "";
			_playerScoreTextElement.text = "0";

			DeactivateSeat(i);
			_playerColorOutlines[i].SetActive(false);
			_playerPointsBg[i].SetActive(false);

			_rank1[i].SetActive(false);
			_rank2[i].SetActive(false);
			_rank3[i].SetActive(false);

			playerSeats[i].SetActive(false);
		}
	}

	private void SetPlayerDataForSeats() {
		if (currentGameStateMessage is not null && currentGameStateMessage.Data is not null  &&
		currentGameStateMessage.Data.Locations is not null) {
			for (var i=0; i<playerSeats.Length; i++) {
				Seat seat = currentGameStateMessage.Data.Locations.Find(l => l.Location == i + 1);
				if (seat != default(Seat)) {
					Debug.Log($"Updating data for seat index {i}: {seat}");
					if (currentGameState == "end") {
						List<Seat> winners = currentGameStateMessage.Data.Locations.OrderByDescending(s => s.Score).ToList();
						var rankIndex = winners.FindIndex(w => w.Location == i + 1);
						Debug.Log("Seat " + (i+1) + " has rank " + (rankIndex + 1));
						SetPlayerDataForSeat(seat, i, rankIndex);
					}
					else {
						SetPlayerDataForSeat(seat, i);
					}
				}
				else {
					ClearPlayerDataForSeat(i);
				}
			}
		}
	}


	private void ClearPlayerDataForSeat(int index) {
		playerSeats[index].SetActive(false);
		_playerColorOutlines[index].SetActive(false);
	}

	private void SetPlayerDataForSeat(Seat playerData, int index, int rankIndex=-1) {
		// Use index passed in to determine which slot to populate with data
		_playerNameTextElement = _playerNameTextComponents[index];
		_playerScoreTextElement = _playerScoreTextComponents[index];

		if (!_playerNameTextElement.Equals(null)) {
			if (currentGameState == "load") {
				playerSeats[index].SetActive(true);
				_playerColorOutlines[index].SetActive(true);
			}
			_playerNameTextElement.text = playerData.PlayerName;
		}

		if (!_playerScoreTextElement.Equals(null)) {
			_playerScoreTextElement.text = playerData.Score.ToString();
			//if (playerData.Score > 0) {
			if (currentGameState == "run") {
				_playerPointsBg[index].SetActive(true);
			}
		}

		if (currentGameState == "end") {
			if (rankIndex == 0) {
				_rank1[index].SetActive(true);
			}
			else if (rankIndex == 1) {
				_rank2[index].SetActive(true);
			}
			else if (rankIndex == 2) {
				_rank3[index].SetActive(true);
			}
		}
	}
		private void Countdown() {
		if (currentGameStateMessage is not null && currentGameStateMessage.Data is not null
			//&& currentGameStateMessage.Timestamp != default(long) &&
			&& currentGameStateMessage.Data.GameStartTimestamp.HasValue
			// && currentGameStateMessage.Data.GameEndTimestamp.HasValue
			&& currentGameStateMessage.Data.GameLength.HasValue
		) {
			ActivateAllSeats();

			var nowInMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();

			var countdownToGameStartInSeconds =
				((currentGameStateMessage.Data.GameStartTimestamp ?? default (long)) - nowInMs) / 1000f;
			Debug.Log("countdownToGameStartInSeconds: " + countdownToGameStartInSeconds);

			//var gameLengthInSeconds = ((currentGameStateMessage.Data.GameEndTimestamp ?? default(long)) -
			//(currentGameStateMessage.Data.GameStartTimestamp ?? default(long))) / 1000f;
			var gameLengthInSeconds = (currentGameStateMessage.Data.GameLength ?? default(long)) / 1000f;
			Debug.Log("gameLengthInSeconds: " + gameLengthInSeconds);

			// Countdown to Game Start
			/*var delayBeforeCountdownToStart = countdownToGameStartInSeconds - _numberOfCountdownSlides;
			if (delayBeforeCountdownToStart > 0) {
				StartCoroutine(Utilities.ExecuteAfterTime(delayBeforeCountdownToStart, () => CountdownToGameStart()));
			}
			else {*/
				CountdownToGameStart();
			//}

			// Countdown to Game End
			StartCoroutine(Utilities.ExecuteAfterTime(countdownToGameStartInSeconds, () => {
				// Activate timer
				timerContainer.SetActive(true);
				_timer = FindObjectOfType<Timer>();

				CountdownToGameEnd(gameLengthInSeconds);
			}));
		}
		else {
			Debug.LogError("Missing required data for game countdown in " + currentGameStateMessage);
		}
	}
	 private void CountdownToGameStart() {
		Debug.Log("Counting down to game start");
		//_timer.StartTimer(countdownToGameStartInSeconds);
		slides[0].SetActive(false);
		for (var i = 1; i <= _numberOfCountdownSlides; i++) {
			slides[i].SetActive(true);
		}
		//ActivateAllSeats();
	}

	private void CountdownToGameEnd(float gameLengthInSeconds) {
		Debug.Log("Counting down to game end");
		_timer.StartTimer(gameLengthInSeconds);

		/*for (var i = 0; i < playerSeats.Length; i++) {
			_playerPointsBg[i].SetActive(true);
		}*/
	}
	private void ActivateAllSeats() {
		Debug.Log($"playerSeats.Length: {playerSeats.Length}");
		for (var i = 0; i < playerSeats.Length; i++) {
			if (playerSeats[i].activeSelf) ActivateSeat(i);
		}
	}
	private void ActivateSeat(int index) {
		Debug.Log($"Activating seat {index}");
		_playerColors[index].SetActive(true);
		_playerColorAnimators[index].Play();
	}
	private void DeactivateSeat(int index) {
		Debug.Log($"Deactivating seat {index}");
		_playerColors[index].SetActive(false);
		//if (_playerColorAnimators[index].isActiveAndEnabled) _playerColorAnimators[index].Reverse();
		_playerNameTextComponents[index].color = Color.black;
	}

	private void SetComponents() {
		Debug.Log("Setting components for all " + playerSeats.Length + " seats");
		for (var i=0; i < playerSeats.Length; i++) {
			var seatObj = playerSeats[i];
			//Debug.Log("Seat " + i + " has " + seatObj.transform.childCount + "children") ;
			foreach (Transform child in seatObj.transform) {
				//Debug.Log("Found "+ child.name +" for seat " + i);
				if (child.name == "PlayerName") {
					_playerNameTextComponents.Add(child.GetComponent<TextMeshProUGUI>());
					_playerColorAnimators.Add(child.GetComponent<ColorAnimator>());
				}
				else if (child.name == "PlayerColor") {
					_playerColors.Add(child.gameObject);
				}
				else if (child.name == "PlayerColorOutline") {
					_playerColorOutlines.Add(child.gameObject);
				}
				else if (child.name == "PointsBG") {
					_playerPointsBg.Add(child.gameObject);
					_playerScoreTextComponents.Add(child.GetComponentInChildren<TextMeshProUGUI>());
				}
				else if (child.name == "Rank1") {
					_rank1.Add(child.gameObject);
				}
				else if (child.name == "Rank2") {
					_rank2.Add(child.gameObject);
				}
				else if (child.name == "Rank3") {
					_rank3.Add(child.gameObject);
				}
			}
		}
	}

	private void TriggerFlowControl() {
		Debug.Log($"Changing game state to {currentGameState}");

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
				ResetSeats();

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
				Countdown();

				//runButtonComponent.onClickEvent.Invoke();
				_flowControllerComponent.SetActiveNodeByName("Game");
				break;
			case "end":
				timerContainer.SetActive(false);
				ActivateAllSeats();
				//endButtonComponent.onClickEvent.Invoke();
				_flowControllerComponent.SetActiveNodeByName("Game");
				break;
		}
	}

	private void SwitchActivePlayer() {
		if (_currentTurnStartMessage is not null && _currentTurnStartMessage.Data is not null &&
			currentGameStateMessage is not null && currentGameStateMessage.Data is not null && currentGameStateMessage.Data
			.Locations is not null) {

			string activePlayerId = _currentTurnStartMessage.Data.PlayerId;
			Debug.Log("activePlayerId: " + activePlayerId);
			int activeIndex = currentGameStateMessage.Data.Locations.FindIndex(i => i.PlayerId == activePlayerId);
			_activeSlot = currentGameStateMessage.Data.Locations[activeIndex].Location;

			for (var i=0; i < playerSeats.Length; i++) {
				if (i == _activeSlot - 1) {
					ActivateSeat(i);
				}
				else {
					DeactivateSeat(i);
				}
			}
		}
	}
}
