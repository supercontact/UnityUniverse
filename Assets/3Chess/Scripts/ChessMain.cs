﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ChessMain : MonoBehaviour {

    private readonly List<Type> packetTypes = new List<Type> {
        typeof(PlayerInfoRequest),
        typeof(InitChessRequest),
        typeof(RestartChessRequest),
        typeof(PlaceChessRequest),
        typeof(ObserveCameraControlRequest),
        typeof(ObserveModeRequest),
    };

    public ChessControl chessControl;
    public GameObject startMenu;
    public InputField ipInput;
    public InputField nameInput;
    public GameObject serverWaitMessage;
    public GameObject clientWaitMessage;

    public string playerName;
    public string opponentName;

    private Server server;
    private Client client;

    private void Awake() {
        int typeId = 1;
        foreach (Type packetType in packetTypes) {
            NetworkRegistry.packetTypeById.Add(typeId++, packetType);
        }
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnDestroy() {
        NetworkRegistry.packetTypeById.Clear();
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        Debug.Log(e.ExceptionObject.ToString());
    }

    public void StartLocalPlay() {
        chessControl.Init(ChessControl.Mode.Local, "", "", new IntVector3(3, 3, 3), 3);
        startMenu.SetActive(false);
    }

    public void HostGame() {
        playerName = nameInput.text;
        if (playerName == "") {
            playerName = "Black Player";
        }
        server = new Server();
        server.onClientConnected += OnConnectedOnServer;
        server.Start(12019);
        startMenu.SetActive(false);
        serverWaitMessage.SetActive(true);
    }

    public void ConnectToServer() {
        playerName = nameInput.text;
        if (playerName == "") {
            playerName = "White Player";
        }
        string ip = ipInput.text;
        client = new Client();
        client.onConnected += OnConnectedOnClient;
        client.Connect(ip, 12019);
        startMenu.SetActive(false);
        clientWaitMessage.SetActive(true);
    }

    private void OnConnectedOnServer(int clientId) {
        server.SendToClient(clientId, new PlayerInfoRequest(playerName));
        server.ListenFromClient<PlayerInfoRequest>(clientId, OnOpponentNameRecieved);
    }

    private void OnConnectedOnClient() {
        client.SendToServer(new PlayerInfoRequest(playerName));
        client.ListenFromServer<PlayerInfoRequest>(OnOpponentNameRecieved);
    }

    private void OnOpponentNameRecieved(PlayerInfoRequest request) {
        opponentName = request.playerName;
        if (server != null) {
            chessControl.Init(ChessControl.Mode.Server, playerName, opponentName, new IntVector3(3, 3, 3), 3);
            ChatManager.instance.SetServer(server, playerName);
        } else {
            chessControl.Init(ChessControl.Mode.Client, playerName, opponentName);
            ChatManager.instance.SetClient(client, playerName);
        }
        serverWaitMessage.SetActive(false);
        clientWaitMessage.SetActive(false);
    }
}