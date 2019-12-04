using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChessControl : MonoBehaviour {

    public enum Mode {
        Local,
        Server,
        Client,
    }
    public Mode mode;

    public float chessGap = 1f;
    public GameObject chessPiecePrefab;
    public GameObject mainUI;
    public Text player1ScoreHint;
    public Text player2ScoreHint;
    public Text player1ScoreBoard;
    public Text player2ScoreBoard;
    public Text player1sTurnMessage;
    public Text player2sTurnMessage;
    public Text player1Congratulation;
    public Text player2Congratulation;
    public Text drawMessage;
    public ObserveCamera mainCamera;
    public GuideLine guideLine;

    public ChessModel chessModel;
    public bool observeMode;

    private Connection opponent;

    private ChessPieceControl[,,] chessPieces;
    private List<int> playersControlled = new List<int>();
    private FocusableInput input = new FocusableInput();
    private float lastCameraDistance;
    private Quaternion lastCameraRotation;
    private float cameraRequestTimer;

    public void Init(Mode mode, string localPlayerName = "", string remotePlayerName = "", IntVector3 size = default, int comboLength = default, int scoreToWin = default) {
        ClearModel();

        this.mode = mode;
        if (mode == Mode.Server) {
            chessModel = new ChessServerModel(2);
            playersControlled = new List<int> { 2 };
            opponent = NetworkManager.GetInstance().server.clientConnections.Values.Single();
            opponent.Listen<ObserveCameraControlRequest>(HandleObserveCameraRequest);
            opponent.Listen<ObserveModeRequest>(HandleObserveModeRequest);
        } else if (mode == Mode.Client) {
            chessModel = new ChessClientModel();
            playersControlled = new List<int> { 1 };
            opponent = NetworkManager.GetInstance().client.connection;
            opponent.Listen<ObserveCameraControlRequest>(HandleObserveCameraRequest);
            opponent.Listen<ObserveModeRequest>(HandleObserveModeRequest);
        } else {
            chessModel = new ChessModel();
            playersControlled = new List<int> { 1, 2 };
        }

        UpdatePlayerNames(localPlayerName, remotePlayerName);
        mainUI.SetActive(true);

        chessModel.onGameInit += OnGameInit;
        chessModel.onGameStart += OnGameStart;
        chessModel.onGameFinish += OnGameFinish;
        chessModel.onNextTurn += OnNextTurn;
        chessModel.onChessStateChange += OnChessStateChange;
        chessModel.onPlayerScoreChange += OnPlayerScoreChange;
        chessModel.onCombo += OnCombo;

        if (mode != Mode.Client) {
            chessModel.Init(size, comboLength, scoreToWin);
        }
    }

    public bool HasFreeControl() {
        return chessModel == null || chessModel.isFinished || mode == Mode.Local || IsMyTurn(); 
    }

    public bool IsMyTurn() {
        return chessModel != null && !chessModel.isFinished && playersControlled.Contains(chessModel.currentPlayer); 
    }

    public void PlaceChess(IntVector3 location) {
        if (IsMyTurn()) {
            chessModel.PlaceChess(chessModel.currentPlayer, location);
        }
    }

    public void Restart() {
        if (mode != Mode.Client) {
            chessModel.Restart();
        }
    }

    private void ClearModel() {
        if (chessModel == null) return;

        chessModel.onGameInit -= OnGameInit;
        chessModel.onGameStart -= OnGameStart;
        chessModel.onGameFinish -= OnGameFinish;
        chessModel.onNextTurn -= OnNextTurn;
        chessModel.onChessStateChange -= OnChessStateChange;
        chessModel.onPlayerScoreChange -= OnPlayerScoreChange;
        chessModel.onCombo -= OnCombo;
        chessModel.Destroy();
        chessModel = null;  
    }

    private void ClearGameObjects() {
        if (chessPieces != null) {
            foreach (ChessPieceControl chessPiece in chessPieces) {
                Destroy(chessPiece.gameObject);
            }
            chessPieces = null;
        }
    }

    private void UpdatePlayerNames(string localPlayerName = "", string remotePlayerName = "") {
        string player1, player2;
        if (mode == Mode.Server) {
            player1 = remotePlayerName;
            player2 = localPlayerName;
        } else if (mode == Mode.Client) {
            player1 = localPlayerName;
            player2 = remotePlayerName;
        } else {
            player1 = "White Player";
            player2 = "Black Player";
        }
        player1ScoreHint.text = player1ScoreHint.text.Replace("{player1}", player1);
        player2ScoreHint.text = player2ScoreHint.text.Replace("{player2}", player2);
        player1sTurnMessage.text = player1sTurnMessage.text.Replace("{player1}", player1);
        player2sTurnMessage.text = player2sTurnMessage.text.Replace("{player2}", player2);
        player1Congratulation.text = player1Congratulation.text.Replace("{player1}", player1);
        player2Congratulation.text = player2Congratulation.text.Replace("{player2}", player2);
    }

    private void OnGameInit() {
        ClearGameObjects();

        IntVector3 size = chessModel.size;
        chessPieces = new ChessPieceControl[size.x, size.y, size.z];
        Vector3 corner = -0.5f * chessGap * (size - IntVector3.one);
        foreach (IntVector3 location in new IntBox(size).allPointsInside) {
            ChessPieceControl chessPiece = Instantiate(chessPiecePrefab).GetComponent<ChessPieceControl>();
            chessPieces[location.x, location.y, location.z] = chessPiece;
            chessPiece.transform.SetParent(transform, false);
            chessPiece.transform.position = corner + chessGap * location;
            chessPiece.location = location;
            chessPiece.chessControl = this;
        }
        guideLine.gameObject.SetActive(true);
        guideLine.transform.localScale = Vector3.one * chessGap * Mathf.Max(size.x, size.y, size.z) / 5;
        guideLine.Init(opponent);
    }

    private void OnGameStart(int firstPlayer) {
        player1Congratulation.gameObject.SetActive(false);
        player2Congratulation.gameObject.SetActive(false);
        drawMessage.gameObject.SetActive(false);

        OnNextTurn(firstPlayer);
    }

    private void OnGameFinish(int playerWon) {
        player1Congratulation.gameObject.SetActive(playerWon == 1);
        player2Congratulation.gameObject.SetActive(playerWon == 2);
        drawMessage.gameObject.SetActive(playerWon == 0);
        mainCamera.allowRotating = true;
        mainCamera.allowZooming = true;
        guideLine.controlsOther = false;
    }

    private void OnNextTurn(int nextPlayer) {
        player1sTurnMessage.gameObject.SetActive(nextPlayer == 1);
        player2sTurnMessage.gameObject.SetActive(nextPlayer == 2);
        if (!IsMyTurn()) {
            observeMode = false;
        }
        mainCamera.allowRotating = IsMyTurn();
        mainCamera.allowZooming = IsMyTurn();
        guideLine.controlsOther = IsMyTurn();
    }

    private void OnChessStateChange(IntVector3 location, int newChessState) {
        chessPieces[location.x, location.y, location.z].SetState(newChessState);
    }

    private void OnPlayerScoreChange(int player, int newScore) {
        if (player == 1) {
            player1ScoreBoard.text = newScore.ToString();
        } else {
            player2ScoreBoard.text = newScore.ToString();
        }
    }

    private void OnCombo(IntVector3 startLocation, IntVector3 direction, int length) {
        for (int i = 0; i < length; i++) {
            IntVector3 location = startLocation + i * direction;
            chessPieces[location.x, location.y, location.z].ShowCombo();
        }
    }

    private void HandleObserveCameraRequest(ObserveCameraControlRequest request) {
        if (!IsMyTurn()) {
            mainCamera.targetDistance = request.targetDistance;
            mainCamera.targetRotation = request.targetRotation;
        }
    }

    private void HandleObserveModeRequest(ObserveModeRequest request) {
        if (!IsMyTurn()) {
            observeMode = request.isOn;
        }
    }

    private void Update() {
        if (chessModel != null && chessModel.isInitiated && input.GetKeyDown(KeyCode.R)) {
            Restart();
        }
        if (chessModel != null && chessModel.isInitiated && input.GetKeyDown(KeyCode.Q) && mode != Mode.Client) {
            ChessMain.instance.Reconfigure();
        }
        if (HasFreeControl()) {
            bool nextObserveMode = input.GetKey(KeyCode.Space);
            if (nextObserveMode != observeMode) {
                observeMode = nextObserveMode;
                if (IsMyTurn() && opponent != null) {
                    opponent.Send(new ObserveModeRequest(observeMode));
                }
            } 
        }
        if (IsMyTurn() && opponent != null) {
            cameraRequestTimer -= Time.deltaTime;
            if (cameraRequestTimer <= 0 && (mainCamera.targetDistance != lastCameraDistance || mainCamera.targetRotation != lastCameraRotation)) {
                lastCameraDistance = mainCamera.targetDistance;
                lastCameraRotation = mainCamera.targetRotation;
                opponent.Send(new ObserveCameraControlRequest(lastCameraDistance, lastCameraRotation));
                cameraRequestTimer = 0.1f;
            }
        }
    }

    private void OnDestroy() {
        ClearGameObjects();
    }
}
