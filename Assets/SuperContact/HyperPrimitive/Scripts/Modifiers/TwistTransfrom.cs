using UnityEngine;

public class TwistTransform : Modifier {

    public float angle = 0;
    public Vector3 axis = Vector3.up;
    public Vector3 pivot = Vector3.zero;

    public TwistTransform() { }
    public TwistTransform(float angle, Vector3 axis, Vector3 pivot) {
        this.angle = angle;
        this.axis = axis;
        this.pivot = pivot;
    }

    public void Apply(RenderGeometry geometry) {
        if (angle == 0) return;

        Vector3 transform(Vector3 pos, out Matrix4x4 localTransform) {
            Vector3 v = pos - pivot;
            Quaternion rotation = Quaternion.AngleAxis(Vector3.Dot(v, axis.normalized) * angle, axis);

            localTransform = Matrix4x4.Rotate(rotation);
            return pivot + rotation * v;
        }
        geometry.ApplyPositionTransform(transform);
    }
}
