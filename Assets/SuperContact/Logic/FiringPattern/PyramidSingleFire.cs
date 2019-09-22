using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PyramidSingleFire", menuName = "Scriptable Objects/FiringPattern/PyramidSingleFire", order = 1)]
public class PyramidSingleFire : FiringPattern {

    public int level = 2;
    public int denseFactor = 1;
    public float centerForwardVelocity = 1f;
    public float cornerForwardVelocity = 0.25f;
    public float cornerOutwardVelocity = 0.4f;

    public override int GetProjectileCount(int comboNumber) {
        return 1 + 2 * level * (level + 1) * denseFactor; 
    }

    public override IEnumerable<Vector3> GetProjectileOrigins(int comboNumber) {
        while (true) {
            yield return Vector3.zero;
        }
    }

    public override IEnumerable<Vector3> GetProjectileVelocities(int comboNumber) {
        yield return centerForwardVelocity * Vector3.forward;

        for (int l = 1; l <= level; l++) {
            Vector3 cornerVelocity =
                Vector3.Lerp(
                    centerForwardVelocity * Vector3.forward,
                    new Vector3(cornerOutwardVelocity, 0, cornerForwardVelocity),
                    l / (float)level);

            for (int side = 0; side < 4; side++) {
                Vector3 startVelocity = Quaternion.AngleAxis(90 * side, Vector3.forward) * cornerVelocity;
                Vector3 endVelocity = Quaternion.AngleAxis(90, Vector3.forward) * startVelocity;
                for (int i = 0; i < l * denseFactor; i++) {
                    yield return Vector3.Lerp(startVelocity, endVelocity, i / (float)(l * denseFactor));
                }
            }
        }
    }
}
