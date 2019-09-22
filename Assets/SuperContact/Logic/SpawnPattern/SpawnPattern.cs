using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnPattern : ScriptableObject {
    public enum Target {
        None,
        ComputerPlayer,
        VRPlayer,
        LeftPlayer,
        RightPlayer,
        TopPlayer,
        BottomPlayer,
        FrontPlayer,
        BackPlayer,
        NearPlayer,
        FarPlayer,
        RandomPlayer,
    }

    public abstract int GetSpawnCount();
    public abstract IEnumerable<float> GetSpawnTimes();
    public abstract IEnumerable<GameObject> GetPrefabs();
    public virtual IEnumerable<Vector3> GetTrajectoryOffsets() {
        while (true) {
            yield return Vector3.zero;
        }
    }
    public virtual IEnumerable<Quaternion> GetTrajectoryRotations() {
        while (true) {
            yield return Quaternion.identity;
        }
    }
    public abstract IEnumerable<Trajectory> GetTrajectories();
    public virtual IEnumerable<Target> GetTargets() {
        while (true) {
            yield return Target.None;
        }
    }
}
