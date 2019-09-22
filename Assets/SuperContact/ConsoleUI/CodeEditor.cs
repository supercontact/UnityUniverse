using UnityEngine;
using UnityEngine.UI;

public class CodeEditor : MonoBehaviour {

    public static CodeEditor instance;

    public ScriptingInterface scriptingInterface;

    public InputField codeInput;
    public ScrollRect scrollView;

    private int keepScrollingToBottom = 0;

    private void Awake() {
        instance = this;
    }

    private void Update() {
        CheckScrolling();
    }

    public void SaveAndRunCode() {
        scriptingInterface.SubmitCode("main", codeInput.text);
    }

    public void ScrollToBottom() {
        keepScrollingToBottom = 2;
    }

    private void CheckScrolling() {
        if (keepScrollingToBottom > 0) {
            keepScrollingToBottom--;
            scrollView.verticalNormalizedPosition = 0;
        }
    }
}
