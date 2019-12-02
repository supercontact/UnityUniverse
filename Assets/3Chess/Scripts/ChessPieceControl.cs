using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPieceControl : MonoBehaviour {

    public float observeChessSize = 0.7f;
    public float highlightPlaceholderSize = 1.25f;

    public IntVector3 location;
    public GameObject placeholder;
    public GameObject player1Chess;
    public GameObject player2Chess;

    public ChessControl chessControl;

    private int currentState;
    private bool isHighlighted;
    private float comboRotationDegrees;
    private SpringValue animatedChessSize = new SpringValue(1, 500, 30);
    private SpringValue animatedPlaceHolderSize = new SpringValue(1, 1000, 75);

    public void SetState(int chessState) {
        currentState = chessState;
        placeholder.SetActive(chessState == 0);
        player1Chess.SetActive(chessState == 1);
        player2Chess.SetActive(chessState == 2);
    }

    public void ShowCombo() {
        comboRotationDegrees = 90;
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
        if (chessControl.IsMyTurn() && currentState == 0) {
            chessControl.PlaceChess(location);
        }
    }

    private void Update() {
        animatedPlaceHolderSize.targetValue = chessControl.observeMode ? 0 : isHighlighted ? highlightPlaceholderSize : 1f;
        animatedPlaceHolderSize.Evolve(Time.deltaTime);
        placeholder.transform.localScale = Vector3.one * animatedPlaceHolderSize.value;

        animatedChessSize.targetValue = chessControl.observeMode ? observeChessSize : 1f;
        animatedChessSize.Evolve(Time.deltaTime);
        Vector3 scale = Vector3.one * animatedChessSize.value;
        player1Chess.transform.localScale = scale;
        player2Chess.transform.localScale = scale;

        if (comboRotationDegrees > 0) {
            comboRotationDegrees -= 180 * Time.deltaTime;
            comboRotationDegrees = Mathf.Max(comboRotationDegrees, 0);
            transform.rotation = Quaternion.Euler(0, comboRotationDegrees, 0);
        }
    }
}