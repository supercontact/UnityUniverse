using System;
using UnityEngine;
using UnityEngine.UI;

public class LogEditor : MonoBehaviour {

    public static LogEditor instance;

    public GameObject logArea;
    public ScrollRect scrollView;
    public GameObject warningSign;

    public GameObject logBlockPrefab;
    public GameObject errorBlockPrefab;

    private ScriptingInterface _scriptingInterface;
    public ScriptingInterface scriptingInterface {
        get { return _scriptingInterface; }
        set {
            UnlinkScriptingInterface(_scriptingInterface);
            _scriptingInterface = value;
            LinkScriptingInterface(_scriptingInterface);
        }
    }

    private int keepScrollingToBottom = 0;

    private void Awake() {
        instance = this;
    }

    private void OnDestroy() {
        scriptingInterface = null;
        instance = null;
    }

    private void Update() {
        CheckScrolling();
        if (scrollView.gameObject.activeSelf) {
            warningSign.SetActive(false);
        }
    }

    public void AddLog(string text) {
        AddBlock(text, logBlockPrefab);
    }

    public void AddErrorLog(string text) {
        AddBlock(text, errorBlockPrefab);
        MaybeShowWarning();
    }

    public void AddException(Exception exception) {
        AddBlock(exception.Message, errorBlockPrefab);
        MaybeShowWarning();
    }

    public void AddBlock(string text, GameObject blockPrefab) {
        bool atButton = (scrollView.verticalNormalizedPosition < 0.000001f);

        GameObject block = Instantiate(blockPrefab);
        block.GetComponent<MainText>().text = text.ToString();
        block.transform.SetParent(logArea.transform, false);

        if (atButton) {
            ScrollToBottom();
        }
    }

    public void ClearLogs() {
        int removeCount = logArea.transform.childCount;
        for (int i = 0; i < removeCount; i++) {
            Destroy(logArea.transform.GetChild(i).gameObject);
        }
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

    private void MaybeShowWarning() {
        if (!scrollView.gameObject.activeSelf) {
            warningSign.SetActive(true);
        } 
    }

    private void LinkScriptingInterface(ScriptingInterface scriptingInterface) {
        if (scriptingInterface == null) return;
        scriptingInterface.OnLog += AddLog;
        scriptingInterface.OnLogError += AddErrorLog;
        scriptingInterface.OnException += AddException;
    }

    private void UnlinkScriptingInterface(ScriptingInterface scriptingInterface) {
        if (scriptingInterface == null) return;
        scriptingInterface.OnLog -= AddLog;
        scriptingInterface.OnLogError -= AddErrorLog;
        scriptingInterface.OnException -= AddException;
    }
}
