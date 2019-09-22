using UnityEngine;

public class MatrixTransform : Modifier {
    public Matrix4x4 matrix;

    public MatrixTransform() { }
    public MatrixTransform(Matrix4x4 matrix) {
        this.matrix = matrix;
    }

    public void Apply(RenderGeometry geometry) {
        geometry.ApplyLinearTransform(matrix, enablePerspective: true);
    }
}
