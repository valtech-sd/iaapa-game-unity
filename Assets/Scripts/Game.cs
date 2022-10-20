using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Doozy.Runtime.Nody;
using Doozy.Runtime.UIManager.Components;
using RabbitMQ.Client;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour {
	[Header("Message Bus")]
	[SerializeField] string gameStateQueueName = "qu.iaapa-unity-gamestate";
	[SerializeField] string gameStateRoutingKey = "#.gamestate";

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

	[Header("Game Run Current Turns")]
	[SerializeField] bool[] runPlayerHasCurrentTurns;

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
		set {
			currentGameStateData = value;
			Debug.Log("CurrentGameStateData has been set!");
		}
	}

	// Whether the text elements need updating due to a new game state message
	private bool needsUpdate = false;
	public bool NeedsUpdate {
		get => needsUpdate;
		set {
			needsUpdate = value;
			Debug.Log("NeedsUpdate has been set!");
		}
	}

	// Reference to Timer class instance in the game
	private Timer timer;

	// Start is called before the first frame update
	void Start() {
		showControl = FindObjectOfType<ShowControl>();
		showControl.RegisterConsumer(gameStateQueueName, gameStateRoutingKey, HandleGameStateMessage);

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
	}
	void OnDestroy() {
		//showControl.UnRegisterConsumer(HandleGameStateMessage);
	}

	void Update() {
		if (needsUpdate) {
			TriggerFlowControl(currentGameStateData.Data.GameStatus);
			switch (currentGameStateData.Data.GameStatus) {
				case "idle":
					break;
				case "load":
					SetPlayerDataForSeats(currentGameStateData.Data);
					break;
				case "run":
					SetPlayerDataForSeats(currentGameStateData.Data);
					break;
				case "end":
					SetPlayerDataForSeats(currentGameStateData.Data);
					break;
				default:
					break;
			}

			needsUpdate = false;
		}
	}
	//void HandleGameStateMessage(object obj, BasicDeliverEventArgs eventArgs) {
	void HandleGameStateMessage(IBasicConsumer obj, BasicDeliverEventArgs eventArgs) {
		var body = eventArgs.Body.ToArray();
		var message = Encoding.UTF8.GetString(body);
		var receivedRoutingKey = eventArgs.RoutingKey;
		var consumerTag = eventArgs.ConsumerTag;
		Debug.Log($"Consumer [{consumerTag}] received '{receivedRoutingKey}' message: '{message}'");

		if (message.Length > 0) {
			try {
				GameStateMessage gameStateMessage = JsonConvert.DeserializeObject<GameStateMessage>(message);

				if (!gameStateMessage.Equals(null) && gameStateMessage.Type == "gamestate") {
					//Debug.Log($"Deserialized Message: {gameStateMessage}");

					// NOTE: Unity is single-threaded and does not allow direct game object updates from delegates.
					// Update a variable we can read from the main thread instead.
					// https://answers.unity.com/questions/1327573/how-do-i-resolve-get-isactiveandenabled-can-only-b.html
					currentGameStateData = gameStateMessage;

					// Use a flag to let us know when we should update text objects.
					needsUpdate = true;
				}
				else {
					Debug.LogError("Null Deserialization Result");
				}
			}
			catch (Exception e) {
				Debug.LogError(e);
			}
		}
	}

	private void SetPlayerDataForSeats(GameStateData gameData) {
		foreach (var seat in gameData.Locations) {
			Debug.Log($"Updating data for seat: {seat}");
			SetPlayerDataForSeat(gameData.GameStatus, seat);
		}
	}

	private void SetPlayerDataForSeat(string gameState, Seat playerData) {
		// Use seat location and game state to determine which slot to populate with data
		if (gameState == "load" || gameState == "run") {
			playerNameTextElement = runPlayerNameTextComponents[playerData.Location - 1];
			playerScoreTextElement = runPlayerScoreTextComponents[playerData.Location - 1];
		} else if (gameState == "end") {
			playerNameTextElement = endPlayerNameTextComponents[playerData.Location - 1];
			playerScoreTextElement = endPlayerScoreTextComponents[playerData.Location - 1];
		}

		if (!playerNameTextElement.Equals(null)) playerNameTextElement.text = playerData.PlayerName;
		if (!playerScoreTextElement.Equals(null)) playerScoreTextElement.text = playerData.Score.ToString();
	}

	private void AddTextComponentToList(GameObject[] gameObjects, List<TextMeshProUGUI> textComponents) {
		foreach (var obj in gameObjects) {
			textComponents.Add(obj.GetComponent<TextMeshProUGUI>());
		}
	}

	private void TriggerFlowControl(string gameState) {
		Debug.Log($"Triggering flow control for game state: {currentGameStateData.Data.GameStatus}");

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
}
