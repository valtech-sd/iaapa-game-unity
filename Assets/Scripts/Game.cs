using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour {
	[Header("Message Bus")]
	[SerializeField] string gameStateQueueName = "qu.iaapa-unity-gamestate";
	[SerializeField] string gameStateRoutingKey = "#.gamestate";

	// We need this because we cannot easily find objects outside of main thread from the handler
	[Header("Game Run Text Objects")]

	[SerializeField] GameObject[] runPlayerNames;
	[SerializeField] GameObject[] runPlayerScores;
	[SerializeField] bool[] runPlayerHasCurrentTurns;

	// We need this because we cannot easily find objects outside of main thread from the handler
	[Header("Game End Text Objects")]
	[SerializeField] GameObject[] endPlayerNames;
	[SerializeField] GameObject[] endPlayerScores;

	private List<TextMeshProUGUI> runPlayerNameTextComponents = new List<TextMeshProUGUI>();
	private List<TextMeshProUGUI> runPlayerScoreTextComponents = new List<TextMeshProUGUI>();
	private List<TextMeshProUGUI> endPlayerNameTextComponents = new List<TextMeshProUGUI>();
	private List<TextMeshProUGUI> endPlayerScoreTextComponents = new List<TextMeshProUGUI>();

	private TextMeshProUGUI playerNameTextElement;
	private TextMeshProUGUI playerScoreTextElement;

	private ShowControl showControl;

	private GameStateMessage currentGameStateData;
	private bool needsUpdate = false;


	// Start is called before the first frame update
	void Start() {
		showControl = FindObjectOfType<ShowControl>();
		showControl.RegisterConsumer(gameStateQueueName, gameStateRoutingKey, HandleGameStateMessage);

		// We need to find and set components here because will get "can only be run in main thread" error from handler
		addTextComponentToList(runPlayerNames, runPlayerNameTextComponents);
		addTextComponentToList(runPlayerScores, runPlayerScoreTextComponents);
		addTextComponentToList(endPlayerNames, endPlayerNameTextComponents);
		addTextComponentToList(endPlayerScores, endPlayerScoreTextComponents);
	}
	void OnDestroy() {
		showControl.UnRegisterConsumer(HandleGameStateMessage);
	}

	void Update() {
		if (needsUpdate) {
			Debug.Log($"Current game state: {currentGameStateData.Data.GameStatus}");
			switch (currentGameStateData.Data.GameStatus) {
				case "idle":
					break;
				case "load":
					setPlayerDataForSeats(currentGameStateData.Data);
					break;
				case "run":
					setPlayerDataForSeats(currentGameStateData.Data);
					break;
				case "end":
					setPlayerDataForSeats(currentGameStateData.Data);
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
					//Debug.Log($"Deserialized Message: {gameRun}");

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

	private void setPlayerDataForSeats(GameStateData gameData) {
		foreach (var seat in gameData.Locations) {
			Debug.Log($"Updating data for seat: {seat}");
			setPlayerDataForSeat(gameData.GameStatus, seat);
		}
	}

	private void setPlayerDataForSeat(string gameState, Seat playerData) {
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

	private void addTextComponentToList(GameObject[] gameObjects, List<TextMeshProUGUI> textComponents) {
		foreach (var obj in gameObjects) {
			textComponents.Add(obj.GetComponent<TextMeshProUGUI>());
		}
	}
}
