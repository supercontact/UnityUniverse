using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FocusableObject : MonoBehaviour {

    public string focusName = "Default";

    public void Focus() {
        FocusManager.currentFocus = focusName;
    }
}
