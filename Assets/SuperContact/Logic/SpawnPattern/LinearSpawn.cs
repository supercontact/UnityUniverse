using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LinearSpawn", menuName = "Scriptable Objects/SpawnPattern/LinearSpawn", order = 1)]
public class LinearSpawn : PeriodicPrefabSpawnPattern {
    public int count;
    public float timeGap;
    public Vector3 startPoint;
    public Vector3 endPoint;
    public Quaternion trajectoryRotation = Quaternion.identity;
    public Trajectory trajectory;

    public override int GetSpawnCount() {
        return count;
    }

    public override IEnumerable<float> GetSpawnTimes() {
        for (int i = 0; i < count; i++) {
            yield return timeGap * i;
        }
    }

    public override IEnumerable<Vector3> GetTrajectoryOffsets() {
        for (int i = 0; i < count; i++) {
            yield return Vector3.Lerp(startPoint, endPoint, (float)i / (count - 1));
        }
    }

    public override IEnumerable<Quaternion> GetTrajectoryRotations() {
        while (true) {
            yield return trajectoryRotation;
        }
    }

    public override IEnumerable<Trajectory> GetTrajectories() {
        while (true) {
            yield return trajectory;
        }
    }
}
