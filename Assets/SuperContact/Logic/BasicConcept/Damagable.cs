using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Damagable {
    int GetTeam();
    void ReceiveDamage(Damage damage);
}
