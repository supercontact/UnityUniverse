using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessClientModel : ChessModel {

    public int localPlayer;
    public int opponentPlayer => 3 - localPlayer;

    private Client client;

    public ChessClientModel() {
        client = NetworkManager.GetInstance().client;
        client.ListenFromServer<InitChessRequest>(HandleServerInit);
        client.ListenFromServer<RestartChessRequest>(HandleServerReset);
        client.ListenFromServer<PlaceChessRequest>(HandleServerPlaceChess);
    }

    ~ChessClientModel() {
        client.UnlistenFromServer<InitChessRequest>(HandleServerInit);
        client.UnlistenFromServer<RestartChessRequest>(HandleServerReset);
        client.UnlistenFromServer<PlaceChessRequest>(HandleServerPlaceChess);
    }

    public override void PlaceChess(int player, IntVector3 location) {
        if (player != localPlayer) {
            throw new Exception($"You are controlling player {localPlayer}, cannot place chess for player {player}!");
        }
        base.PlaceChess(player, location);
        client.SendToServer(new PlaceChessRequest(location));
    }

    private void HandleServerInit(InitChessRequest request) {
        localPlayer = request.controlledPlayer;
        Init(request.size, request.comboLength, request.scoreToWin, request.firstPlayer);
    }

    private void HandleServerReset(RestartChessRequest request) {
        Restart(request.firstPlayer);
    }

    private void HandleServerPlaceChess(PlaceChessRequest request) {
        base.PlaceChess(opponentPlayer, request.location);
    }
}
