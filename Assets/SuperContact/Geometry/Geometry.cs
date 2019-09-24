using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Vertex {
    /// <summary> The index of the vertex in the vertices array. </summary>
    public int index;
    /// <summary> The position of the vertex. </summary>
    public Vector3 p;
    /// <summary> One of the halfedge pointing to the vertex. </summary>
    public Halfedge edge;
    /// <summary> The list of all neighboring halfedges. </summary>
    public List<Halfedge> edges {
        get {
            var edges = new List<Halfedge>();
            Halfedge first = edge, current = edge;
            do {
                edges.Add(current);
                current = current.next.opposite;
            } while (current != first);
            return edges;
        }
    }
    /// <summary> Whether the vertex is on a boundary. </summary>
    public bool isOnBoundary {
        get { return edges.Any(e => e.isBoundary); }
    }

    public Vertex(Vector3 pos) {
        p = pos;
    }

    /// <summary> Calculates the weight of the vertex (A third of the areas of its neighboring triangles). </summary>
    public float CalculateVertexAreaTri() {
        float result = 0;
        foreach (Halfedge edge in edges) {
            if (edge.face.index != -1) {
                result += edge.face.CalculateAreaTri();
            }
        }
        result /= 3;
        return result;
    }

    /// <summary> Calculates the average normal of its neighboring triangles. </summary>
    public Vector3 CalculateNormal() {
        var result = new Vector3();
        foreach (Halfedge edge in edges) {
            if (edge.face.index != -1) {
                result += Vector3.Cross(edge.vector, edge.next.vector).normalized;
            }
        }
        result.Normalize();
        return result;
    }
}


public class Halfedge {
    /// <summary> The index of the halfedge in the halfedges array. </summary>
    public int index;
    /// <summary> The next halfedge this halfedge connects to, which also belongs to the same face. </summary>
    public Halfedge next;
    /// <summary> The previous halfedge that connects to this halfedge, which also belongs to the same face. </summary>
    public Halfedge prev;
    /// <summary> The opposite halfedge with inversed direction, which belongs to the adjacent face. </summary>
    public Halfedge opposite;
    /// <summary> The vertex it points to. </summary>
    public Vertex vertex;
    /// <summary> The face it belongs to. </summary>
	public Face face;
    /// <summary> Whether this halfedge is a boundary </summary>
    public bool isBoundary {
        get { return face == null; }
    }
    public float length {
        get { return (vertex.p - opposite.vertex.p).magnitude; }
    }
    public Vector3 vector {
        get { return vertex.p - opposite.vertex.p; }
    }
    public Vector3 midPoint {
        get { return (vertex.p + opposite.vertex.p) * 0.5f; }
    }
}


public class Face {
    /// <summary> The index of the face in the faces array. </summary>
    public int index;
    /// <summary> One of the halfedge which belongs to the face. </summary>
    public Halfedge edge;
    /// <summary> The list of all neighboring halfedges. </summary>
    public List<Halfedge> edges {
        get {
            var edges = new List<Halfedge>();
            Halfedge first = edge, current = edge;
            do {
                edges.Add(current);
                current = current.next;
            } while (current != first);
            return edges;
        }
    }
    /// <summary> The list of all vertices on this face. </summary>
    public List<Vertex> vertices {
        get { return edges.Select(e => e.prev.vertex).ToList(); }
    }
    /// <summary> The surface group number of this face. </summary>
    public int surfaceGroup = 0;

    /// <summary> Calculates the area of the triangle face. </summary>
    public float CalculateAreaTri() {
        return Vector3.Cross(edge.vector, edge.next.vector).magnitude / 2;
    }

    /// <summary> Calculates the normal of the triangle face. </summary>
    public Vector3 CalculateNormalTri() {
        return Vector3.Cross(edge.vector, edge.next.vector).normalized;
    }

