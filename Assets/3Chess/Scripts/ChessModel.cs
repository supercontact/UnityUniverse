using System;
using System.Collections.Generic;
using System.Linq;

public class ChessModel {

    private static readonly IntVector3[] comboDirections = IntVector3.allDirections.Where(dir => dir.x * 4 + dir.y * 2 + dir.z > 0).ToArray();

    public IntVector3 size { get; private set; }
    public int comboLength { get; private set; }
    public int scoreToWin { get; private set; }
    public int currentPlayer { get; private set; }
    public int player1Score { get; private set; }
    public int player2Score { get; private set; }

    public bool isFinished { get; protected set; } = false;
    public bool isInitiated { get; protected set; } = false;

    public delegate void GameInitHandler();
    public event GameInitHandler onGameInit;

    public delegate void GameStartHandler(int firstPlayer);
    public event GameStartHandler onGameStart;

    public delegate void GameFinishHandler(int playerWon);
    public event GameFinishHandler onGameFinish;

    public delegate void NextTurnHandler(int nextPlayer);
    public event NextTurnHandler onNextTurn;

    public delegate void ChessStateChangeHandler(IntVector3 location, int newChessState);
    public event ChessStateChangeHandler onChessStateChange;

    public delegate void PlayerScoreChangeHandler(int player, int newScore);
    public event PlayerScoreChangeHandler onPlayerScoreChange;

    public delegate void ComboHandler(IntVector3 startLocation, IntVector3 direction, int length);
    public event ComboHandler onCombo;

    protected int[,,] board;
   
    public virtual void Init(IntVector3 size, int comboLength = 3, int scoreToWin = 3, int firstPlayer = 0) {
        isInitiated = true;
        this.size = size;
        this.comboLength = comboLength;
        this.scoreToWin = scoreToWin;
        onGameInit?.Invoke();
        Reset(firstPlayer);
    }

    public virtual void Restart(int firstPlayer = 0) {
        Reset(firstPlayer);
    }

    private void Reset(int firstPlayer = 0) {
        isFinished = false;
        if (firstPlayer == 0) {
            currentPlayer = new Random().Next() % 2 + 1;
        } else if (firstPlayer <= 2) {
            currentPlayer = firstPlayer;
        } else {
            throw new Exception($"Player {firstPlayer} is not a valid player!");
        }

        board = new int[size.x, size.y, size.z];

        SetPlayerScore(1, 0);
        SetPlayerScore(2, 0);
        onGameStart?.Invoke(currentPlayer);
        foreach (IntVector3 location in new IntBox(size).allPointsInside) {
            onChessStateChange?.Invoke(location, 0);
        }
    }

    public virtual void PlaceChess(int player, IntVector3 location) {
        if (currentPlayer != player) {
            throw new Exception($"It's not player {player}'s turn to place chess!");
        }
        if (GetChess(location) != 0) {
            throw new Exception($"There is already a chess placed at location {location}!");
        }
        SetChess(location, player);
        UpdateScore(location);

        if (player1Score >= scoreToWin) {
            isFinished = true;
            onGameFinish?.Invoke(1);
        } else if (player2Score >= scoreToWin) {
            isFinished = true;
            onGameFinish?.Invoke(2);
        } else if (BoardIsFull()) {
            isFinished = true;
            onGameFinish?.Invoke(player1Score > player2Score ? 1 : player1Score < player2Score ? 2 : 0);
        } else {
            currentPlayer = 3 - currentPlayer;
            onNextTurn?.Invoke(currentPlayer);
        }
    }

    public int GetChess(IntVector3 location) {
        return board[location.x, location.y, location.z];
    }

    protected void SetChess(IntVector3 location, int chessState) {
        board[location.x, location.y, location.z] = chessState;
        onChessStateChange?.Invoke(location, chessState);
    }

    public int GetPlayerScore(int player) {
        return player == 1 ? player1Score : player2Score;
    }

    protected void SetPlayerScore(int player, int newScore) {
        if (player == 1) {
            player1Score = newScore;
        } else {
            player2Score = newScore;
        }
        onPlayerScoreChange?.Invoke(player, newScore);
    }

    protected void UpdateScore(IntVector3 newChessLocation) {
        int player = GetChess(newChessLocation);
        foreach (IntVector3 dir in comboDirections) {
            int positiveConnection = 0;
            int negativeConnection = 0;
            IntVector3 loc = newChessLocation + dir;
            while (loc < size && loc >= IntVector3.zero) {
                if (GetChess(loc) == player) {
                    positiveConnection++;
                    loc += dir;
                } else {
                    break;
                }
            }
            loc = newChessLocation - dir;
            while (loc < size && loc >= IntVector3.zero) {
                if (GetChess(loc) == player) {
                    negativeConnection++;
                    loc -= dir;
                } else {
                    break;
                }
            }
            int length = positiveConnection + negativeConnection + 1;
            if (positiveConnection < comboLength && negativeConnection < comboLength && length >= comboLength) {
                SetPlayerScore(player, GetPlayerScore(player) + 1);
                onCombo?.Invoke(newChessLocation - dir * negativeConnection, dir, length);
            }
        }
    }

    protected bool BoardIsFull() {
        return board.Cast<int>().All(c => c > 0);
    }
}
