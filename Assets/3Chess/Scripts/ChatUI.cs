using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour {

    public InputField input;
    public GameObject danmakuPrefab;

    private FocusableInput defaultInput = new FocusableInput();
    private FocusableInput chatInput = new FocusableInput("Chat");

    private void Start() {
        ChatManager.instance.onNewMessage += DisplayMessage;
    }

    private void Update() {
        if (defaultInput.GetKeyDown(KeyCode.Return)) {
            input.gameObject.SetActive(true);
            input.Select();
            input.ActivateInputField();
            FocusManager.currentFocus = "Chat";
        } else if (chatInput.GetKeyDown(KeyCode.Return)) {
            string message = input.text;
            if (message != "") {
                ChatManager.instance.SendGlobalMessage(message);
            }
            input.text = "";
            input.gameObject.SetActive(false);
            FocusManager.currentFocus = "Default";
        }
    }

    private void DisplayMessage(ChatRequest request) {
        GameObject danmaku = Instantiate(danmakuPrefab, transform);
        danmaku.GetComponent<ChatDanmaku>().Init($"{request.playerName}: {request.message}");
    }
}
