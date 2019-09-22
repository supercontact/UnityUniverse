using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour {

    public static Testing instance;

    public GameObject island;
    public OctreeSpace<Unit> space;

    public GameObject debugPrefab;

    //private CSharpScriptingInterface scriptingInterface;
    private TestEnvironment t;

    private void Awake() {
        instance = this;
        space = new OctreeSpace<Unit>(new Vector3(-5f, 0, -5f), new Vector3(5f, 10f, 5f), 5, 8, 4);
    }

    private void Start() {
        //scriptingInterface = new CSharpScriptingInterface();
        //t = new TestEnvironment();
        //t.x = island.transform.position.x;
        //t.y = island.transform.position.y;
        //t.z = island.transform.position.z;
        //scriptingInterface.SetEnvironment(t);
        //CodeEditor.instance.scriptingInterface = scriptingInterface;
        //ConsoleEditor.instance.scriptingInterface = scriptingInterface;
        //LogEditor.instance.scriptingInterface = scriptingInterface;
    }

    private void Update() {
        // space.ShowDebugVisualization(debugPrefab);
        island.transform.position = new Vector3(t.x, t.y, t.z);
    }
}