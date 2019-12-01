using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChatManager : MonoBehaviour {

    public static ChatManager instance;

    public string playerName = "Local Player";
    public event Action<ChatRequest> onNewMessage;

    private Server server;
    private Client client;
    private bool isInitiated = false;

    private void Awake() {
        instance = this;
        NetworkRegistry.packetTypeById.Add(10001, typeof(ChatRequest));
    }

    private void OnDestroy() {
        instance = null;
        NetworkRegistry.packetTypeById.Remove(10001);
    }

    public void SetServer(Server server, string playerName) {
        if (isInitiated) {
            throw new Exception("Already initiated!");
        }
        this.server = server;
        this.playerName = playerName;
        server.ListenFromAllClients<ChatRequest>(OnReceiveChatMessage);
        isInitiated = true;
    }

    public void SetClient(Client client, string playerName) {
        if (isInitiated) {
            throw new Exception("Already initiated!");
        }
        this.client = client;
        this.playerName = playerName;
        client.ListenFromServer<ChatRequest>(OnReceiveChatMessage);
        isInitiated = true;
    }

    public void SendGlobalMessage(string message) {
        var request = new ChatRequest(playerName, message);
        if (server != null) {
            server.SendToAllClients(request);
        } else if (client != null) {
            client.SendToServer(request);
        }
        onNewMessage?.Invoke(request);
    }

    private void OnReceiveChatMessage(ChatRequest request, Connection connection) {
        if (server != null) {
            server.clientConnections.Values.Where(c => c != connection).ForEach(c => c.Send(request));
        }
        onNewMessage?.Invoke(request);
    }
}
