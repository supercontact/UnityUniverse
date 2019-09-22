using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitedLifespan : MonoBehaviour {

    public float lifespan = 1;

    private float timePassed;

	// Use this for initialization
	void Start () {
        timePassed = 0;
	}
	
	// Update is called once per frame
	void Update () {
        timePassed += Time.deltaTime;
        if (timePassed > lifespan) {
            Destroy(gameObject);
        }
	}
}
