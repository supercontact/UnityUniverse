using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableInputField : MonoBehaviour {

    public void OnInputFieldChanged(string text) {
        this.GetComponent<Text>().text = text;
    }
}
