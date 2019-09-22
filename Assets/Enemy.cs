using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit, Damagable {

    public Vector3 speed;

    private Vector3 min;
    private Vector3 max;

    private DamageBlink damageBlink;

    private bool isDead = false;

    public override Vector3 Velocity {
        get { return speed; }
    }

    public int GetTeam() {
        return team;
    }

    public void ReceiveDamage(Damage damage) {
        hp -= damage.amount;
        if (hp < 0) {
            Die();
        }
        damageBlink.Blink();
    }

    public void Die() {
        if (!isDead) {
            isDead = true;
            Testing.instance.space.Remove(this);
            Destroy(gameObject);
        }
    }

    private void Awake() {
        damageBlink = GetComponent<DamageBlink>();
    }

    // Start is called before the first frame update
    void Start() {
        Testing.instance.space.Add(this, transform.position);
        min = Testing.instance.space.spaceMin;
        max = Testing.instance.space.spaceMax;
        speed = Random.insideUnitSphere;
    }

    // Update is called once per frame
    void Update() {
        Vector3 oldSpeed = speed;
        Vector3 predictedPos = transform.position + speed * Time.deltaTime;
        for (int i = 0; i < 3; i++) {
            if (predictedPos[i] <= min[i] || predictedPos[i] >= max[i]) {
                speed[i] = -speed[i];
            }
        }
        if (speed != oldSpeed) {
            transform.rotation = Quaternion.LookRotation(speed);
        }
        transform.position += speed * Time.deltaTime;
        Testing.instance.space.Update(this, transform.position);
    }
}
