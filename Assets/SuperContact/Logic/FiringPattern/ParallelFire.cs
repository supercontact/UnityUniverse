using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParallelFire", menuName = "Scriptable Objects/FiringPattern/ParallelFire", order = 1)]
public class ParallelFire : FiringPattern {

    public Vector3[] projectileOrigins;
    public float projectileSpeed = 1f;

    public override int GetProjectileCount(int comboNumber) {
        return projectileOrigins.Length; 
    }

    public override IEnumerable<Vector3> GetProjectileOrigins(int comboNumber) {
        return projectileOrigins;
    }

    public override IEnumerable<Vector3> GetProjectileVelocities(int comboNumber) {
        while (true) {
            yield return projectileSpeed * Vector3.forward;
        }
    }
}
