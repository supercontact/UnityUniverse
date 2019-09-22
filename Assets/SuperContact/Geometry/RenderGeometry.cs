using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary> The halfedge representation of a mesh. </summary>
public class RenderGeometry : Geometry {
    /// <summary> Face types that decides how normals are calculated on the face. </summary>
    public enum FaceType {
        Smooth,          // Normals are interpolated on each face depending on vertex normals.
        Directinal1,     // Edge 0, 2 of the quad face is smooth, others create separated normals (auto-calculated).
        Directinal2,     // Edge 1, 3 of the quad face is smooth, others create separated normals (auto-calculated).
        Polygonal,       // Normals are constant (auto-calculated) on each face.
        Triangular,      // Normals are constant (auto-calculated) on each triangle the face is composed of.
    }

    /// <summary> The global surface facing types. </summary>
    public enum Facing {
        Normal,
        Flipped,
        TwoSided,
    }

    /// <summary> The global surface smoothing types. </summary>
    public enum GlobalSurfaceType {
        Normal,
        AutoSmoothAll,
        //AutoSmoothByGroups,
        HardPolygonAll,
        HardTriangleAll,
        SplitTriangleAll,
    }

    public Dictionary<Face, FaceType> faceTypes = new Dictionary<Face, FaceType>();

    public Dictionary<Halfedge, Vector3> normals = new Dictionary<Halfedge, Vector3>();
    public Dictionary<Halfedge, Vector3> tangents = new Dictionary<Halfedge, Vector3>();
    public Dictionary<Halfedge, Vector2> uv = new Dictionary<Halfedge, Vector2>();

    public RenderGeometry() {}
    public RenderGeometry(Mesh mesh) {
        FromMesh(mesh);
    }

    public FaceType GetFaceType(Face face) {
        return faceTypes.TryGetValue(face, out FaceType result) ? result : FaceType.Polygonal;
    }

    public void SetFaceType(Face face, FaceType faceType) {
        faceTypes[face] = faceType;
    }

    public Vector3 GetNormal(Halfedge e) {
        return normals.TryGetValue(e, out Vector3 result) ? result : Vector3.zero;
    }

    public Vector3 GetEffectiveNormal(Halfedge e) {
        if (e.isBoundary) return Vector3.zero;

        FaceType faceType = GetFaceType(e.face);
        switch (faceType) {
        case FaceType.Smooth:
            return normals[e];
        case FaceType.Polygonal:
        case FaceType.Triangular:
            return e.face.CalculateNormal();
        case FaceType.Directinal1:
        case FaceType.Directinal2:
            List<Halfedge> edges = e.face.edges;
            return CalculateDirectionalNormal(e, GetFaceType(e.face), edges.IndexOf(e), edges.Count);
        default:
            return Vector3.zero;
        }
    }

    public void SetNormal(Halfedge e, Vector3 normal) {
        normals[e] = normal;
    }
    public void SetNormal(Vertex v, Vector3 normal, int surfaceGroup = 0) {
        foreach (Halfedge e in v.edges) {
            if (e.face != null && e.face.surfaceGroup == surfaceGroup) {
                normals[e] = normal;
            }
        }
    }

    public Vector3 GetTangent(Halfedge e) {
        return tangents.TryGetValue(e, out Vector3 result) ? result : Vector3.zero;
    }

    public void SetTangent(Halfedge e, Vector3 tangent) {
        tangents[e] = tangent;
    }

    public Vector3 GetUV(Halfedge e) {
        return uv.TryGetValue(e, out Vector2 result) ? result : Vector2.zero;
    }

    public void SetUV(Halfedge e, Vector2 uv) {
        this.uv[e] = uv;
    }
    public void SetUV(Vertex v, Vector2 uv, int surfaceGroup = 0) {
        foreach (Halfedge e in v.edges) {
            if (e.face != null && e.face.surfaceGroup == surfaceGroup) {
                this.uv[e] = uv;
            }
        }
    }

    public override void ConnectEdges(Halfedge e1, Halfedge e2) {
        if (tangents.ContainsKey(e1) && !tangents.ContainsKey(e2.opposite)) {
            tangents[e2.opposite] = tangents[e1];
        }
        if (tangents.ContainsKey(e2) && !tangents.ContainsKey(e1.opposite)) {
            tangents[e1.opposite] = tangents[e2];
        }
        base.ConnectEdges(e1, e2);
    }

