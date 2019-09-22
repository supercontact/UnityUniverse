using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PeriodicPrefabSpawnPattern : SpawnPattern {
    public GameObject[] prefabs;
    public int prefabRepeat = 1;
    public Target[] targets;
    public int targetRepeat = 1;

    public override IEnumerable<GameObject> GetPrefabs() {
        while (true) {
            foreach (GameObject prefab in prefabs) {
                for (int i = 0; i < prefabRepeat; i++) {
                    yield return prefab;
                }
            }
        }
    }

    public override IEnumerable<Target> GetTargets() {
        while (true) {
            foreach (Target target in targets) {
                for (int i = 0; i < targetRepeat; i++) {
                    yield return target;
                }
            }
        }
    }
}
