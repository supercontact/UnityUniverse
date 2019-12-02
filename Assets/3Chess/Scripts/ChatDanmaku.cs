using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatDanmaku : MonoBehaviour {

    public float speed = 180;
    public float marginTop = 50;
    public float marginBottom = 50;

    private RectTransform rect;
    private Vector2 size;

    public void Init(string message) {
        GetComponent<Text>().text = message;
    }

    private void Start() {
        Canvas canvas = GetComponentInParent<Canvas>();
        Rect canvasRect = canvas.GetComponent<RectTransform>().rect;

        rect = GetComponent<RectTransform>();
        size = rect.offsetMax - rect.offsetMin;
        Vector2 startPos = new Vector2(canvasRect.width, Random.Range(-canvasRect.height / 2 + size.y / 2 + marginBottom, canvasRect.height / 2 - size.y - marginTop));
        rect.offsetMin = new Vector2(startPos.x, startPos.y - size.y / 2);
        rect.offsetMax = new Vector2(startPos.x + size.x, startPos.y + size.y / 2);
    }

    private void Update() {
        rect.offsetMin += Vector2.left * speed * Time.deltaTime;
        rect.offsetMax += Vector2.left * speed * Time.deltaTime;
        if (rect.offsetMax.x < 0) {
            Destroy(gameObject);
        }
    }
}
