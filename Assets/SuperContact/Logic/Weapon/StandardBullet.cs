using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardBullet : Projectile {
    public Vector3 velocity;
    public float lifespan = 5;
    public int damage = 1;
    public string damageType = "";
    public float radius = 0;

    public GameObject hitEffect;

    public delegate void OnHitCallback(Damagable damagable);
    public event OnHitCallback OnHit;

    private float lastUpdateTime;
    private float launchTime;

    public override int team { get; set; }

    public override void Fire(Vector3 origin, Vector3 velocity, float timestamp) {
        transform.position = origin + (Time.time - timestamp) * velocity;
        transform.rotation = Quaternion.FromToRotation(Vector3.forward, velocity);
        this.velocity = velocity;
        launchTime = timestamp;
        lastUpdateTime = timestamp;
    }
	
	void Update() {
        RaycastHit hitInfo;
        if (Raycast(out hitInfo) && Hit(hitInfo)) {
            return;
        } else {
            UpdatePosition();
        }
    }

    private bool Raycast(out RaycastHit hitInfo) {
        float distance = (Time.time - lastUpdateTime) * velocity.magnitude;
        if (radius == 0) {
            return Physics.Raycast(transform.position, velocity, out hitInfo, distance);
        } else {
            return Physics.SphereCast(transform.position, radius, velocity, out hitInfo, distance);
        }
    }

    protected virtual bool Hit(RaycastHit hitInfo) {
        Damagable damagable = hitInfo.collider.GetComponentInParent<Damagable>();
        if (damagable != null) {
            if (damagable.GetTeam() == team) {
                return false;
            }
            if (damage > 0) {
                damagable.ReceiveDamage(new Damage(damage, damageType));
            }
        }
        OnHit?.Invoke(damagable);
        if (hitEffect != null) {
            GameObject newEffect = Instantiate(hitEffect);
            newEffect.transform.position = hitInfo.point - velocity.normalized * 0.02f;
            newEffect.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hitInfo.normal);
        }
        Destroy(gameObject);
        return true;
    }

    private void UpdatePosition() {
        transform.position += (Time.time - lastUpdateTime) * velocity;
        lastUpdateTime = Time.time;
        if (Time.time - launchTime > lifespan) {
            Destroy(gameObject);
        }
    }
}
