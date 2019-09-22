using UnityEngine;

public class BendTransform : Modifier {

    public float curvature = 0;
    public Vector3 axis = Vector3.forward;
    public Vector3 direction = Vector3.up;
    public Vector3 pivot = Vector3.zero;

    public BendTransform() { }
    public BendTransform(float curvature, Vector3 axis, Vector3 direction, Vector3 pivot) {
        this.curvature = curvature;
        this.axis = axis;
        this.direction = direction;
        this.pivot = pivot;
    }

    public void Apply(RenderGeometry geometry) {
        if (curvature == 0) return;

        Vector3 dir1 = Vector3.Cross(direction, axis).normalized;
        Vector3 dir2 = Vector3.Cross(axis, dir1).normalized;
        Vector3 dir3 = axis.normalized;
        Vector3 transform(Vector3 pos, out Matrix4x4 localTransform) {
            Vector3 v = pos - pivot;
            float v1 = Vector3.Dot(v, dir1);
            float v2 = Vector3.Dot(v, dir2);
            float v3 = Vector3.Dot(v, dir3);

            float sin = Mathf.Sin(v1 * curvature);
            float cos = Mathf.Cos(v1 * curvature);
            float v1t = sin / curvature - v2 * sin;
            float v2t = (1 - cos) / curvature + v2 * cos;

            localTransform = Matrix4x4.Rotate(Quaternion.AngleAxis(v1 * curvature * Mathf.Rad2Deg, dir3));
            return dir1 * v1t + dir2 * v2t + dir3 * v3;
        }
        geometry.ApplyPositionTransform(transform);
    }
}
