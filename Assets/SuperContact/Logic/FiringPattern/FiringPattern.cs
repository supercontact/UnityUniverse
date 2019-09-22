using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FiringPattern : ScriptableObject {
    public abstract int GetProjectileCount(int comboNumber);
    public abstract IEnumerable<Vector3> GetProjectileOrigins(int comboNumber);
    public abstract IEnumerable<Vector3> GetProjectileVelocities(int comboNumber);
}
