using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamageBlink : MonoBehaviour {

    public float blinkDuration = 0.05f;
    public Color blinkColor = new Color(0.9f, 0.9f, 0.9f);
    public float blinkColorIntensity = 1f;

    private Material[] bodyMaterials;
    private float blinkTimer = 0f;

    private void Start () {
        var bodies = GetComponentsInChildren<MeshRenderer>();
        bodyMaterials = bodies.Select(body => body.material).ToArray();
        foreach (Material material in bodyMaterials) {
            material.EnableKeyword("_EMISSION");
        }
        enabled = false;
    }

    private void Update () {
        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0) {
            StopBlink();
        }
    }

    private void OnDestroy() {
        bodyMaterials.ForEach(Destroy);
    }

    public void Blink() {
        bodyMaterials.ForEach(m => m.SetColor("_EmissionColor", blinkColor * blinkColorIntensity));
        blinkTimer = blinkDuration;
        enabled = true;
    }

    public void StopBlink() {
        if (bodyMaterials == null || !enabled) return;
        blinkTimer = 0f;
        bodyMaterials.ForEach(m => m.SetColor("_EmissionColor", Color.black));
        enabled = false;
    }
}
