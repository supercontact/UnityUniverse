using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EdgeSmoothing : Modifier {

    public float smoothRadius = 0f;
    public float angleThreshold = 30f;

    public EdgeSmoothing(float smoothRadius, float angleThreshold = 30f) {
        this.smoothRadius = smoothRadius;
        this.angleThreshold = angleThreshold;
    }

    public void Apply(RenderGeometry geometry) {
        var splitHalfedges = new HashSet<Halfedge>(geometry.halfedges.Where(e => IsValidEdge(e, geometry)));
        var additionalBoundaryHalfedges = new HashSet<Halfedge>();
        var splitVertices = splitHalfedges.Select(e => e.vertex).Distinct().ToArray();

        var splitHalfedgeToNewVertexPosition = new Dictionary<Halfedge, Vector3>();
        var splitHalfedgeToVertexNormal = new Dictionary<Halfedge, Vector3>();
        var splitHalfedgeToPreviousOpposite = new Dictionary<Halfedge, Halfedge>();
        var splitVertexToSurroundingSplitHalfedges = new Dictionary<Vertex, Halfedge[]>();

        // Precalculates new vertex positions. Memorize old normals and old opposites of split halfedges.
        foreach (Halfedge edge in splitHalfedges) {
            Halfedge nextSplitEdge = NextEdgeThat(edge, e => splitHalfedges.Contains(e) || e.isBoundary);
            if (nextSplitEdge != edge) {
                splitHalfedgeToNewVertexPosition[edge] = CalculateVertexPosition(edge, nextSplitEdge.opposite, geometry);
                if (nextSplitEdge.isBoundary) {
                    Halfedge otherEdge = nextSplitEdge.next.opposite;
                    Halfedge otherNextSplitEdge = NextEdgeThat(edge, e => splitHalfedges.Contains(e));
                    splitHalfedgeToNewVertexPosition[otherEdge] = CalculateVertexPosition(otherEdge, otherNextSplitEdge.opposite, geometry);

                    additionalBoundaryHalfedges.Add(otherEdge);
                    splitHalfedgeToVertexNormal[otherEdge] = geometry.GetEffectiveNormal(otherEdge);
                }
            } else {
                splitHalfedgeToNewVertexPosition[edge] = edge.vertex.p;
            }
            splitHalfedgeToVertexNormal[edge] = geometry.GetEffectiveNormal(edge);
            splitHalfedgeToPreviousOpposite[edge] = edge.opposite;
        }

        // Memorize the old connected split halfedges around any given split vertex.
        foreach (Vertex vertex in splitVertices) {
            splitVertexToSurroundingSplitHalfedges[vertex] = vertex.edges.Where(e => splitHalfedges.Contains(e) || additionalBoundaryHalfedges.Contains(e)).ToArray();
        }

        // Split the geometry. A pair of halfedges only needs to be split once.
        foreach (Halfedge edge in splitHalfedges) {
            if (!edge.opposite.isBoundary) {
                geometry.DisconnectEdge(edge);
            }
        }

        // Set new vertex positions
        foreach (Halfedge edge in splitHalfedges.Concat(additionalBoundaryHalfedges)) {
            edge.vertex.p = splitHalfedgeToNewVertexPosition[edge];
        }

        var vertexNormals = splitHalfedgeToVertexNormal.ToDictionary(entry => entry.Key.vertex, entry => entry.Value);
        void AddFace(params Vertex[] faceVertices) {
            Face newFace = geometry.CreateFace(faceVertices.ToArray());
            geometry.SetFaceType(newFace, RenderGeometry.FaceType.Smooth);
            newFace.edges.ForEach(e => geometry.SetNormal(e, vertexNormals[e.vertex]));
        }

        // Create one edge face for each split edge.
        foreach (Halfedge edge in splitHalfedges) {
            if (!edge.opposite.isBoundary) continue;

            Halfedge otherEdge = splitHalfedgeToPreviousOpposite[edge];
            var faceVertices = new List<Vertex>();
            AddIfNotPresent(faceVertices, edge.vertex);
            AddIfNotPresent(faceVertices, edge.prev.vertex);
            AddIfNotPresent(faceVertices, otherEdge.vertex);
            AddIfNotPresent(faceVertices, otherEdge.prev.vertex);

            if (faceVertices.Count >= 3) {
                AddFace(faceVertices.ToArray());
            }
        }

        // Create one corner face for each split vertex.
        foreach (Vertex vertex in splitVertices) {
            Halfedge[] surroundingSplitHalfedges = splitVertexToSurroundingSplitHalfedges[vertex];
            int n = surroundingSplitHalfedges.Length;
            if (n < 3) continue;

            Vertex[] faceVertices = surroundingSplitHalfedges.Select(e => e.vertex).Reverse().ToArray();
            if (n == 3) {
                AddFace(faceVertices.ToArray());
            } else {
                Vertex faceCenter = geometry.CreateVertex(faceVertices.Average(v => v.p));
                vertexNormals[faceCenter] = faceVertices.Average(v => vertexNormals[v]).normalized;
                for (int i = 0; i < n; i++) {
                    AddFace(faceVertices[i], faceVertices[(i + 1) % n], faceCenter);
                }
            }
        }
    }

    private bool IsValidEdge(Halfedge edge, RenderGeometry geometry) {
        if (edge.isBoundary || edge.opposite.isBoundary) return false;
        return
            CalculateEdgeAngleDegree(edge, geometry) >= angleThreshold ||
            CalculateEdgeAngleDegree(edge.opposite, geometry) >= angleThreshold;
    }

    private float CalculateEdgeAngleDegree(Halfedge edge, RenderGeometry geometry) {
        return Vector3.Angle(geometry.GetEffectiveNormal(edge), geometry.GetEffectiveNormal(edge.opposite.prev));
    }

    private Vector3 CalculateVertexPosition(Halfedge edge1, Halfedge edge2, RenderGeometry geometry) {
        Vector3 origin1 = CalculateShiftedEdgeToPosition(edge1, geometry);
        Vector3 origin2 = CalculateShiftedEdgeFromPosition(edge2, geometry);
        return CalculateApproximateIntersection(origin1, edge1.vector, origin2, edge2.vector);
    }

    private Vector3 CalculateShiftedEdgeToPosition(Halfedge edge, RenderGeometry geometry) {
        if (edge.isBoundary || edge.opposite.isBoundary) {
            return edge.vertex.p;
        } else {
            float shiftedDistance = smoothRadius * Mathf.Tan(CalculateEdgeAngleDegree(edge, geometry) * Mathf.Deg2Rad / 2);
            return edge.vertex.p + Vector3.Cross(geometry.GetEffectiveNormal(edge), edge.vector).normalized * shiftedDistance;
        }
    }

    private Vector3 CalculateShiftedEdgeFromPosition(Halfedge edge, RenderGeometry geometry) {
        if (edge.isBoundary || edge.opposite.isBoundary) {
            return edge.opposite.vertex.p;
        } else {
            float shiftedDistance = smoothRadius * Mathf.Tan(CalculateEdgeAngleDegree(edge.opposite, geometry) * Mathf.Deg2Rad / 2);
            return edge.opposite.vertex.p + Vector3.Cross(geometry.GetEffectiveNormal(edge.prev), edge.vector).normalized * shiftedDistance;
        }
    }

    private Vector3 CalculateApproximateIntersection(Vector3 origin1, Vector3 direction1, Vector3 origin2, Vector3 direction2) {
        Vector3 normal = Vector3.Cross(direction1, direction2).normalized;
        if (normal.sqrMagnitude != 0) {
            Vector3 vector12Normal = Vector3.Project(origin2 - origin1, normal);
            Vector3 vector12Plane = Vector3.ProjectOnPlane(origin2 - origin1, normal);
            float distance1 = Vector3.Dot(Vector3.Cross(vector12Plane, direction2), normal) / Vector3.Dot(Vector3.Cross(direction1, direction2), normal);
            return origin1 + distance1 * direction1 + vector12Normal / 2;
        } else {
            return (origin1 + origin2) / 2;
        }
    }

    private Halfedge NextEdgeThat(Halfedge edge, Predicate<Halfedge> predicate) {
        Halfedge result = edge;
        do {
            result = result.next.opposite;
        } while (!predicate(result));
        return result;
    }

    private void AddIfNotPresent<T>(List<T> list, T element) {
        if (!list.Contains(element)) {
            list.Add(element);
        }
    }
}
