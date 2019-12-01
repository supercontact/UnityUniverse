using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusableInput {

    public string focusName;

    public Vector3 mousePosition {
        get { return Input.mousePosition; }
    }

    public Vector2 mouseScrollDelta {
        get { return IsInFocus() ? Input.mouseScrollDelta : Vector2.zero; }
    }

    public FocusableInput(string focusName = "Default") {
        this.focusName = focusName;
    }

    public float GetAxis(string axisName) {
        return IsInFocus() ? Input.GetAxis(axisName) : 0;
    }

    public float GetAxisRaw(string axisName) {
        return IsInFocus() ? Input.GetAxisRaw(axisName) : 0;
    }

    public bool GetButton(string buttonName) {
        return IsInFocus() ? Input.GetButton(buttonName) : false;
    }

    public bool GetButtonDown(string buttonName) {
        return IsInFocus() ? Input.GetButtonDown(buttonName) : false;
    }

    public bool GetButtonUp(string buttonName) {
        return IsInFocus() ? Input.GetButtonUp(buttonName) : false;
    }

    public bool GetKey(KeyCode key) {
        return IsInFocus() ? Input.GetKey(key) : false;
    }

    public bool GetKeyDown(KeyCode key) {
        return IsInFocus() ? Input.GetKeyDown(key) : false;
    }

    public bool GetKeyUp(KeyCode key) {
        return IsInFocus() ? Input.GetKeyUp(key) : false;
    }

    public bool GetMouseButton(int button) {
        return IsInFocus() ? Input.GetMouseButton(button) : false;
    }

    public bool GetMouseButtonDown(int button) {
        return IsInFocus() ? Input.GetMouseButtonDown(button) : false;
    }

    public bool GetMouseButtonUp(int button) {
        return IsInFocus() ? Input.GetMouseButtonUp(button) : false;
    }

    private bool IsInFocus() {
        return FocusManager.currentFocus == focusName;
    }
}
