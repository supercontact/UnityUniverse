using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavBar : MonoBehaviour {
    public Button[] navButtons;
    public GameObject[] navContents;
    public GameObject[] additionalNavContents;

    private void Start() {
        for (int i = 0; i < navButtons.Length; i++) {
            int index = i;
            navButtons[i].onClick.AddListener(() => { SwitchTo(index); });
        }
    }

    public void SwitchTo(int index) {
        for (int i = 0; i < navButtons.Length; i++) {
            if (i == index) {
                navButtons[i].interactable = false;
                navContents[i].SetActive(true);
                additionalNavContents[i].SetActive(true);
            } else {
                navButtons[i].interactable = true;
                navContents[i].SetActive(false);
                additionalNavContents[i].SetActive(false);
            }
        }
    }
}
