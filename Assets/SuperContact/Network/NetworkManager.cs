using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public class NetworkManager : MonoBehaviour {

    private static NetworkManager instance;
    public static NetworkManager GetInstance() {
        if (instance == null) {
            throw new Exception("You need to attach a NetworkEventUpdate script to the scene!");
        }
        return instance;
    }

    public Server server;
    public Client client;

    private ConcurrentQueue<Action> eventQueue = new ConcurrentQueue<Action>();
    private ConcurrentQueue<ActionOnPacket> packetEventQueue = new ConcurrentQueue<ActionOnPacket>();

    public void QueueAction(Action action) {
        eventQueue.Enqueue(action);
    }
    public void QueueAction(Action<object> action, object packet) {
        packetEventQueue.Enqueue(new ActionOnPacket(action, packet));
    }

    private void Awake() {
        instance = this;
    }

    private void OnDestroy() {
        instance = null;
    }

    private void Update() {
        while (eventQueue.TryDequeue(out Action act)) {
            act();
        }
        while (packetEventQueue.TryDequeue(out ActionOnPacket act)) {
            act.action(act.packet);
        }
    }
}

public class ActionOnPacket {
    public Action<object> action;
    public object packet;

    public ActionOnPacket(Action<object> action, object packet) {
        this.action = action;
        this.packet = packet;
    }
}