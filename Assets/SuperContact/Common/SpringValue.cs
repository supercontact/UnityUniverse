using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringValue {

    private static readonly float EPSILON = 0.00001f;

    public float acceleration { get; private set; }
    public float drag { get; private set; }

    public float targetValue;
    public float value;
    public float velocity;

    private float r1, r2;
    private int positivity;

    public SpringValue(float initialValue, float acceleration = 100f, float drag = 10f) {
        value = initialValue;
        targetValue = initialValue;
        velocity = 0;

        SetProperties(acceleration, drag);
    }

    public float Evolve(float deltaTime) {
        if (positivity == 1) {
            float c1 = (r2 * (value - targetValue) - velocity) / (r2 - r1);
            float c2 = (r1 * (value - targetValue) - velocity) / (r1 - r2);
            value = c1 * Mathf.Exp(r1 * deltaTime) + c2 * Mathf.Exp(r2 * deltaTime) + targetValue;
            velocity = c1 * r1 * Mathf.Exp(r1 * deltaTime) + c2 * r2 * Mathf.Exp(r2 * deltaTime);
        } else if (positivity == -1) {
            float c1 = value - targetValue;
            float c2 = (velocity - r1 * (value - targetValue)) / r2;
            value = Mathf.Exp(r1 * deltaTime) * (c1 * Mathf.Cos(r2 * deltaTime) + c2 * Mathf.Sin(r2 * deltaTime)) + targetValue;
            velocity = Mathf.Exp(r1 * deltaTime) * ((c1 * r1 + c2 * r2) * Mathf.Cos(r2 * deltaTime) + (c2 * r1 - c1 * r2) * Mathf.Sin(r2 * deltaTime));
        } else {
            float c1 = value - targetValue;
            float c2 = velocity - r1 * (value - targetValue);
            value = (c1 + c2 * deltaTime) * Mathf.Exp(r1 * deltaTime) + targetValue;
            velocity = (r1 * c1 + c2 + r1 * c2 * deltaTime) * Mathf.Exp(r1 * deltaTime);
        }
        return value;
    }

    private void SetProperties(float acceleration, float drag) {
        this.acceleration = acceleration;
        this.drag = drag;

        float delta2 = drag * drag - 4 * acceleration;
        if (delta2 > EPSILON) {
            float delta = Mathf.Sqrt(delta2);
            r1 = (-drag - delta) / 2;
            r2 = (-drag + delta) / 2;
            positivity = 1;
        } else if (delta2 < -EPSILON) {
            r1 = -drag / 2;
            r2 = Mathf.Sqrt(-delta2) / 2;
            positivity = -1;
        } else {
            r1 = -drag / 2;
            positivity = 0;
        }
    }
}