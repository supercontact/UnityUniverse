using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPieceControl : MonoBehaviour {

    public float observeChessSize = 0.7f;
    public float observePlaceholderSize = 0.2f;
    public float highlightPlaceholderSize = 1.25f;

    public IntVector3 location;
    public GameObject placeholder;
    public GameObject player1Chess;
    public GameObject player2Chess;

    public ChessControl chessControl;

    private int currentState;
    private bool isHighlighted;
    private SpringValue animatedChessSize = new SpringValue(1, 500, 30);
    private SpringValue animatedPlaceHolderSize = new SpringValue(1, 1000, 75);
    private SpringValue animatedRotationAngle = new SpringValue(0, 500, 30);

    public void SetState(int chessState) {
        currentState = chessState;
        placeholder.SetActive(chessState == 0);
        player1Chess.SetActive(chessState == 1);
        player2Chess.SetActive(chessState == 2);
    }

    public void ShowCombo() {
        animatedRotationAngle.value = 90;
    }

    public void OnPointerEnter() {
        if (chessControl.IsMyTurn()) {
            isHighlighted = true;
        }
    }

    public void OnPointerExit() {
        isHighlighted = false;
    }

    public void OnPointerClick() {
        if (chessControl.IsMyTurn() && currentState == 0 && !chessControl.observeMode) {
            chessControl.PlaceChess(location);
        }
    }

    private void Update() {
        animatedPlaceHolderSize.targetValue = chessControl.observeMode ? observePlaceholderSize : isHighlighted ? highlightPlaceholderSize : 1f;
        animatedPlaceHolderSize.Evolve(Time.deltaTime);
        placeholder.transform.localScale = Vector3.one * animatedPlaceHolderSize.value;

        animatedChessSize.targetValue = chessControl.observeMode ? observeChessSize : 1f;
        animatedChessSize.Evolve(Time.deltaTime);
        Vector3 scale = Vector3.one * animatedChessSize.value;
        player1Chess.transform.localScale = scale;
        player2Chess.transform.localScale = scale;

        animatedRotationAngle.Evolve(Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, animatedRotationAngle.value, 0);
    }
}