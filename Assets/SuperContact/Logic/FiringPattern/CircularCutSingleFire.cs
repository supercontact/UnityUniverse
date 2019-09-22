using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CircularCutSingleFire", menuName = "Scriptable Objects/FiringPattern/CircularCutSingleFire", order = 1)]
public class CircularCutSingleFire : FiringPattern {

    public int countPerCircularLine = 12;
    public int circularLinePerCut = 1;
    public int cutCount = 1;
    public float circularAngle = 180f;
    public float rotationOffsetAngle = 0f;
    public float maxProjectileSpeed = 1f;
    public float minProjectileSpeed = 0.4f;

    public override int GetProjectileCount(int comboNumber) {
        return countPerCircularLine * circularLinePerCut * cutCount; 
    }

    public override IEnumerable<Vector3> GetProjectileOrigins(int comboNumber) {
        while (true) {
            yield return Vector3.zero;
        }
    }

    public override IEnumerable<Vector3> GetProjectileVelocities(int comboNumber) {
        for (int i = 0; i < countPerCircularLine; i++) {
            Vector3 baseDirection = Quaternion.AngleAxis(circularAngle * (-0.5f + i / (float)(countPerCircularLine - 1)), Vector3.up) * Vector3.forward;
            for (int c = 0; c < cutCount; c++) {
                Vector3 rotatedDirection = Quaternion.AngleAxis(rotationOffsetAngle + 180f * c / cutCount, Vector3.forward) * baseDirection;
                for (int l = 0; l < circularLinePerCut; l++) {
                    if (circularLinePerCut == 1) {
                        yield return rotatedDirection * maxProjectileSpeed;
                    } else {
                        yield return rotatedDirection * Mathf.Lerp(minProjectileSpeed, maxProjectileSpeed, l / (float)(circularLinePerCut - 1));
                    }
                }
            }
        }
    }
}
