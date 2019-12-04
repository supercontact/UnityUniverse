using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server {

    public bool isStarted {
        get { return server != null; }
    }

    public ConcurrentDictionary<int, Connection> clientConnections = new ConcurrentDictionary<int, Connection>();

    public delegate void ClientConnectedListener(int clientId);
    public event ClientConnectedListener onClientConnected;

    private TcpListener server;
    private int nextClientId = 1;
    private IAsyncResult clientListeningTask;

    public Server() {
        NetworkManager.GetInstance().server = this;
    }

    public void Start(int port) {
        CheckStarted(false);

        try {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Logging.Log("Server started!");
            clientListeningTask = server.BeginAcceptTcpClient(OnClientConnected, null);
        } catch (SocketException e) {
            Logging.Log($"Server failed to start: {e}");
            server.Stop();
            server = null;
        }
    }

    public void Stop() {
        CheckStarted(true);

        server.Stop();
        server = null;
        clientListeningTask = null;
        Logging.Log("Server stopped!");
    }

    public void EndAllClientConnections() {
        foreach (Connection connection in clientConnections.Values) {
            connection.Close();
        }
        clientConnections.Clear();
    }

    public void SendToClient<T>(int clientId, T packet) {
        clientConnections[clientId].Send(packet);
    }

    public void SendToAllClients<T>(T packet) {
        foreach (Connection connection in clientConnections.Values) {
            connection.Send(packet);
        }
    }

    public void ListenFromClient<T>(int clientId, Action<T> listener) {
        clientConnections[clientId].Listen(listener);
    }
    public void ListenFromClient<T>(int clientId, Action<T, Connection> listener) {
        clientConnections[clientId].Listen(listener);
    }

    public void UnlistenFromClient<T>(int clientId, Action<T> listener) {
        clientConnections[clientId].Unlisten(listener);
    }
    public void UnlistenFromClient<T>(int clientId, Action<T, Connection> listener) {
        clientConnections[clientId].Unlisten(listener);
    }

    public void ListenFromAllClients<T>(Action<T> listener) {
        foreach (Connection clientConnection in clientConnections.Values) {
            clientConnection.Listen(listener);
        }
    }
    public void ListenFromAllClients<T>(Action<T, Connection> listener) {
        foreach (Connection clientConnection in clientConnections.Values) {
            clientConnection.Listen(listener);
        }
    }

    public void UnlistenFromAllClients<T>(Action<T> listener) {
        foreach (Connection clientConnection in clientConnections.Values) {
            clientConnection.Unlisten(listener);
        }
    }
    public void UnlistenFromAllClients<T>(Action<T, Connection> listener) {
        foreach (Connection clientConnection in clientConnections.Values) {
            clientConnection.Unlisten(listener);
        }
    }

    private void OnClientConnected(IAsyncResult asyncResult) {
        // Handle error
        TcpClient client = server.EndAcceptTcpClient(asyncResult);
        // client.NoDelay = true;
        int clientId = nextClientId++;
        clientListeningTask = null;
        clientConnections[clientId] = new Connection(client, clientId);
        Logging.Log($"Client connection #{nextClientId - 1} established!");
        NetworkManager.GetInstance().QueueAction(() => { onClientConnected?.Invoke(clientId); });

        clientListeningTask = server.BeginAcceptTcpClient(OnClientConnected, null);
    }

    private void CheckStarted(bool started) {
        if (started && !isStarted) {
            throw new InvalidOperationException("Server not started!");
        } else if (!started && isStarted) {
            throw new InvalidOperationException("Server already started!");
        }
    }
}
