using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShowControl : MonoBehaviour {
	[Header("Message Bus")]
	[SerializeField] string hostname = "localhost";
	[SerializeField] string username = "rmqadmin";
	[SerializeField] string password = "MTDXTnpuF0jGwDA";
	[SerializeField] string exchangeName = "ex.iaapa-topic";

	private ConnectionFactory factory;
	private IConnection connection;
	private EventingBasicConsumer consumer;

	private IModel channel;

	private void Awake() {
		// Connect to message bus
		factory = new ConnectionFactory() { HostName = hostname, UserName = username, Password = password};
		connection = factory.CreateConnection();
		channel = connection.CreateModel();

		// Make sure the message bus exchange we need exists
		channel.ExchangeDeclare(exchangeName, "topic", false, true, null);
	}

	//public void RegisterConsumer(string messageBusQueueName, string messageBusRoutingKey, EventHandler<BasicDeliverEventArgs> handler) {
	public void RegisterConsumer(string messageBusQueueName, string messageBusRoutingKey, BasicDeliverEventHandler
	handler) {
		// Make sure exchange is bound to queue
		Debug.Log($"Binding '{messageBusQueueName}' queue to '{exchangeName}' exchange with '{messageBusRoutingKey}' routing key.");
		channel.QueueBind(queue: messageBusQueueName,
			exchange: exchangeName, routingKey: messageBusRoutingKey);

		Debug.Log($"Waiting for messages from '{messageBusQueueName}' queue.");

		consumer = new EventingBasicConsumer(channel);
		consumer.Received += handler;
		channel.BasicConsume(queue: messageBusQueueName,
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
