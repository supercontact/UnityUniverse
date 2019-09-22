using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainText : MonoBehaviour {

    public Text textField;
    public InputField selectableField;

    public string text {
        get {
            return textField.text;
        }
        set {
            textField.text = value;
            selectableField.text = value;
        }
    }
}
