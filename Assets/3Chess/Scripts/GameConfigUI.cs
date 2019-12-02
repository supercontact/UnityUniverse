using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameConfigUI : MonoBehaviour {

    public InputField inputW;
    public InputField inputH;
    public InputField inputD;
    public InputField comboLengthInput;
    public Toggle fillAllSpacesToggle;
    public Toggle fixedScoreToggle;
    public InputField fixedScoreInput;
    public Button startButton;

    public delegate void ConfigHandler(IntVector3 size, int comboLength, int scoreToWin);
    public event ConfigHandler onStart;

    public void OnToggleEndCondition(bool fixedScore) {
        fixedScoreInput.interactable = fixedScore;
    }

    public void OnClickStart() {
        onStart?.Invoke(
            new IntVector3(int.Parse(inputW.text), int.Parse(inputH.text), int.Parse(inputD.text)),
            int.Parse(comboLengthInput.text),
            fillAllSpacesToggle.isOn ? 999 : int.Parse(fixedScoreInput.text));
    }

    private void Update() {
        bool inputValid = IsInputValid(inputW)
            && IsInputValid(inputH)
            && IsInputValid(inputD)
            && IsInputValid(comboLengthInput)
            && (fillAllSpacesToggle.isOn || IsInputValid(fixedScoreInput, 99));
        startButton.interactable = inputValid;
    }

    private bool IsInputValid(InputField input, int maxValue = 10) {
        int result;
        try {
            result = int.Parse(input.text);
        } catch (Exception e) {
            return false;
        }
        return result > 0 && result <= maxValue;
    }
}
