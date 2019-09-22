using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gaussian {

    public float variance = 1;
    public float mean = 1;
    public float amplitude = 1;

    Gaussian() {}
    Gaussian(float variance, float mean, float amplitude) {
        this.variance = variance;
        this.mean = mean;
        this.amplitude = amplitude;
    }
    
    public float Evaluate(float x) {
        return amplitude * Mathf.Exp(-(x - mean) * (x - mean) / (2 * variance * variance));
    }

    public static float Random() {
        return Mathf.Sqrt(-2 * Mathf.Log(UnityEngine.Random.Range(0.0000001f, 1f))) * Mathf.Cos(2 * Mathf.PI * UnityEngine.Random.Range(0f, 1f));
    }

    public static float Random(float limit) {
        if (limit <= 0) throw new System.Exception("Limit should be positive!");
        float result;
        do {
            result = Random();
        } while (Mathf.Abs(result) > limit);
        return result;
    }

    public static Vector2 Random2D() {
        float ang, dist;
        Random2D(out ang, out dist);
        return new Vector2(dist * Mathf.Cos(ang), dist * Mathf.Sin(ang));
    }

    public static Vector2 Random2D(float limit) {
        float ang, dist;
        Random2D(out ang, out dist, limit);
        return new Vector2(dist * Mathf.Cos(ang), dist * Mathf.Sin(ang));
    }

    public static void Random2D(out float angle, out float magnitude) {
        angle = 2 * Mathf.PI * UnityEngine.Random.Range(0f, 1f);
        magnitude = Mathf.Sqrt(-2 * Mathf.Log(UnityEngine.Random.Range(0f, 1f)));
    }

    public static void Random2D(out float angle, out float magnitude, float limit) {
        if (limit <= 0) throw new System.Exception("Limit should be positive!");
        angle = 2 * Mathf.PI * UnityEngine.Random.Range(0f, 1f);
        do {
            magnitude = Mathf.Sqrt(-2 * Mathf.Log(UnityEngine.Random.Range(0f, 1f)));
        } while (magnitude > limit);
    }

    public static Vector3 Random3D() {
        Vector3 result = Random2D();
        result.z = Random();
        return result;
    }

    public static Vector3 Random3D(float limit) {
        if (limit <= 0) throw new System.Exception("Limit should be positive!");
        Vector3 result;
        do {
            result = Random3D();
        } while (result.sqrMagnitude > limit * limit);
        return result;
    }
}