    /// <summary> Calculates the average normal of the polygon face. </summary>
    public Vector3 CalculateNormal() {
        Vector3 normal = Vector3.zero;
        Halfedge e = edge;
        do {
            normal += Vector3.Cross(e.vector, e.next.vector);
            e = e.next;
        } while (e != edge);
        return normal.normalized;
    }

    /// <summary> Calculates the center of the face. </summary>
    public Vector3 CalculateCenter() {
        var c = Vector3.zero;
        foreach (Halfedge edge in edges) {
            c += edge.vertex.p;
        }
        return c / edges.Count;
    }
}

public class Geometry {
    public List<Vertex> vertices = new List<Vertex>();
    public List<Halfedge> halfedges = new List<Halfedge>();
    public List<Face> faces = new List<Face>();

    /// <summary> Creates a new vertex. </summary>
    public Vertex CreateVertex(Vector3 pos) {
        Vertex v = new Vertex(pos);
        AddVertexToList(v);
        return v;
    }

    /// <summary> Creates a face from existing vertices. Do make sure that the geometry is still a manifold after this operation. </summary>
    public Face CreateFace(params Vertex[] verts) {
        int n = verts.Length;
        var perimeter = new Halfedge[n];
        var newBoundaryHalfedges = new HashSet<Halfedge>();
        Halfedge lastEdge = null;

        Face face = new Face();
        AddFaceToList(face);

        // Find or create all the halfedges of the face and their opposite halfedge.
        // Prev and next fields for new halfedges are not set in this step.
        for (int i = 0; i < n; i++) {
            Vertex from = verts[i];
            Vertex to = verts[(i + 1) % n];
            perimeter[i] = FindBoundaryHalfedge(from, to);
            if (perimeter[i] == null) {
                // Handle special case of self-touching face.
                foreach (Halfedge e in newBoundaryHalfedges) {
                    if (e.vertex == to && e.opposite.vertex == from && e.opposite != lastEdge) {
                        perimeter[i] = e;
                        newBoundaryHalfedges.Remove(e);
                        break;
                    }
                }
            }
            if (perimeter[i] == null) {
                // Create face halfedge and its opposite.
                perimeter[i] = new Halfedge();
                perimeter[i].vertex = to;
                perimeter[i].opposite = new Halfedge();
                perimeter[i].opposite.vertex = from;
                perimeter[i].opposite.opposite = perimeter[i];
                AddHalfedgeToList(perimeter[i]);
                AddHalfedgeToList(perimeter[i].opposite);
                newBoundaryHalfedges.Add(perimeter[i].opposite);
            }
            perimeter[i].face = face;
            lastEdge = perimeter[i];
        }
        // Add reference to the halfedge for new vertices and the new face.
        for (int i = 0; i < n; i++) {
            if (perimeter[i].vertex.edge == null) {
                perimeter[i].vertex.edge = perimeter[i];
            }
        }
        face.edge = perimeter[0];

        // Set next and prev fields for all affected halfedges.
        for (int i = 0; i < n; i++) {
            Halfedge current = perimeter[i];
            Halfedge next = perimeter[(i + 1) % n];
            if (current.next == null && next.prev == null) {
                // Current and next are all new edges.
                current.next = next;
                next.prev = current;
                if (current.vertex.edge == current) {
                    // Normal case.
                    current.opposite.prev = next.opposite;
                    next.opposite.next = current.opposite;
                } else {
                    // The new triangle is touching the existing surface at this vertex but not connected via edges.
                    // We handle only the case that this vertex is having only one connected fan before adding the face.
                    Halfedge otherBoundary1 = current.vertex.edges.Find(e => e.isBoundary);
                    Halfedge otherBoundary2 = otherBoundary1.next;
                    current.opposite.prev = otherBoundary1;
                    otherBoundary1.next = current.opposite;
                    next.opposite.next = otherBoundary2;
                    otherBoundary2.prev = next.opposite;
                }
            } else if (current.next == null && next.prev != null) {
                // Current is new edge and next is existing boundary edge.
                next.prev.next = current.opposite;
                current.opposite.prev = next.prev;
                current.next = next;
                next.prev = current;
            } else if (current.next != null && next.prev == null) {
                // Current is existing boundary edge and next is new edge.
                current.next.prev = next.opposite;
                next.opposite.next = current.next;
                current.next = next;
                next.prev = current;
            }
        }
        return face;
    }

