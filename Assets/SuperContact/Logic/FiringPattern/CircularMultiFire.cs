using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CircularMultiFire", menuName = "Scriptable Objects/FiringPattern/CircularMultiFire", order = 1)]
public class CircularMultiFire : FiringPattern {

    public int count = 12;
    public float startAngle = 5f;
    public float angleIncreasePerCombo = 5f;
    public float rotationPerCombo = 0f;
    public float projectileSpeed = 1f;

    public override int GetProjectileCount(int comboNumber) {
        return count; 
    }

    public override IEnumerable<Vector3> GetProjectileOrigins(int comboNumber) {
        while (true) {
            yield return Vector3.zero;
        }
    }

    public override IEnumerable<Vector3> GetProjectileVelocities(int comboNumber) {
        float angle = startAngle + comboNumber * angleIncreasePerCombo;
        Vector3 startVelocity = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * projectileSpeed;
        for (int i = 1; i <= count; i++) {
            yield return Quaternion.AngleAxis(360f * i / count + comboNumber * rotationPerCombo, Vector3.forward) * startVelocity;
        }
    }
}
