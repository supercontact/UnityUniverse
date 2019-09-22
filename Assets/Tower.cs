using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : Unit, Damagable {

    public StandardGun gun;

    private DamageBlink damageBlink;

    public int GetTeam() {
        return team;
    }

    public void ReceiveDamage(Damage damage) {
        damageBlink.Blink();
    }

    public void Shoot(Unit enemy) {
        float estimatedHitTime = Vector3.Distance(enemy.transform.position, gun.transform.position) / gun.projectileSpeed;
        gun.transform.rotation = Quaternion.LookRotation(enemy.transform.position + estimatedHitTime * enemy.Velocity - gun.transform.position);
        gun.PullTrigger(Time.time);
    }

    public void StopShoot() {
        gun.ReleaseTrigger(Time.time);
    }

    private void Awake() {
        damageBlink = GetComponent<DamageBlink>();
    }

    private void Start() {
        Testing.instance.space.Add(this, transform.position);
    }

    private void Update() {
        Unit closestEnemy = Testing.instance.space.FindClosest(transform.position, float.PositiveInfinity, InSight);
        if (closestEnemy != null) {
            Shoot(closestEnemy);
        } else {
            StopShoot();
        }
        
        Testing.instance.space.Update(this, transform.position);
    }

    private bool InSight(Unit enemy) {
        return enemy.team != team && enemy.transform.position.y >= transform.position.y;
    }
}
