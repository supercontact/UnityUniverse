using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CompositeSpawn", menuName = "Scriptable Objects/SpawnPattern/CompositeSpawn", order = 1)]
public class CompositeSpawnPattern : SpawnPattern {
    public SpawnPattern[] spawnPatterns;
    public float[] patternStartTimes;

    private bool initiated = false;
    private int totalSpawnCount;
    private List<float> spawnTimes = new List<float>();
    private List<GameObject> prefabs = new List<GameObject>();
    private List<Vector3> trajectoryOffsets = new List<Vector3>();
    private List<Quaternion> trajectoryRotations = new List<Quaternion>();
    private List<Trajectory> trajectories = new List<Trajectory>();
    private List<Target> targets = new List<Target>();

    public override int GetSpawnCount() {
        MaybeInitiate();
        return totalSpawnCount;
    }

    public override IEnumerable<float> GetSpawnTimes() {
        MaybeInitiate();
        return spawnTimes;
    }

    public override IEnumerable<GameObject> GetPrefabs() {
        MaybeInitiate();
        return prefabs;
    }

    public override IEnumerable<Vector3> GetTrajectoryOffsets() {
        MaybeInitiate();
        return trajectoryOffsets;
    }

    public override IEnumerable<Quaternion> GetTrajectoryRotations() {
        MaybeInitiate();
        return trajectoryRotations;
    }

    public override IEnumerable<Trajectory> GetTrajectories() {
        MaybeInitiate();
        return trajectories;
    }

    public override IEnumerable<Target> GetTargets() {
        MaybeInitiate();
        return targets;
    }

    private void MaybeInitiate() {
        if (initiated) return;

        int n = spawnPatterns.Length;
        for (int i = 0; i < n; i++) {
            SpawnPattern pattern = spawnPatterns[i];
            int count = pattern.GetSpawnCount();
            float startTime = patternStartTimes[i];

            totalSpawnCount += count;
            spawnTimes.AddRange(pattern.GetSpawnTimes().Take(count).Select(t => t + startTime));
            prefabs.AddRange(pattern.GetPrefabs().Take(count));
            trajectoryOffsets.AddRange(pattern.GetTrajectoryOffsets().Take(count));
            trajectoryRotations.AddRange(pattern.GetTrajectoryRotations().Take(count));
            trajectories.AddRange(pattern.GetTrajectories().Take(count));
            targets.AddRange(pattern.GetTargets().Take(count));
        }

        var indexes = new List<int>(
            spawnTimes
                .Select((time, index) => new { time, index })
                .OrderBy(pair => pair.time)
                .Select(pair => pair.index));
        spawnTimes = new List<float>(indexes.Select(i => spawnTimes[i]));
        prefabs = new List<GameObject>(indexes.Select(i => prefabs[i]));
        trajectoryOffsets = new List<Vector3>(indexes.Select(i => trajectoryOffsets[i]));
        trajectoryRotations = new List<Quaternion>(indexes.Select(i => trajectoryRotations[i]));
        trajectories = new List<Trajectory>(indexes.Select(i => trajectories[i]));
        targets = new List<Target>(indexes.Select(i => targets[i]));

        initiated = true;
    }
}