    /// <summary> Creates a face from existing vertices, with option to inverse the facing. </summary>
    public Face CreateFace(bool inverse, params Vertex[] verts) {
        return CreateFace(inverse ? verts.Reverse().ToArray() : verts);
    }

    /// <summary> Finds the specific halfedge if it exists. </summary>
    public Halfedge FindHalfedge(Vertex from, Vertex to) {
        if (to.edge == null) return null;
        return to.edges.Find(e => e.opposite.vertex == from);
    }

    /// <summary> Finds the specific boundary halfedge if it exists. </summary>
    public Halfedge FindBoundaryHalfedge(Vertex from, Vertex to) {
        if (to.edge == null) return null;
        return to.edges.Find(e => e.opposite.vertex == from && e.isBoundary);
    }

    public List<Halfedge> GetBoundaryRing(Vertex boundaryVertex) {
        Halfedge currentEdge = boundaryVertex.edges.Find(e => e.isBoundary);
        if (currentEdge == null) {
            throw new Exception("Vertex is not on boundary!");
        }
        var boundary = new List<Halfedge>();
        do {
            currentEdge = currentEdge.next;
            boundary.Add(currentEdge);
        } while (currentEdge.vertex != boundaryVertex);
        return boundary;
    }

    /// <summary> Connects a pair of boundary halfedges to form a connected surface. </summary>
    public virtual void ConnectEdges(Halfedge edge1, Halfedge edge2) {
        if (!edge1.isBoundary || !edge2.isBoundary) throw new Exception("Cannot merge halfedges that are not part of a boundary!");
        Vertex v1 = edge1.vertex;
        Vertex v1t = edge2.opposite.vertex;
        Vertex v2 = edge2.vertex;
        Vertex v2t = edge1.opposite.vertex;
        if (v1 != v1t) {
            foreach (Halfedge e in v1t.edges) {
                e.vertex = v1;
            }
            RemoveVertexFromList(v1t);
            edge1.next.prev = edge2.prev;
            edge2.prev.next = edge1.next;
        }
        if (v2 != v2t) {
            foreach (Halfedge e in v2t.edges) {
                e.vertex = v2;
            }
            RemoveVertexFromList(v2t);
            edge1.prev.next = edge2.next;
            edge2.next.prev = edge1.prev;
        }
        edge1.opposite.opposite = edge2.opposite;
        edge2.opposite.opposite = edge1.opposite;
        RemoveHalfedgeFromList(edge1);
        RemoveHalfedgeFromList(edge2);
    }

