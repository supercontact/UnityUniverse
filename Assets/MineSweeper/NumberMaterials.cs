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
    public Material highlightedMaterial;

    private Material[] numberMaterials;
    private Material[] highlightedNumberMaterials;

    private void Awake() {
        instance = this;
        numberMaterials = new Material[numberTextures.Length];
        highlightedNumberMaterials = new Material[numberTextures.Length];
        for (int i = 0; i < numberTextures.Length; i++) {
            Material material = Instantiate(baseMaterial);
            material.SetColor("_Color", numberColors[i].TimesIgnoringAlpha(albedoFactor));
            material.SetColor("_EmissionColor", numberColors[i].TimesIgnoringAlpha(emissionFactor));
            material.SetTexture("_MainTex", numberTextures[i]);
            numberMaterials[i] = material;

            Material materialH = Instantiate(highlightedMaterial);
            materialH.SetTexture("_MainTex", numberTextures[i]);
            highlightedNumberMaterials[i] = materialH;
        }
    }

    public static Material GetNumberMaterial(int number) {
        return instance.numberMaterials[NumberToIndex(number)];
    }

    public static Material GetHighlightedNumberMaterial(int number) {
        return instance.highlightedNumberMaterials[NumberToIndex(number)];
    }

    private static int NumberToIndex(int number) {
        return Mathf.Min(number - 1, instance.numberMaterials.Length - 1);
    }
}
