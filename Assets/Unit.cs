using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    public int team = 0;
    public int maxHP = 100;
    public int hp = 100;

    public virtual Vector3 Velocity { get; }
}
