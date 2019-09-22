using System.Collections.Generic;
using UnityEngine;

public class FaceMerging : Modifier {

    public float angleThreshold = 1f;

    public FaceMerging(float angleThreshold) {
        this.angleThreshold = angleThreshold;
    }

    public void Apply(RenderGeometry geometry) {
        foreach (Halfedge edge in new List<Halfedge>(geometry.halfedges)) {
            if (edge.index < 0 || edge.isBoundary || edge.opposite.isBoundary) continue;

            float angle1 = Vector3.Angle(geometry.GetEffectiveNormal(edge), geometry.GetEffectiveNormal(edge.opposite.prev));
            if (angle1 > angleThreshold) continue;
            float angle2 = Vector3.Angle(geometry.GetEffectiveNormal(edge.opposite), geometry.GetEffectiveNormal(edge.prev));
            if (angle2 > angleThreshold) continue;

            geometry.MergeFaces(edge);
        }

        foreach (Vertex vertex in new List<Vertex>(geometry.vertices)) {
            List<Halfedge> edges = vertex.edges;
            if (edges.Count != 2) continue;

            if (Vector3.Angle(edges[0].vector, edges[1].opposite.vector) < angleThreshold) {
                geometry.MergeEdges(vertex);
            }
        }
    }
}