    public override void DisconnectEdge(Halfedge edge) {
        Halfedge previousOpposite = edge.opposite;
        base.DisconnectEdge(edge);
        if (tangents.ContainsKey(edge)) {
            tangents[previousOpposite.opposite] = tangents[edge];
        }
        if (tangents.ContainsKey(previousOpposite)) {
            tangents[edge.opposite] = tangents[previousOpposite];
        }
    }

    public override Halfedge SplitFace(Vertex v1, Vertex v2) {
        Halfedge edge2 = base.SplitFace(v1, v2);
        Halfedge edge1 = edge2.opposite;

        SetFaceType(edge2.face, GetFaceType(edge1.face));
        if (normals.ContainsKey(edge2.prev)) {
            normals[edge1] = normals[edge2.prev];
        }
        if (normals.ContainsKey(edge1.prev)) {
            normals[edge2] = normals[edge1.prev];
        }
        return edge2;
    }

    public override void ApplyLinearTransform(Matrix4x4 transform, bool enablePerspective = false) {
        base.ApplyLinearTransform(transform, enablePerspective);
        if (transform.IsTranslationOnly()) return;

        Matrix4x4 normalTransform = transform.CofactorMatrix();
        foreach (Halfedge e in new List<Halfedge>(normals.Keys)) {
            normals[e] = normalTransform.MultiplyVector(normals[e]).normalized;
        }
        foreach (Halfedge e in new List<Halfedge>(tangents.Keys)) {
            tangents[e] = transform.MultiplyVector(tangents[e]).normalized;
        }
    }

    public delegate Vector3 PositionAndLocalSpaceTransform(Vector3 pos, out Matrix4x4 localTransform);
    public void ApplyPositionTransform(PositionAndLocalSpaceTransform transform) {
        foreach (Vertex v in vertices) {
            Vector3 newPosition = transform(v.p, out Matrix4x4 localTransform);
            Matrix4x4 localNormalTransform = localTransform.CofactorMatrix();
            foreach (Halfedge e in v.edges) {
                if (normals.ContainsKey(e)) {
                    normals[e] = (localNormalTransform * normals[e]).normalized;
                }
                if (tangents.ContainsKey(e)) {
                    tangents[e] = (localTransform * tangents[e]).normalized;
                }
            }
            v.p = newPosition;
        }
    }

    public override void ApplySpaceWarp(SpaceWarp warp) {
        foreach (Halfedge e in new List<Halfedge>(normals.Keys)) {
            normals[e] = warp.EvaluateNormal(e.vertex.p, normals[e]);
        }
        foreach (Halfedge e in new List<Halfedge>(tangents.Keys)) {
            tangents[e] = warp.EvaluateTangent(e.vertex.p, tangents[e]);
        }
        base.ApplySpaceWarp(warp);
    }

    /// <summary> Adds the other geometry to this geometry. The other geometry should not be used afterwards </summary>
    public void CombineGeometry(RenderGeometry other) {
        foreach (Face f in other.faceTypes.Keys) {
            faceTypes[f] = other.faceTypes[f];
        }
        foreach (Halfedge e in other.normals.Keys) {
            normals[e] = other.normals[e];
        }
        foreach (Halfedge e in other.tangents.Keys) {
            tangents[e] = other.tangents[e];
        }
        foreach (Halfedge e in other.uv.Keys) {
            uv[e] = other.uv[e];
        }
        base.CombineGeometry(other);
    }

    /// <summary> Clears the geometry. </summary>
    public override void Clear() {
        base.Clear();
        faceTypes.Clear();
        normals.Clear();
        tangents.Clear();
        uv.Clear();
    }

