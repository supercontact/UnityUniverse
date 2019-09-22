using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LineNumbers : MonoBehaviour {

    public Text codeText;

    private Text text;
    private int lineCount = 1;

    private void Awake() {
        text = GetComponent<Text>();
    }

    private void Update () {
        int newLineCount = codeText.cachedTextGenerator.lineCount;
        if (newLineCount != lineCount) {
            text.text = buildLineNumbersString(newLineCount);
        }
        lineCount = newLineCount;
    }

    private static string buildLineNumbersString(int lineCount) {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < lineCount; i++) {
            builder.AppendLine($"{i+1}");
        }
        return builder.ToString();
    }
}
