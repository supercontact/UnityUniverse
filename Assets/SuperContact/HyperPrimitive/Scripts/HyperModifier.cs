using System.Collections.Generic;
using UnityEngine;
using SuperContact.MathExpression;

public class HyperModifier : MonoBehaviour {

    public enum HyperModifierType {
        CommonTransform = 100,
        MatrixTransform = 200,
        BendTransform = 300,
        TwistTransform = 400,
        CustomTransform = 900,
        FaceCurving = 1000,
        FaceMerging = 1050,
        EdgeSmoothing = 1100,
    }

    public HyperModifierType type = HyperModifierType.CommonTransform;

    public Vector3 translation = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public Vector3 pivot = Vector3.zero;
    public Vector3 axis1 = Vector3.forward;
    public Vector3 axis2 = Vector3.up;
    public Vector3 direction = Vector3.up;
    public float curvature = 0;
    public float angle = 0;
    public float angleThreshold = 30;
    public float angleThresholdSmall = 1;
    public float smoothRadius = 0.01f;
    public string exprX = "x", exprY = "y", exprZ = "z";
    public float m00 = 1, m01 = 0, m02 = 0, m03 = 0;
    public float m10 = 0, m11 = 1, m12 = 0, m13 = 0;
    public float m20 = 0, m21 = 0, m22 = 1, m23 = 0;
    public float m30 = 0, m31 = 0, m32 = 0, m33 = 1;

    public string message = "";

    void OnValidate() {
        message = "";
        UpdateMesh();
    }

    public void Apply(RenderGeometry geometry) {
        if (type == HyperModifierType.CommonTransform) {
            new CommonTransform(translation, rotation, scale, pivot).Apply(geometry);
        } else if (type == HyperModifierType.MatrixTransform) {
            new MatrixTransform(new Matrix4x4(new Vector4(m00, m10, m20, m30), new Vector4(m01, m11, m21, m31), new Vector4(m02, m12, m22, m32), new Vector4(m03, m13, m23, m33))).Apply(geometry);
        } else if (type == HyperModifierType.BendTransform) {
            new BendTransform(curvature, axis1, direction, pivot).Apply(geometry);
        } else if (type == HyperModifierType.TwistTransform) {
            new TwistTransform(angle, axis2, pivot).Apply(geometry);
        } else if (type == HyperModifierType.CustomTransform) {
            try {
                new CustomTransform(exprX, exprY, exprZ).Apply(geometry);
            } catch (ExpressionParseException e) {
                message = e.Message;
            }
        } else if (type == HyperModifierType.FaceCurving) {
            new FaceCurving(curvature).Apply(geometry);
        } else if (type == HyperModifierType.FaceMerging) {
            new FaceMerging(angleThresholdSmall).Apply(geometry);
        } else if (type == HyperModifierType.EdgeSmoothing) {
            new EdgeSmoothing(smoothRadius, angleThreshold).Apply(geometry);
        }
    }

    public void UpdateMesh() {
        var hyperPrimitive = GetComponent<HyperPrimitive>();
        if (hyperPrimitive != null) {
            hyperPrimitive.UpdateMesh();
        }
    }
}
