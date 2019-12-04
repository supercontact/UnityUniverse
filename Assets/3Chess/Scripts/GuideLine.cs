using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideLine : MonoBehaviour {

    public GameObject[] gameObjects;
    public Material glowMaterialPrefab;
    public float maxIntensity = 0.5f;
    public float minIntensity = -0.5f;

    private bool _controlsOther;
    public bool controlsOther {
        get { return _controlsOther; }
        set {
            _controlsOther = value;
            if (!value && intensityValue.targetValue != minIntensity) {
                connection?.Send(new GuideLineDisplayRequest(false));
            }
        }
    }

    private Connection connection;
    private Material glowMaterial;
    private SpringValue intensityValue = new SpringValue(0, 200, 40);
    private bool isVisible = false;

    private void Start() {
        glowMaterial = Instantiate(glowMaterialPrefab);
        foreach (GameObject obj in gameObjects) {
            obj.GetComponent<MeshRenderer>().sharedMaterial = glowMaterial;
        }
    }

    private void OnDestroy() {
        Destroy(glowMaterial);
        foreach (GameObject obj in gameObjects) {
            obj.GetComponent<MeshRenderer>().sharedMaterial = glowMaterialPrefab;
        }
    }

    public void Init(Connection connection) {
        this.connection = connection;
        connection?.Listen<GuideLineDisplayRequest>(OnGuideLineDisplayRequest);
    }

    public void OnCameraModeChange(ObserveCamera.Mode mode) {
        if (!gameObject.activeSelf) return;

        if (mode == ObserveCamera.Mode.Rotation) {
            if (intensityValue.targetValue != maxIntensity) {
                intensityValue.targetValue = maxIntensity;
                if (controlsOther) {
                    connection?.Send(new GuideLineDisplayRequest(true));
                }
            }
        } else {
            if (intensityValue.targetValue != minIntensity) {
                intensityValue.targetValue = minIntensity;
                if (controlsOther) {
                    connection?.Send(new GuideLineDisplayRequest(false));
                }
            }
        }
    }

    private void OnGuideLineDisplayRequest(GuideLineDisplayRequest request) {
        intensityValue.targetValue = request.isOn ? maxIntensity : minIntensity;
    }

    private void Update() {
        intensityValue.Evolve(Time.deltaTime);
        glowMaterial.SetColor("_Color", new Color(1, 1, 1, Mathf.Max(intensityValue.value, 0)));
        if (intensityValue.value > 0.01f && !isVisible) {
            isVisible = true;
            foreach (GameObject obj in gameObjects) {
                obj.SetActive(true);
            }
        } else if (intensityValue.value <= 0.01f && isVisible) {
            isVisible = false;
            foreach (GameObject obj in gameObjects) {
                obj.SetActive(false);
            }
        }
    }

}
