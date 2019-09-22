using UnityEngine;

public class CommonTransform : Modifier {
    public Vector3 translation;
    public Vector3 eularRotation = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public Vector3 pivot = Vector3.zero;

    public CommonTransform() { }
    public CommonTransform(Vector3 translation, Vector3 eularRotation, Vector3 scale, Vector3 pivot) {
        this.translation = translation;
        this.eularRotation = eularRotation;
        this.scale = scale;
        this.pivot = pivot;
    }

    public void Apply(RenderGeometry geometry) {
        geometry.ApplyLinearTransform(Matrix4x4.Translate(pivot) * Matrix4x4.TRS(translation, Quaternion.Euler(eularRotation), scale) * Matrix4x4.Translate(-pivot));
    }
}
