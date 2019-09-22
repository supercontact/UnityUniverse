using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NumberMaterials : MonoBehaviour {

    private static NumberMaterials instance;

    public Texture2D[] numberTextures;
    public Color[] numberColors;
    public float albedoFactor = 0.2f;
    public float emissionFactor = 1f;
    public Material baseMaterial;

    private Material[] numberMaterials;

    private void Awake() {
        instance = this;
        numberMaterials = new Material[numberTextures.Length];
        for (int i = 0; i < numberTextures.Length; i++) {
            Material material = Instantiate(baseMaterial);
            material.SetColor("_Color", numberColors[i].TimesIgnoringAlpha(albedoFactor));
            material.SetColor("_EmissionColor", numberColors[i].TimesIgnoringAlpha(emissionFactor));
            material.SetTexture("_MainTex", numberTextures[i]);
            numberMaterials[i] = material;
        }
    }

    public static Material GetNumberMaterial(int number) {
        int index = number - 1;
        return instance.numberMaterials[Mathf.Min(index, instance.numberMaterials.Length - 1)];
    }
}
