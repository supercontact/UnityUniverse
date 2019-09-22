using System.Collections.Generic;
using UnityEngine;

public class FaceCurving : Modifier {

    public float curvature = 0f;

    public FaceCurving(float curvature) {
        this.curvature = curvature;
    }

    public void Apply(RenderGeometry geometry) {
        foreach (Face face in geometry.faces) {
            Vector3 faceCenter = face.CalculateCenter();

            foreach (Halfedge edge in face.edges) {
                geometry.SetNormal(edge, geometry.GetEffectiveNormal(edge) + (edge.vertex.p - faceCenter).normalized * curvature);
            }
            geometry.SetFaceType(face, RenderGeometry.FaceType.Smooth);
        }
    }
}
