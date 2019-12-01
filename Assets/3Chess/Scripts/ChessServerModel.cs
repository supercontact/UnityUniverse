using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChessServerModel : ChessModel {

    public int localPlayer;
    public int opponentPlayer => 3 - localPlayer;

    private Server server;
    private int clientId;

    public ChessServerModel(int localPlayer) {
        server = NetworkManager.GetInstance().server;
        this.localPlayer = localPlayer;
        clientId = server.clientConnections.Keys.Single();
        server.ListenFromClient<PlaceChessRequest>(clientId, HandleClientPlaceChess);
    }

    ~ChessServerModel() {
        server.UnlistenFromClient<PlaceChessRequest>(clientId, HandleClientPlaceChess);
    }

    public override void Init(IntVector3 size, int comboLength = 3, int firstPlayer = 0) {
        base.Init(size, comboLength, firstPlayer);
        server.SendToClient(clientId, new InitChessRequest(size, comboLength, opponentPlayer, currentPlayer));
    }

    public override void Restart(int firstPlayer = 0) {
        base.Restart(firstPlayer);
        server.SendToClient(clientId, new RestartChessRequest(currentPlayer));
    }

    public override void PlaceChess(int player, IntVector3 location) {
        if (player != localPlayer) {
            throw new Exception($"You are controlling player {localPlayer}, cannot place chess for player {player}!");
        }
        base.PlaceChess(player, location);
        server.SendToClient(clientId, new PlaceChessRequest(location));
    }

    private void HandleClientPlaceChess(PlaceChessRequest request) {
        base.PlaceChess(opponentPlayer, request.location);
    }
}
