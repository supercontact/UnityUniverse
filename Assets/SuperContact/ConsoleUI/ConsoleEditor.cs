using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleEditor : MonoBehaviour {

    public static ConsoleEditor instance;

    public ScriptingInterface scriptingInterface;

    public GameObject consoleArea;
    public InputField consoleInput;
    public ScrollRect scrollView;

    public GameObject codeBlockPrefab;
    public GameObject resultBlockPrefab;
    public GameObject errorBlockPrefab;

    private int keepScrollingToBottom = 0;

    private void Awake() {
        instance = this;
    }

    private void OnDestroy() {
        instance = null;
    }

    private void Update() {
        CheckInputSubmit();
        CheckScrolling();
    }

    public void RunConsole() {
        ScrollToBottom();

        string code = consoleInput.text;
        if (code == "") {
            return;
        }
   
        consoleInput.text = "";

        GameObject codeBlock = Instantiate(codeBlockPrefab);
        codeBlock.GetComponent<MainText>().text = code;
        AddBlockToConsoleArea(codeBlock);

        ExecuteScriptAsync(code);
    }

    public async void ExecuteScriptAsync(String code) {
        LockConsole();

        object result = null;
        try {
            result = await scriptingInterface.Execute(code, true /* throwException */);
        } catch (Exception e) {
            GameObject errorBlock = Instantiate(errorBlockPrefab);
            errorBlock.GetComponent<MainText>().text = e.Message;
            AddBlockToConsoleArea(errorBlock);
        }

        if (result != null) {
            GameObject resultBlock = Instantiate(resultBlockPrefab);
            resultBlock.GetComponent<MainText>().text = result.ToString();
            AddBlockToConsoleArea(resultBlock);
        }
        UnlockConsole();
    }

    public void ClearConsole() {
        int removeCount = consoleArea.transform.childCount - 1;
        for (int i = 0; i < removeCount; i++) {
            Destroy(consoleArea.transform.GetChild(i).gameObject);
        }
    }

    public void LockConsole() {
        consoleInput.interactable = false;
    }

    public void UnlockConsole() {
        consoleInput.interactable = true;
        consoleInput.Select();
        consoleInput.ActivateInputField();
    }

    public void ScrollToBottom() {
        keepScrollingToBottom = 2;
    }

    public void SetIsAsync(bool isAsync) {
        scriptingInterface.isAsync = isAsync;
    }

    private void CheckScrolling() {
        if (keepScrollingToBottom > 0) {
            keepScrollingToBottom--;
            scrollView.verticalNormalizedPosition = 0;
        }
    }

    private void AddBlockToConsoleArea(GameObject block) {
        block.transform.SetParent(consoleArea.transform);
        block.transform.SetSiblingIndex(consoleArea.transform.childCount - 2);
    }

    private void CheckInputSubmit() {
        if (consoleInput.isFocused &&
            Input.GetKeyDown(KeyCode.Return) &&
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {

            // Remove the additional line break caused by pressing enter.
            consoleInput.text =
                consoleInput.text.Substring(0, consoleInput.caretPosition - 1) +
                consoleInput.text.Substring(consoleInput.caretPosition);
            RunConsole();
        }
    }
}
