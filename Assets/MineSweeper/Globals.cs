using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour {

    public static Globals instance;

    public ObserveCamera observeCamera;

    private void Awake() {
        instance = this;
    }
}
