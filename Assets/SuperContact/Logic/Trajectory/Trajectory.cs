using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trajectory : ScriptableObject {
    public abstract float GetDuration();
    public abstract Vector3 GetPosition(float t);
}
