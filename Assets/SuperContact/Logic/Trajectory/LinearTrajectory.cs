using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LinearTrajectory", menuName = "Scriptable Objects/Trajectory/LinearTrajectory", order = 1)]
public class LinearTrajectory : Trajectory {

    public float duration = 15f;
    public Vector3 startPosition = Vector3.zero;
    public Vector3 endPosition = new Vector3(0, 0, -3);

    public override float GetDuration() {
        return duration;
    }

    public override Vector3 GetPosition(float t) {
        return Vector3.Lerp(startPosition, endPosition, t / duration);
    }
}