    /// <summary> Disconnects a halfedge from its opposite so that they become on separate boundaries. </summary>
    public virtual void DisconnectEdge(Halfedge edge) {
        Halfedge edge1 = edge;
        Halfedge edge2 = edge.opposite;
        if (edge1.isBoundary || edge2.isBoundary) throw new Exception("Cannot split edges on a boundary!");

        Halfedge edge1NextBoundary = FindNextHalfedgeAroundVertexThat(edge1, e => e.isBoundary);
        Halfedge edge2NextBoundary = FindNextHalfedgeAroundVertexThat(edge2, e => e.isBoundary);

        Halfedge edge1Opposite = new Halfedge();
        Halfedge edge2Opposite = new Halfedge();
        edge1Opposite.next = edge2Opposite;
        edge1Opposite.prev = edge2Opposite;
        edge1Opposite.opposite = edge1;
        edge1Opposite.vertex = edge1.prev.vertex;
        edge2Opposite.next = edge1Opposite;
        edge2Opposite.prev = edge1Opposite;
        edge2Opposite.opposite = edge2;
        edge2Opposite.vertex = edge2.prev.vertex;

        edge1.opposite = edge1Opposite;
        edge2.opposite = edge2Opposite;
        AddHalfedgeToList(edge1Opposite);
        AddHalfedgeToList(edge2Opposite);

        if (edge1NextBoundary != null) {
            Vertex newVertex1 = CreateVertex(edge1.vertex.p);
            newVertex1.edge = edge1;
            AddVertexToList(newVertex1);
            edge1.vertex.edge = edge2.prev;

            Halfedge edgeToUpdate = edge1;
            while (!edgeToUpdate.isBoundary) {
                edgeToUpdate.vertex = newVertex1;
                edgeToUpdate = edgeToUpdate.next.opposite;
            }
            edgeToUpdate.vertex = newVertex1;
            edgeToUpdate.next.prev = edge2Opposite;
            edge2Opposite.next = edgeToUpdate.next;
            edgeToUpdate.next = edge1Opposite;
            edge1Opposite.prev = edgeToUpdate;
        }
        if (edge2NextBoundary != null) {
            Vertex newVertex2 = CreateVertex(edge2.vertex.p);
            newVertex2.edge = edge2;
            AddVertexToList(newVertex2);
            edge2.vertex.edge = edge1.prev;

            Halfedge edgeToUpdate = edge2;
            while (!edgeToUpdate.isBoundary) {
                edgeToUpdate.vertex = newVertex2;
                edgeToUpdate = edgeToUpdate.next.opposite;
            }
            edgeToUpdate.vertex = newVertex2;
            edgeToUpdate.next.prev = edge1Opposite;
            edge1Opposite.next = edgeToUpdate.next;
            edgeToUpdate.next = edge2Opposite;
            edge2Opposite.prev = edgeToUpdate;
        }
    }

    /// <summary> Merges two neighboring faces as one given one of their connecting halfedge. </summary>
    public virtual void MergeFaces(Halfedge edge) {
        Halfedge oppo = edge.opposite;
        Face face1 = edge.face;
        Face face2 = oppo.face;
        Asserts.AssertThat(
            (face1 != face2 && edge.next != oppo && edge.prev != oppo) ||
            (face1 == face2 && (edge.next == oppo ^ edge.prev == oppo)),
            $"Merge will lead to non-valid geometry! face1 {(face1 == face2 ? "==" : "!=")} face2, edge.next {(edge.next == oppo ? "==" : "!=")} oppo, edge.prev {(edge.prev == oppo ? "==" : "!=")} oppo.");

        if (face1 != face2) {
            face2.edges.ForEach(e => e.face = face1);
            RemoveFaceFromList(face2);
        }
        face1.edge = edge.next != oppo ? edge.next : edge.prev;

        if (edge.next != oppo) {
            edge.next.prev = oppo.prev;
            oppo.prev.next = edge.next;
            edge.vertex.edge = oppo.prev;
        } else {
            RemoveVertexFromList(edge.vertex);
        }

        if (oppo.next != edge) {
            oppo.next.prev = edge.prev;
            edge.prev.next = oppo.next;
            oppo.vertex.edge = edge.prev;
        } else {
            RemoveVertexFromList(oppo.vertex);
        }

        RemoveHalfedgeFromList(edge);
        RemoveHalfedgeFromList(oppo);
    }

