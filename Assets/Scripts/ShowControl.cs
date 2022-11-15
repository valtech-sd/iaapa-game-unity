using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ShowControl : MonoBehaviour {
	[SerializeField] private GameObject[] _winners;
	public GameObject[] winners {
		get => _winners;
		private set {
			_winners = value;
			Debug.Log("winners has been set to " + value);
		}
	}

	private bool _isConnected = false;
	public bool isConnected {
		get => _isConnected;
		private set {
			_isConnected = value;
			Debug.Log("isConnected has been set to " + value);
		}
	}

	private string _exchangeName;
	private ConnectionFactory _factory;
	private IConnection _connection;
	private EventingBasicConsumer _consumer;

	private IModel _channel;
	public IModel channel {
		get => _channel;
		private set  {
			_channel = value;
			Debug.Log("channel has been set to " + value);
		}
	}

	private void Awake() {
		try {
			// Get config settings from file
			// We are not serializing or setting these values here because it includes a password
			// that should not be committed to version control.
			string configFilePath = string.Format("{0}/{1}", Application.streamingAssetsPath, "config.json");
			string configString = Utilities.GetTextFromFile(configFilePath);
			Config config = JsonConvert.DeserializeObject<Config>(configString);

			if (config is not null && config.MessageBroker is not null) {
				_exchangeName = config.MessageBroker.Exchange;

				// Connect to message broker
				_factory = new ConnectionFactory() {
					HostName = config.MessageBroker.Host,
					UserName = config.MessageBroker.User,
					Password = config.MessageBroker.Pass,
				};
				_connection = _factory.CreateConnection();
				channel = _connection.CreateModel();

				// Make sure the message bus exchange we need exists
				channel.ExchangeDeclare(_exchangeName, "topic", false, false, null);

				isConnected = true;
			}
		}
		catch (Exception e) {
			Debug.LogError($"Message Broker is not connected: {e.Message}");
		}
	}

	//public void RegisterConsumer(string messageBrokerQueueName, string messageBrokerRoutingKey, EventHandler<BasicDeliverEventArgs> handler) {
	public void RegisterConsumer(string messageBrokerQueueName, string messageBrokerRoutingKey, BasicDeliverEventHandler
	handler) {
		if (!isConnected) {
			Debug.LogError("Missing required message broker connection. Unable to register as consumer of {messageBusQueueName}.");
			return;
		}

		// Only queue up 1 message at a time until an ack is sent
		channel.BasicQos(0, 1,false);

		var randomNumberGenerator = new System.Random();
		var randomizedQueueName = messageBrokerQueueName + "-" + randomNumberGenerator.Next();

		// Declare Queue
		channel.QueueDeclare(randomizedQueueName, false, false, true, null);

		// Make sure exchange is bound to queue
		Debug.Log($"Binding '{randomizedQueueName}' queue to '{_exchangeName}' exchange with '{messageBrokerRoutingKey}' routing key.");
		channel.QueueBind(queue: randomizedQueueName,
			exchange: _exchangeName, routingKey: messageBrokerRoutingKey);

		Debug.Log($"Waiting for messages from '{randomizedQueueName}' queue.");

		_consumer = new EventingBasicConsumer(_channel);
		_consumer.Received += handler;

		// Consume messages without auto ack
		channel.BasicConsume(queue: randomizedQueueName,
			//autoAck: false,
			noAck: false,
			consumer: _consumer);
	}

	//public void UnRegisterConsumer(EventHandler<BasicDeliverEventArgs> handler) {
	public void UnRegisterConsumer(BasicDeliverEventHandler handler) {
		_consumer.Received -= handler;
	}

	public T GetMessageData<T>(BasicDeliverEventArgs eventArgs) {
		var body = eventArgs.Body.ToArray();
		var message = Encoding.UTF8.GetString(body);
		var receivedRoutingKey = eventArgs.RoutingKey;
		var consumerTag = eventArgs.ConsumerTag;
		Debug.Log($"Consumer [{consumerTag}] received '{receivedRoutingKey}' message: '{message}'");

		if (message.Length > 0) {
			try {
				T messageData = JsonConvert.DeserializeObject<T>(message);
				//Debug.Log($"Deserialized Message: {messageData}");
				return messageData;
			}
			catch (Exception e) {
				Debug.LogError(e);
				throw;
			}
		}

		return default(T);
	}


	public GameObject FindChildWithTag(GameObject parent, string childTag) {
		GameObject child = null;

		foreach(Transform childTransform in parent.transform) {
			if(childTransform.CompareTag(childTag)) {
				child = childTransform.gameObject;
				break;
			}
		}

		return child;
	}
	public List<GameObject> FindChildrenWithTag(GameObject parent, string childTag) {
		List<GameObject> children = new List<GameObject>();

		foreach(Transform childTransform in parent.transform) {
			if(childTransform.CompareTag(childTag)) {
				children.Add(childTransform.gameObject);
				break;
			}
		}

		return children;
	}

	public GameObject FindChildWithName(GameObject obj, string childName) {
		Transform trans = obj.transform;
		Transform childTrans = trans.Find(childName);
		if (childTrans != null) {
			return childTrans.gameObject;
		} else {
			return null;
		}
	}

}