    /// <summary> Construct the geometry from a mesh. </summary>
    public void FromMesh(Mesh mesh) {
        Vector3[] meshVerts = mesh.vertices;
        Vector3[] meshNormals = mesh.normals;
        Vector2[] meshUV = mesh.uv;
        int[] meshFaces = mesh.triangles;

        Clear();

        // Build vertices
        var vertexMap = new Dictionary<Vector3, Vertex>();
        var vertexHalfedges = new Dictionary<Vertex, List<Halfedge>>();
        foreach (Vector3 meshVert in meshVerts) {
            if (!vertexMap.ContainsKey(meshVert)) {
                var vertex = new Vertex(meshVert);
                AddVertexToList(vertex);
                vertexMap[meshVert] = vertex;
                vertexHalfedges[vertex] = new List<Halfedge>();
            }
        }

        // Build faces and halfedges, linked to vertices
        for (int i = 0; i < meshFaces.Length / 3; i++) {
            Face trig = new Face();
            Halfedge e1 = new Halfedge(), e2 = new Halfedge(), e3 = new Halfedge();
            e1.face = trig;
            e1.next = e2;
            e1.prev = e3;
            e1.vertex = vertexMap[meshVerts[meshFaces[3 * i]]];
            e1.vertex.edge = e1;
            e2.face = trig;
            e2.next = e3;
            e2.prev = e1;
            e2.vertex = vertexMap[meshVerts[meshFaces[3 * i + 1]]];
            e2.vertex.edge = e2;
            e3.face = trig;
            e3.next = e1;
            e3.prev = e2;
            e3.vertex = vertexMap[meshVerts[meshFaces[3 * i + 2]]];
            e3.vertex.edge = e3;
            trig.edge = e1;

            AddFaceToList(trig);
            AddHalfedgeToList(e1);
            AddHalfedgeToList(e2);
            AddHalfedgeToList(e3);

            vertexHalfedges[e1.vertex].Add(e1);
            vertexHalfedges[e2.vertex].Add(e2);
            vertexHalfedges[e3.vertex].Add(e3);

            SetNormal(e1, meshNormals[meshFaces[3 * i]]);
            SetNormal(e2, meshNormals[meshFaces[3 * i + 1]]);
            SetNormal(e3, meshNormals[meshFaces[3 * i + 2]]);
            SetUV(e1, meshUV[meshFaces[3 * i]]);
            SetUV(e2, meshUV[meshFaces[3 * i + 1]]);
            SetUV(e3, meshUV[meshFaces[3 * i + 2]]);
        }

        // Set the corresponding opposite to each halfedge
        foreach (Vertex vertex in vertices) {
            foreach (Halfedge edge in vertexHalfedges[vertex]) {
                if (edge.opposite == null) {
                    Vertex vt = edge.prev.vertex;
                    bool foundOpposite = false;
                    foreach (Halfedge e in vertexHalfedges[vt]) {
                        if (e.prev.vertex == vertex) {
                            if (foundOpposite) {
                                throw new Exception("Edge shared by 3 faces, not manifold!");
                            }
                            edge.opposite = e;
                            foundOpposite = true;
                        }
                    }

                    // If no opposite, we are on a boundary
                    if (edge.opposite == null) {
                        edge.opposite = new Halfedge();
                        edge.opposite.opposite = edge;
                        edge.opposite.vertex = edge.prev.vertex;
                        AddHalfedgeToList(edge.opposite);
                    }
                }
            }
        }

        // Reconnect all newly created halfedges on a boundary
        for (int i = 0; i < halfedges.Count; i++) {
            if (halfedges[i].next == null) {

                // Connect all halfedges of this boundary
                Halfedge first = halfedges[i], temp = halfedges[i];
                do {
                    Halfedge next = temp.opposite;
                    while (next.prev != null) {
                        next = next.prev.opposite;
                    }
                    temp.next = next;
                    next.prev = temp;
                    temp = next;
                } while (temp != first);
            }
        }
        // Debug.Log($"Geometry created! Geometry has {vertices.Count} vertices, {halfedges.Count} halfedges and {faces.Count} faces, Eular number = {(vertices.Count - halfedges.Count / 2 + faces.Count)}");
    }

