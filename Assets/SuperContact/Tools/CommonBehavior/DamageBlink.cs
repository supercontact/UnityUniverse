using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBlink : MonoBehaviour {

    public float blinkDuration = 0.05f;

    private Material bodyMaterial;
    private float blinkTimer = 0f;

    private void Start () {
        var body = GetComponentInChildren<MeshRenderer>();
        bodyMaterial = body.material;
        bodyMaterial.EnableKeyword("_EMISSION");
        enabled = false;
    }

    private void Update () {
        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0) {
            bodyMaterial.SetColor("_EmissionColor", Color.black);
            enabled = false;
        }
    }

    private void OnDestroy() {
        Destroy(bodyMaterial);
    }

    public void Blink() {
        bodyMaterial.SetColor("_EmissionColor", new Color(0.9f, 0.9f, 0.9f));
        blinkTimer = blinkDuration;
        enabled = true;
    }
}