    public virtual Halfedge SplitFace(Vertex v1, Vertex v2) {
        Face oldFace = v1.edges.Where(e => !e.isBoundary).Select(e => e.face).Intersect(v2.edges.Where(e => !e.isBoundary).Select(e => e.face)).Single();
        Face newFace = new Face();
        Halfedge newEdge1 = new Halfedge();
        Halfedge newEdge2 = new Halfedge();

        var edgeList1to2 = new List<Halfedge>();
        var edgeList2to1 = new List<Halfedge>();
        Halfedge current = v1.edges.Where(e => e.face == oldFace).Single();
        while (current.vertex != v2) {
            current = current.next;
            edgeList1to2.Add(current);
        }
        while (current.vertex != v1) {
            current = current.next;
            edgeList2to1.Add(current);
        }

        newEdge1.vertex = v1;
        newEdge2.vertex = v2;
        newEdge1.face = oldFace;
        newEdge2.face = newFace;
        newEdge1.opposite = newEdge2;
        newEdge2.opposite = newEdge1;
        newEdge1.next = edgeList1to2[0];
        newEdge2.next = edgeList2to1[0];
        newEdge1.prev = edgeList1to2[edgeList1to2.Count - 1];
        newEdge2.prev = edgeList2to1[edgeList2to1.Count - 1];
        edgeList1to2[0].prev = newEdge1;
        edgeList2to1[0].prev = newEdge2;
        edgeList1to2[edgeList1to2.Count - 1].next = newEdge1;
        edgeList2to1[edgeList2to1.Count - 1].next = newEdge2;

        oldFace.edge = newEdge1;
        newFace.edge = newEdge2;
        edgeList2to1.ForEach(e => e.face = newFace);

        AddFaceToList(newFace);
        AddHalfedgeToList(newEdge1);
        AddHalfedgeToList(newEdge2);
        return newEdge2;
    }

    public virtual void MergeEdges(Vertex middleVertex) {
        List<Halfedge> edges = middleVertex.edges;
        Asserts.AssertThat(edges.Count == 2, "Vertex must has exactly two halfedges connecting to it.");

        RemoveVertexFromList(middleVertex);
        RemoveHalfedgeFromList(edges[0].opposite);
        RemoveHalfedgeFromList(edges[1].opposite);

        edges[0].vertex = edges[0].next.vertex;
        edges[1].vertex = edges[1].next.vertex;
        edges[0].vertex.edge = edges[0];
        edges[1].vertex.edge = edges[1];

        edges[0].next.next.prev = edges[0];
        edges[1].next.next.prev = edges[1];
        edges[0].next = edges[0].next.next;
        edges[1].next = edges[1].next.next;
        edges[0].opposite = edges[1];
        edges[1].opposite = edges[0];

        if (!edges[0].isBoundary) {
            edges[0].face.edge = edges[0];
        }
        if (!edges[1].isBoundary) {
            edges[1].face.edge = edges[1];
        }
    }

    public virtual Vertex SplitEdge(Halfedge edge) {
        Halfedge oldEdge = edge;
        Halfedge oldOppo = edge.opposite;
        Halfedge newEdge = new Halfedge();
        Halfedge newOppo = new Halfedge();
        Vertex newVertex = CreateVertex(edge.midPoint);

        newEdge.vertex = oldEdge.vertex;
        newOppo.vertex = oldOppo.vertex;
        oldEdge.vertex = newVertex;
        oldOppo.vertex = newVertex;

        newVertex.edge = oldEdge;
        newEdge.vertex.edge = newEdge;
        newOppo.vertex.edge = newOppo;

        newEdge.face = oldEdge.face;
        newOppo.face = oldOppo.face;

        newEdge.next = oldEdge.next;
        newOppo.next = oldOppo.next;
        newEdge.prev = oldEdge;
        newOppo.prev = oldOppo;
        newEdge.opposite = oldOppo;
        newOppo.opposite = oldEdge;
        oldEdge.next.prev = newEdge;
        oldOppo.next.prev = newOppo;
        oldEdge.next = newEdge;
        oldOppo.next = newOppo;
        oldEdge.opposite = newOppo;
        oldOppo.opposite = newEdge;

        AddHalfedgeToList(newEdge);
        AddHalfedgeToList(newOppo);

        return newVertex;
    }

    public void ApplyOffset(Vector3 offset) {
        ApplyLinearTransform(Matrix4x4.Translate(offset));
    }

    public void ApplyRotation(Quaternion rotation, Vector3 center = default) {
        Matrix4x4 transform = Matrix4x4.Translate(center) * Matrix4x4.Rotate(rotation) * Matrix4x4.Translate(-center);
        ApplyLinearTransform(transform);
    }

