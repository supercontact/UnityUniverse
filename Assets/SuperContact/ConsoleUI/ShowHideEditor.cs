using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideEditor : MonoBehaviour {

    public GameObject editorUI;
    public GameObject showButton;
    public bool showWithBacktick = true;

    public void HideEditor() {
        SetEditorVisibility(false);
    }

    public void ShowEditor() {
        SetEditorVisibility(true);
    }

    public void ToggleEditor() {
        SetEditorVisibility(!editorUI.activeSelf);
    }

    private void SetEditorVisibility(bool isVisible) {
        editorUI.SetActive(isVisible);
        if (showButton != null) {
            showButton.SetActive(!isVisible);
        }
    }

    private void Update() {
        if (showWithBacktick && Input.GetKeyDown(KeyCode.BackQuote)) {
            ToggleEditor();
        }
    }
}
