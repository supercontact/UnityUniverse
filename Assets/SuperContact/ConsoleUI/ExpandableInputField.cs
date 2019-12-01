using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableInputField : MonoBehaviour {

    public InputField innerInputField;

    private Text text;

    private void Awake() {
        text = GetComponent<Text>();
    }

    public void OnInputFieldChanged() {
        text.text = innerInputField.text;
    }
}