    public void ApplyScale(Vector3 scale, Vector3 center = default) {
        Matrix4x4 transform = Matrix4x4.Translate(center) * Matrix4x4.Scale(scale) * Matrix4x4.Translate(-center);
        ApplyLinearTransform(transform);
    }

    public virtual void ApplyLinearTransform(Matrix4x4 transform, bool enablePerspective = false) {
        if (transform.isIdentity) return;
        if (enablePerspective) {
            foreach (Vertex v in vertices) {
                v.p = transform.MultiplyPoint(v.p);
            }
        } else {
            foreach (Vertex v in vertices) {
                v.p = transform.MultiplyPoint3x4(v.p);
            }
        }
    }

    public delegate Vector3 PositionTransform(Vector3 pos);
    public virtual void ApplyPositionTransform(PositionTransform transform) {
        foreach (Vertex v in vertices) {
            v.p = transform(v.p);
        }
    }

    public virtual void ApplySpaceWarp(SpaceWarp warp) {
        foreach (Vertex v in vertices) {
            v.p = warp.Evaluate(v.p);
        }
    }

    /// <summary> Adds the other geometry to this geometry. The other geometry should not be used afterwards. </summary>
    public void CombineGeometry(Geometry other) {
        vertices.AddRange(other.vertices);
        halfedges.AddRange(other.halfedges);
        faces.AddRange(other.faces);
        Reindex();
        other.Clear();
    }

    /// <summary> Clears the geometry. </summary>
    public virtual void Clear() {
        vertices.Clear();
        halfedges.Clear();
        faces.Clear();
    }

    public string DebugInfo() {
        string s = "";
        foreach (Halfedge e in halfedges) {
            s +=  $"{(e.prev != null ? e.prev.index.ToString() : "null")} : ({e.opposite.vertex.index}) --{e.index}[{e.opposite.index}]--> ({e.vertex.index}) : {(e.next != null ? e.next.index.ToString() : "null")}\n";
        }
        foreach (Face f in faces) {
            s += "f" + f.index + " | " + (f.edge != null ? f.edge.index.ToString() : "null") + "\n";
        }
        return s;
    }

    protected virtual void AddVertexToList(Vertex v) {
        v.index = vertices.Count;
        vertices.Add(v);
    }

    protected virtual void AddHalfedgeToList(Halfedge e) {
        e.index = halfedges.Count;
        halfedges.Add(e);
    }

    protected virtual void AddFaceToList(Face f) {
        f.index = faces.Count;
        faces.Add(f);
    }

    protected virtual void RemoveVertexFromList(Vertex v) {
        if (v.index != vertices.Count - 1) {
            vertices[v.index] = vertices[vertices.Count - 1];
            vertices[v.index].index = v.index;
        }
        vertices.RemoveAt(vertices.Count - 1);
        v.index = -1;
    }

    protected virtual void RemoveHalfedgeFromList(Halfedge e) {
        if (e.index != halfedges.Count - 1) {
            halfedges[e.index] = halfedges[halfedges.Count - 1];
            halfedges[e.index].index = e.index;
        }
        halfedges.RemoveAt(halfedges.Count - 1);
        e.index = -1;
    }

    protected virtual void RemoveFaceFromList(Face f) {
        if (f.index != faces.Count - 1) {
            faces[f.index] = faces[faces.Count - 1];
            faces[f.index].index = f.index;
        }
        faces.RemoveAt(faces.Count - 1);
        f.index = -1;
    }

    protected void Reindex() {
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i].index = i;
        }
        for (int i = 0; i < halfedges.Count; i++) {
            halfedges[i].index = i;
        }
        for (int i = 0; i < faces.Count; i++) {
            faces[i].index = i;
        }
    }

    private Halfedge FindNextHalfedgeAroundVertexThat(Halfedge e, Predicate<Halfedge> predicate) {
        Halfedge start = e;
        Halfedge current = e.next.opposite;
        while (current != start) {
            if (predicate(current)) {
                return current;
            }
            current = current.next.opposite;
        }
        return null;
    }
}
