using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour {
    public abstract int team { get; set; }
    public abstract void Fire(Vector3 origin, Vector3 velocity, float timestamp);
}