    public Mesh ToMesh(Facing facing = Facing.Normal, GlobalSurfaceType surfaceType = GlobalSurfaceType.Normal) {
        Mesh mesh = new Mesh();
        ToMesh(mesh, facing, surfaceType);
        return mesh;
    }
    public void ToMesh(Mesh mesh, Facing facing = Facing.Normal, GlobalSurfaceType surfaceType = GlobalSurfaceType.Normal) {
        var indexByVertexKey = new Dictionary<Vertex, SortedDictionary<VertexKey, int>>();
        var trigs = new List<int>();
        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var meshuv = new List<Vector2>();

        var vertexKeyComparer = new VertexKeyComparer();
        int MeshVertex(Halfedge e, Vector3 normal) {
            if (!indexByVertexKey.TryGetValue(e.vertex, out SortedDictionary<VertexKey, int> indices)) {
                indices = new SortedDictionary<VertexKey, int>(vertexKeyComparer);
                indexByVertexKey[e.vertex] = indices;
            }
            VertexKey key = new VertexKey(normal, GetUV(e));
            if (!indices.TryGetValue(key, out int index)) {
                index = verts.Count;
                verts.Add(e.vertex.p);
                norms.Add(normal);
                meshuv.Add(GetUV(e));
                indices.Add(key, index);
            }
            return index;
        }
        int MeshVertexEdgeNormal(Halfedge e) {
            return MeshVertex(e, GetNormal(e));
        }

        if (surfaceType == GlobalSurfaceType.AutoSmoothAll || surfaceType == GlobalSurfaceType.SplitTriangleAll) {
            // No vertex duplication.
            foreach (Face f in faces) {
                List<Halfedge> edges = f.edges;
                int p1 = 0;
                int p2 = edges.Count - 1;
                while (p2 - p1 > 1) {
                    trigs.Add(edges[p1].vertex.index);
                    trigs.Add(edges[p1 + 1].vertex.index);
                    trigs.Add(edges[p2].vertex.index);
                    p1++;
                    if (p2 - p1 <= 1) break;
                    trigs.Add(edges[p1].vertex.index);
                    trigs.Add(edges[p2 - 1].vertex.index);
                    trigs.Add(edges[p2].vertex.index);
                    p2--;
                }
            }
            mesh.vertices = vertices.Select(v => v.p).ToArray();
            mesh.triangles = trigs.ToArray();
        } else {
            foreach (Face f in faces) {
                FaceType faceType = GetFaceType(f);
                List<Halfedge> edges = f.edges;
                if (surfaceType == GlobalSurfaceType.HardPolygonAll || faceType == FaceType.Polygonal) {
                    Vector3 normal = f.CalculateNormal();
                    int p1 = 0;
                    int p2 = edges.Count - 1;
                    while (p2 - p1 > 1) {
                        trigs.Add(MeshVertex(edges[p1], normal));
                        trigs.Add(MeshVertex(edges[p1 + 1], normal));
                        trigs.Add(MeshVertex(edges[p2], normal));
                        p1++;
                        if (p2 - p1 <= 1) break;
                        trigs.Add(MeshVertex(edges[p1], normal));
                        trigs.Add(MeshVertex(edges[p2 - 1], normal));
                        trigs.Add(MeshVertex(edges[p2], normal));
                        p2--;
                    }
                } else if (surfaceType == GlobalSurfaceType.HardTriangleAll || faceType == FaceType.Triangular) {
                    int p1 = 0;
                    int p2 = edges.Count - 1;
                    while (p2 - p1 > 1) {
                        Vector3 normal1 = Vector3.Cross(edges[p1 + 1].vertex.p - edges[p1].vertex.p, edges[p2].vertex.p - edges[p1].vertex.p).normalized;
                        trigs.Add(MeshVertex(edges[p1], normal1));
                        trigs.Add(MeshVertex(edges[p1 + 1], normal1));
                        trigs.Add(MeshVertex(edges[p2], normal1));
                        p1++;
                        if (p2 - p1 <= 1) break;
                        Vector3 normal2 = Vector3.Cross(edges[p2 - 1].vertex.p - edges[p1].vertex.p, edges[p2].vertex.p - edges[p1].vertex.p).normalized;
                        trigs.Add(MeshVertex(edges[p1], normal2));
                        trigs.Add(MeshVertex(edges[p2 - 1], normal2));
                        trigs.Add(MeshVertex(edges[p2], normal2));
                        p2--;
                    }
                } else if (faceType == FaceType.Smooth) {
                    int p1 = 0;
                    int p2 = edges.Count - 1;
                    while (p2 - p1 > 1) {
                        trigs.Add(MeshVertexEdgeNormal(edges[p1]));
                        trigs.Add(MeshVertexEdgeNormal(edges[p1 + 1]));
                        trigs.Add(MeshVertexEdgeNormal(edges[p2]));
                        p1++;
                        if (p2 - p1 <= 1) break;
                        trigs.Add(MeshVertexEdgeNormal(edges[p1]));
                        trigs.Add(MeshVertexEdgeNormal(edges[p2 - 1]));
                        trigs.Add(MeshVertexEdgeNormal(edges[p2]));
                        p2--;
                    }
                } else if (faceType == FaceType.Directinal1 || faceType == FaceType.Directinal2) {
                    if (edges.Count > 4) {
                        throw new Exception("Directional smooth can only apply to quad or triangle faces!");
                    }
                    Vector3[] localNormals = edges.Select((e, i) => CalculateDirectionalNormal(e, faceType, i, edges.Count)).ToArray();
                    if (edges.Count == 4) {
                        trigs.Add(MeshVertex(edges[0], localNormals[0]));
                        trigs.Add(MeshVertex(edges[1], localNormals[1]));
                        trigs.Add(MeshVertex(edges[3], localNormals[3]));
                        trigs.Add(MeshVertex(edges[1], localNormals[1]));
                        trigs.Add(MeshVertex(edges[2], localNormals[2]));
                        trigs.Add(MeshVertex(edges[3], localNormals[3]));
                    } else {
                        trigs.Add(MeshVertex(edges[0], localNormals[0]));
                        trigs.Add(MeshVertex(edges[1], localNormals[1]));
                        trigs.Add(MeshVertex(edges[2], localNormals[2]));
                    }
                }
            }
            mesh.vertices = verts.ToArray();
            mesh.triangles = trigs.ToArray();
            mesh.normals = norms.ToArray();
            mesh.uv = meshuv.ToArray();
        }

        if (facing != Facing.Normal || surfaceType == GlobalSurfaceType.SplitTriangleAll) {
            // Extra processing through mesh builder.
            MeshBuilder meshBuilder = new MeshBuilder(mesh);
            if (facing == Facing.Flipped) {
                meshBuilder.Invert();
            } else if (facing == Facing.TwoSided) {
                meshBuilder.AddOtherSide();
            }
            if (surfaceType == GlobalSurfaceType.SplitTriangleAll) {
                meshBuilder.SplitAllTriangles();
            }
            meshBuilder.ToMesh(mesh);
        }

        // Finalize the mesh.
        if (surfaceType == GlobalSurfaceType.AutoSmoothAll) {
            mesh.RecalculateNormals();
        }
        mesh.RecalculateBounds();
    }

