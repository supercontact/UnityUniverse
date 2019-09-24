using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineControl : MonoBehaviour {

    private static readonly float MINE_ELEVATION = 0.15f;
    private static readonly float MINE_SCALE = 1.2f;
    private static readonly float MINE_BLINK_PERIOD_MIN = 0.2f;
    private static readonly float MINE_BLINK_PERIOD_FACTOR = 0.2f;

    public TileControl parent;
    public GameObject explosionPrefab;
    public GameObject mineLock;

    private float blinkingRemainingDuration;
    private float nextBlinkTimer;
    private bool isBlinking = false;
    private bool isBlinkingPaused = false;
    private DamageBlink blinkComponent;

    private void Awake() {
        blinkComponent = GetComponent<DamageBlink>();
        enabled = false;
    }

    private void Update() {
        if (isBlinkingPaused) return;

        nextBlinkTimer -= Time.deltaTime;
        blinkingRemainingDuration -= Time.deltaTime;
        if (nextBlinkTimer <= 0) {
            blinkComponent.Blink();
            nextBlinkTimer = Mathf.Max(MINE_BLINK_PERIOD_MIN, blinkingRemainingDuration * MINE_BLINK_PERIOD_FACTOR);
        }
    }

    public void Init() {
        transform.position = parent.faceCenter + parent.faceNormal * MINE_ELEVATION * parent.GetAverageRadius();
        transform.rotation = Quaternion.LookRotation(parent.faceNormal);
        transform.localScale = MINE_SCALE * parent.GetAverageRadius() * Vector3.one;
    }

    public void StartBlinking(float duration) {
        blinkingRemainingDuration = duration;
        nextBlinkTimer = 0;

        isBlinking = true;
        isBlinkingPaused = false;
        enabled = true;
    }

    public void SetBlinkingPaused(bool isPaused) {
        isBlinkingPaused = isPaused;

        mineLock.SetActive(isPaused);

        //if (isExplodingPaused) {
        //    mine.GetComponent<DamageBlink>().blinkColor = new Color(0f, 0f, 0.5f);
        //    mine.GetComponent<DamageBlink>().blinkDuration = 99999;
        //    mine.GetComponent<DamageBlink>().StopBlink();
        //    mine.GetComponent<DamageBlink>().Blink();
        //} else {
        //    mine.GetComponent<DamageBlink>().blinkColor = Color.white;
        //    mine.GetComponent<DamageBlink>().blinkDuration = 0.05f;
        //    mine.GetComponent<DamageBlink>().StopBlink();
        //}
    }

    public void StopBlinking() {
        if (!isBlinking) return;
        SetBlinkingPaused(false);
        isBlinking = false;
        enabled = false;
        blinkComponent.StopBlink();
    }

    public void Explode() {
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = transform.position;
        StopBlinking();
        gameObject.SetActive(false);
    }
}
