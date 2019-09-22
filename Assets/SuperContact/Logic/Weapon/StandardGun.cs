using System.Collections.Generic;
using UnityEngine;

public class StandardGun : Gun {
    public enum FiringMode {
        Burst,
        Automatic,
    }

    public enum SpreadMode {
        GaussianProbability,
        UniformProbability
    }

    public int team = 1;
    public FiringMode firingMode = FiringMode.Automatic;
    public SpreadMode spreadMode = SpreadMode.GaussianProbability;
    public float fireRatePerSecond = 5;
    public int burstCount = 1;
    public Projectile projectile;
    public Vector3 projectileOrigin = Vector3.zero;
    public Vector3 forwardDirection = Vector3.forward;
    public Vector3 upwardDirection = Vector3.up;

    // The following properties are used when the firingPattern is not set.
    public float projectileSpreadAngle = 2;
    public float projectileSpeed = 100;
    public float projectileSpeedVariation = 0;
    public int projectileCountPerShot = 1;

    public FiringPattern firingPattern;

    public int ammoCostPerShot = 1;
    public int ammoCount = 8;
    public float ammoRegenerationPerSecond = 0;
    public bool canReload = true;
    public float reloadTime = 1;

    public int currentAmmoCount;
    public bool triggerPressed;
    public float lastFireTime = -99999;
    public float lastTriggerReleaseTime = -99999;
    public float lastReloadStartTime = -99999;
    public int burstRoundsFired = int.MaxValue;

    private void Start() {
        currentAmmoCount = ammoCount;
        forwardDirection.Normalize();
    }

    private void Update() {
        float gapTime = 1 / fireRatePerSecond;
        if (firingMode == FiringMode.Automatic) {
            while (Time.time - lastFireTime > gapTime && (triggerPressed || lastTriggerReleaseTime - lastFireTime > gapTime)) {
                Fire(lastFireTime + gapTime, burstRoundsFired);
                burstRoundsFired++;
            }
        } else if (firingMode == FiringMode.Burst) {
            while (Time.time - lastFireTime > gapTime && burstRoundsFired < burstCount) {
                Fire(lastFireTime + gapTime, burstRoundsFired);
                burstRoundsFired++;
            }
        }
    }

    public override void PullTrigger(float timestamp) {
        if (!triggerPressed && timestamp - lastReloadStartTime > reloadTime) {
            Fire(timestamp, 0);
            triggerPressed = true;
            burstRoundsFired = 1;
        }
    }

    public override void ReleaseTrigger(float timestamp) {
        if (triggerPressed) {
            triggerPressed = false;
            lastTriggerReleaseTime = timestamp;
        }
    }

    public void Fire(float timestamp, int comboNumber = 0) {
        if (currentAmmoCount >= ammoCostPerShot) {
            if (firingPattern == null) {
                for (int i = 0; i < projectileCountPerShot; i++) {
                    Vector3 origin = transform.TransformPoint(projectileOrigin);
                    float actualSpeed = projectileSpeed;
                    Vector3 actualDirection = Vector3.forward;
                    if (spreadMode == SpreadMode.UniformProbability) {
                        actualSpeed += projectileSpeedVariation * Random.Range(-1f, 1f);
                        actualDirection = Quaternion.AngleAxis(Mathf.Acos(Random.Range(Mathf.Cos(projectileSpreadAngle * Mathf.Deg2Rad), 1)) * Mathf.Rad2Deg, Vector3.right) * actualDirection;
                        actualDirection = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward) * actualDirection;
                    } else {
                        actualSpeed += projectileSpeedVariation * Gaussian.Random(2) / 2;
                        float ang, dist;
                        Gaussian.Random2D(out ang, out dist, 2);
                        actualDirection = Quaternion.AngleAxis(projectileSpreadAngle * dist / 2, Vector3.right) * actualDirection;
                        actualDirection = Quaternion.AngleAxis(ang * Mathf.Rad2Deg, Vector3.forward) * actualDirection;
                    }
                    actualDirection = Quaternion.FromToRotation(Vector3.forward, forwardDirection) * actualDirection;
                    Vector3 velocity = transform.TransformVector(actualSpeed * actualDirection);

                    GameObject newProjectileObj = Instantiate(projectile.gameObject);
                    Projectile newProjectile = newProjectileObj.GetComponent<Projectile>();
                    newProjectile.team = team;
                    newProjectile.Fire(origin, velocity, timestamp);
                }
            } else {
                Vector3 rightDirection = Vector3.Cross(upwardDirection, forwardDirection);
                Matrix4x4 patternTransform = Matrix4x4.TRS(projectileOrigin, Quaternion.LookRotation(forwardDirection, upwardDirection), Vector3.one);

                int projectileCount = firingPattern.GetProjectileCount(comboNumber);
                IEnumerator<Vector3> projectileOriginEnumerator = firingPattern.GetProjectileOrigins(comboNumber).GetEnumerator();
                IEnumerator<Vector3> projectileVelocityEnumerator = firingPattern.GetProjectileVelocities(comboNumber).GetEnumerator();

                for (int i = 0; i < projectileCount; i++) {
                    projectileOriginEnumerator.MoveNext();
                    projectileVelocityEnumerator.MoveNext();
                    Vector3 origin = transform.TransformPoint(patternTransform.MultiplyPoint3x4(projectileOriginEnumerator.Current));
                    Vector3 velocity = transform.TransformVector(patternTransform.MultiplyVector(projectileVelocityEnumerator.Current));

                    GameObject newProjectileObj = Instantiate(projectile.gameObject);
                    Projectile newProjectile = newProjectileObj.GetComponent<Projectile>();
                    newProjectile.team = team;
                    newProjectile.Fire(origin, velocity, timestamp);
                }
            }
            currentAmmoCount -= ammoCostPerShot;
        }
        lastFireTime = timestamp;
        RaiseOnFireEvent(timestamp);
    }

    public void Reload(float timestamp) {
        if (canReload) {
            currentAmmoCount = ammoCount;
            lastReloadStartTime = timestamp;
        }
    }
}
