using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideEditor : MonoBehaviour {

    public GameObject showButton;

    public void HideEditor() {
        gameObject.SetActive(false);
        showButton.SetActive(true);
    }

    public void ShowEditor() {
        gameObject.SetActive(true);
        showButton.SetActive(false);
    }
}
