using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client {

    public Connection connection { get; private set; }
    public bool isConnected {
        get { return connection != null; }
    }

    public event Action onConnected;

    private TcpClient client;
    private IAsyncResult connectionTask;

    public Client() {
        NetworkManager.GetInstance().client = this;
    }

    public void Connect(string address, int port) {
        if (connectionTask != null) {
            throw new InvalidOperationException("Already connecting!");
        }
        if (isConnected) {
            throw new InvalidOperationException("Already connected!");
        }

        try {
            client = new TcpClient();
            client.NoDelay = true;
            connectionTask = client.BeginConnect(address, port, OnConnected, null);
        } catch (SocketException e) {
            Debug.Log(e);
            client.Close();
            client = null;
            connectionTask = null;
        }
    }

    public void Disconnect() {
        if (!isConnected && connectionTask == null) {
            throw new InvalidOperationException("Connection not started!");
        }
        connection.Close();
        connection = null;
        client = null;
        connectionTask = null;
    }

    public void SendToServer<T>(T packet) {
        connection.Send(packet);
    }

    public void ListenFromServer<T>(Action<T> listener) {
        connection.Listen(listener);
    }
    public void ListenFromServer<T>(Action<T, Connection> listener) {
        connection.Listen(listener);
    }

    public void UnlistenFromServer<T>(Action<T> listener) {
        connection.Unlisten(listener);
    }
    public void UnlistenFromServer<T>(Action<T, Connection> listener) {
        connection.Unlisten(listener);
    }

    private void OnConnected(IAsyncResult asyncResult) {
        // Handle error
        client.EndConnect(asyncResult);
        connection = new Connection(client, 0);
        connectionTask = null;
        Logging.Log("Connection to server established!");
        NetworkManager.GetInstance().QueueAction(() => { onConnected?.Invoke(); });
    }
}
