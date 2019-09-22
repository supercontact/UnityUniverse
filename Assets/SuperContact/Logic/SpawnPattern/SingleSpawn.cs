using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SingleSpawn", menuName = "Scriptable Objects/SpawnPattern/SingleSpawn", order = 1)]
public class SingleSpawn : PeriodicPrefabSpawnPattern {
    public Vector3 trajectoryOffset = Vector3.zero;
    public Quaternion trajectoryRotation = Quaternion.identity;
    public Trajectory trajectory;

    public override int GetSpawnCount() {
        return 1;
    }

    public override IEnumerable<float> GetSpawnTimes() {
        yield return 0;
    }

    public override IEnumerable<Vector3> GetTrajectoryOffsets() {
        yield return trajectoryOffset;
    }

    public override IEnumerable<Quaternion> GetTrajectoryRotations() {
        yield return trajectoryRotation;
    }

    public override IEnumerable<Trajectory> GetTrajectories() {
        yield return trajectory;
    }
}
