﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ChessMain : MonoBehaviour {

    public static ChessMain instance;

    private readonly List<Type> packetTypes = new List<Type> {
        typeof(PlayerInfoRequest),
        typeof(InitChessRequest),
        typeof(RestartChessRequest),
        typeof(PlaceChessRequest),
        typeof(ObserveCameraControlRequest),
        typeof(GuideLineDisplayRequest),
        typeof(ObserveModeRequest),
    };

    public ChessControl chessControl;
    public GameObject startMenu;
    public InputField ipInput;
    public InputField nameInput;
    public GameConfigUI gameConfigPanel;
    public GameObject serverWaitMessage;
    public GameObject clientWaitMessage;

    public string playerName;
    public string opponentName;

    private Server server;
    private Client client;

    private IntVector3 size;
    private int comboLength;
    private int scoreToWin;

    private void Awake() {
        instance = this;

        int typeId = 1;
        foreach (Type packetType in packetTypes) {
            NetworkRegistry.packetTypeById.Add(typeId++, packetType);
        }
    }

    private void OnDestroy() {
        instance = null;

        NetworkRegistry.packetTypeById.Clear();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void StartLocalPlay() {
        startMenu.SetActive(false);
        gameConfigPanel.gameObject.SetActive(true);
        gameConfigPanel.onStart += StartLocalPlayWithConfig;
    }

    private void StartLocalPlayWithConfig(IntVector3 size, int comboLength, int scoreToWin) {
        gameConfigPanel.gameObject.SetActive(false);
        gameConfigPanel.onStart -= StartLocalPlayWithConfig;
        chessControl.Init(ChessControl.Mode.Local, "", "", size, comboLength, scoreToWin);
    }

    public void HostGame() {
        playerName = nameInput.text;
        if (playerName == "") {
            playerName = "Black Player";
        }
        startMenu.SetActive(false);
        gameConfigPanel.gameObject.SetActive(true);
        gameConfigPanel.onStart += HostGameWithConfig;
    }

    public void HostGameWithConfig(IntVector3 size, int comboLength, int scoreToWin) {
        this.size = size;
        this.comboLength = comboLength;
        this.scoreToWin = scoreToWin;
        gameConfigPanel.gameObject.SetActive(false);
        gameConfigPanel.onStart -= HostGameWithConfig;
        server = new Server();
        server.onClientConnected += OnConnectedOnServer;
        server.Start(12019);
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

    public void Reconfigure() {
        if (gameConfigPanel.gameObject.activeSelf) {
            gameConfigPanel.gameObject.SetActive(false);
            gameConfigPanel.onStart -= ReconfigureWithConfig;
        } else {
            gameConfigPanel.gameObject.SetActive(true);
            gameConfigPanel.onStart += ReconfigureWithConfig;
        }
    } 

    public void ReconfigureWithConfig(IntVector3 size, int comboLength, int scoreToWin) {
        gameConfigPanel.gameObject.SetActive(false);
        gameConfigPanel.onStart -= ReconfigureWithConfig;
        chessControl.Init(chessControl.mode, playerName, opponentName, size, comboLength, scoreToWin);
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
            chessControl.Init(ChessControl.Mode.Server, playerName, opponentName, size, comboLength, scoreToWin);
            ChatManager.instance.SetServer(server, playerName);
        } else {
            chessControl.Init(ChessControl.Mode.Client, playerName, opponentName);
            ChatManager.instance.SetClient(client, playerName);
        }
        serverWaitMessage.SetActive(false);
        clientWaitMessage.SetActive(false);
    }
}