    protected override void RemoveVertexFromList(Vertex v) {
        base.RemoveVertexFromList(v);
    }

    protected override void RemoveHalfedgeFromList(Halfedge e) {
        base.RemoveHalfedgeFromList(e);
        normals.Remove(e);
        tangents.Remove(e);
        uv.Remove(e);
    }

    protected override void RemoveFaceFromList(Face f) {
        base.RemoveFaceFromList(f);
    }

    private Vector3 CalculateDirectionalNormal(Halfedge e, FaceType faceType, int edgeIndexInFace, int faceEdgesCount) {
        bool isSmoothEdge = faceEdgesCount == 4 ?
            (faceType == FaceType.Directinal1 ? edgeIndexInFace % 2 == 0 : edgeIndexInFace % 2 == 1) :
            (faceType == FaceType.Directinal1 ? edgeIndexInFace == 0 : edgeIndexInFace > 0);
        if (faceEdgesCount == 3 && edgeIndexInFace == 1) {
            return Vector3.Cross(e.vector, e.next.vector).normalized;
        } else if (isSmoothEdge) {
            //return Vector3.ProjectOnPlane(normals[e], e.vector).normalized;
            //return Vector3.Cross(e.vector, Vector3.ProjectOnPlane(e.next.vector, normals[e])).normalized;
            return Vector3.Cross(e.vector, GetTangent(e.next.opposite)).normalized;
        } else {
            //return Vector3.ProjectOnPlane(normals[e], e.next.vector).normalized;
            //return Vector3.Cross(Vector3.ProjectOnPlane(e.vector, normals[e]), e.next.vector).normalized;
            return Vector3.Cross(e.next.vector, GetTangent(e)).normalized;
        }
    }

    private struct VertexKey {
        public Vector3 normal;
        public Vector2 uv;

        public VertexKey(Vector3 normal, Vector2 uv) {
            this.normal = normal;
            this.uv = uv;
        }
    }

    private class VertexKeyComparer : Comparer<VertexKey> {
        private static readonly ApproximateFloatComparer floatComparer = new ApproximateFloatComparer();

        public override int Compare(VertexKey a, VertexKey b) {
            return Comparer.CombinedCompare(
                floatComparer,
                a.normal.x, a.normal.y, a.normal.z, a.uv.x, a.uv.y,
                b.normal.x, b.normal.y, b.normal.z, b.uv.x, b.uv.y);
        }
    }
}

