using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShowControl : MonoBehaviour {
	public enum MessageType {
		GameState,
		Leaderboard
	}

	[Serializable]
	public enum PartyState {
		Introductions,
		Qualifiers,
		PreTournament,
		Tournament,
		PostTournament,
		FreePlays
	}
	[Serializable]
	public enum GameState {
		Idle,
		OnBoarding,
		Ready,
		Running,
		Ended
	}

	[Header("States")]
	[SerializeField] PartyState partyState;
	public PartyState CurrentPartyState {
		get => partyState;
		private set {
			partyState = value;
			Debug.Log($"CurrentPartyState set to {value}");
			UpdateSecondaryView();
		}
	}
	[SerializeField] GameState gameState;
	public GameState CurrentGameState {
		get =>  gameState;
		private set {
			gameState = value;
			Debug.Log($"CurrentGameState set to {value}");
			UpdateMainView();
		}
	}

	/**
	 * Sets the current party state.
	 * NOTE: Using string because an enum argument is not supported for Unity button click handlers.
	 * https://stackoverflow.com/questions/50131837/unity-is-not-showing-public-methods-in-the-event-field
	 */
	public void SetPartyState(string partyStateString) {
		Enum.TryParse(partyStateString, out PartyState partyState);
		CurrentPartyState = partyState;
	}

	/**
	 * Sets the current game state.
	 * NOTE: Using string because an enum argument is not supported for Unity button click handlers.
	 * https://stackoverflow.com/questions/50131837/unity-is-not-showing-public-methods-in-the-event-field
	 */
	public void SetGameState(string gameStateString) {
		Enum.TryParse(gameStateString, out GameState gameState);
		CurrentGameState = gameState;
	}

	private GameObject mainCanvas;
	private GameObject secondaryCanvas;

	private ConnectionFactory factory;
	private IConnection connection;
	private EventingBasicConsumer consumer;

	private IModel channel;
	public IModel Channel {
		get => channel;
		private set {
			channel = value;
			Debug.Log($"Channel set to {value}");
		}
	}
	[Header("Message Bus")]
	[SerializeField] string hostname = "localhost";
	[SerializeField] string username = "rmqadmin";
	[SerializeField] string password = "MTDXTnpuF0jGwDA";
	[SerializeField] string exchangeName = "ex.iaapa-topic";
	public string ExchangeName {
		get => exchangeName;
		private set {
			exchangeName = value;
			Debug.Log($"MessageBusExchangeName set to {value}");
		}
	}

	private void Awake() {
		mainCanvas = GameObject.FindWithTag("MainCanvas");
		secondaryCanvas = GameObject.FindWithTag("SecondaryCanvas");

		// Connect to message bus
		factory = new ConnectionFactory() { HostName = hostname, UserName = username, Password = password};
		connection = factory.CreateConnection();
		channel = connection.CreateModel();

		// Make sure the message bus exchange we need exists
		channel.ExchangeDeclare(exchangeName, "topic");
	}

	private void UpdateMainView() {
		// NOTE: Assumes View Name contains the GameState name
		ShowView(mainCanvas, CurrentGameState.ToString());
	}
	private void UpdateSecondaryView() {
		// NOTE: Some views are shared by different PartyStates.
		switch (CurrentPartyState) {
			case PartyState.Qualifiers:
				ShowView(secondaryCanvas,"HighScoresAll");
				break;
			case PartyState.PreTournament:
				ShowView(secondaryCanvas,"HighScoresTop30");
				break;
			case PartyState.Tournament:
				ShowView(secondaryCanvas,"Tournament");
				break;
			case PartyState.PostTournament:
				ShowView(secondaryCanvas,"Tournament");
				break;
			case PartyState.FreePlays:
				ShowView(secondaryCanvas,"Tournament");
				break;
			default:
				ShowView(secondaryCanvas,"Idle");
				break;
		}
	}

	private void ShowView(GameObject canvas, string viewToShow) {
		//Debug.Log(canvas.transform.childCount);

		foreach (Transform child in canvas.transform) {
			//Debug.Log(child.name);
			//UIView uiView = child.GetComponent<UIView>();
			if (child.name.Contains(viewToShow)) {
				//uiView.Show();
				child.gameObject.SetActive(true);
			}
			else {
				//uiView.Hide();
				child.gameObject.SetActive(false);
			}
		}
	}

	//public void RegisterConsumer(string messageBusQueueName, string messageBusRoutingKey, EventHandler<BasicDeliverEventArgs> handler) {
	public void RegisterConsumer(string messageBusQueueName, string messageBusRoutingKey, BasicDeliverEventHandler
	handler) {
		// Make sure exchange is bound to queue
		Debug.Log($"Binding '{messageBusQueueName}' queue to '{exchangeName}' exchange with '{messageBusRoutingKey}' routing key.");
		Channel.QueueBind(queue: messageBusQueueName,
			exchange: exchangeName, routingKey: messageBusRoutingKey);

		Debug.Log($"Waiting for messages from '{messageBusQueueName}' queue.");

		consumer = new EventingBasicConsumer(Channel);
		consumer.Received += handler;
		Channel.BasicConsume(queue: messageBusQueueName,
			//autoAck: true,
			noAck: true,
			consumer: consumer);
	}

	//public void UnRegisterConsumer(EventHandler<BasicDeliverEventArgs> handler) {
	public void UnRegisterConsumer(BasicDeliverEventHandler handler) {
		consumer.Received -= handler;
	}

	public GameObject FindChildWithTag(GameObject parent, string tag) {
		GameObject child = null;

		foreach(Transform transform in parent.transform) {
			if(transform.CompareTag(tag)) {
				child = transform.gameObject;
				break;
			}
		}

		return child;
	}
	public List<GameObject> FindChildrenWithTag(GameObject parent, string tag) {
		List<GameObject> children = new List<GameObject>();

		foreach(Transform transform in parent.transform) {
			if(transform.CompareTag(tag)) {
				children.Add(transform.gameObject);
				break;
			}
		}

		return children;
	}

	public GameObject FindChildWithName(GameObject obj, string name) {
		Transform trans = obj.transform;
		Transform childTrans = trans.Find(name);
		if (childTrans != null) {
			return childTrans.gameObject;
		} else {
			return null;
		}
	}

}


/**
 * NOTE(S):
 * - This project uses .NET standard 2.1
 *		- See Unity Editor > Edit > Project Settings > Player > Other Settings > Configuration
 *		- https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/unity-scripting-upgrade#enabling-the-net-4x-scripting-runtime-in-unity
 *
 * - CymaticLabs created a [Unity wrapper for the .NET RabbitMQ client](https://github.com/CymaticLabs/Unity3D.Amqp)
 * However, we are not using any of their sample scripts or actual wrapper methods because it is for an outdated version of Unity.
 *
 * - Instead, we have added just [their DLLs](https://github.com/CymaticLabs/Unity3D.Amqp/tree/master/unity/CymaticLabs.UnityAmqp/Assets/CymaticLabs/Amqp/Plugins) as plugins
 * as per https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/unity-scripting-upgrade
 *
 * - Using the latest (6.0) .NET RabbitMQ client DLL directly:
 *		- requires also adding additional libs:
 *			- System.Runtime.CompilerServices.Unsafe
 *			- System.Threading.Channels
 *		- requires updating some method signatures
 *		- still seems to present the threading issues the old CymaticLabs wrapper tried to fix
 */
