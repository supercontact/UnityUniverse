using UnityEngine;
using System.Collections.Generic;

public class MeshBuilder
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector3> normals;

    public MeshBuilder() {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
    }
    public MeshBuilder(Mesh mesh) {
        vertices = new List<Vector3>(mesh.vertices);
        triangles = new List<int>(mesh.triangles);
        normals = new List<Vector3>(mesh.normals);
    }

    public void CombineMesh(MeshBuilder other, Vector3 offset = default, Quaternion rotation = default) {
        if (rotation.w == 0) {
            rotation = Quaternion.identity;
        }
        CombineMesh(other.vertices, other.triangles, other.normals, offset, rotation);
    }
    public void CombineMesh(Mesh other, Vector3 offset = default, Quaternion rotation = default) {
        if (rotation.w == 0) {
            rotation = Quaternion.identity;
        }
        CombineMesh(other.vertices, other.triangles, other.normals, offset, rotation);
    }
    private void CombineMesh(IList<Vector3> otherVertices, IList<int> otherTriangles, IList<Vector3> otherNormals, Vector3 offset, Quaternion rotation) {
        int d = vertices.Count;
        for (int i = 0; i < otherVertices.Count; i++) {
            vertices.Add(rotation * otherVertices[i] + offset);
            normals.Add(rotation * otherNormals[i]);
        }
        for (int i = 0; i < otherTriangles.Count; i++) {
            triangles.Add(otherTriangles[i] + d);
        }
    }

    public Mesh ToMesh() {
        Mesh mesh = new Mesh();
        ToMesh(mesh);
        return mesh;
    }
    public void ToMesh(Mesh mesh) {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateBounds();
    }

    public void Invert() {
        for (int i = 0; i < normals.Count; i++) {
            normals[i] = -normals[i];
        }
        for (int i = 0; i < triangles.Count / 3; i++) {
            int temp = triangles[i * 3];
            triangles[i * 3] = triangles[i * 3 + 1];
            triangles[i * 3 + 1] = temp;
        }
    }

    public void AddOtherSide() {
        MeshBuilder clone = new MeshBuilder();
        clone.CombineMesh(this);
        clone.Invert();
        CombineMesh(clone);
    }

    public void SplitAllTriangles() {
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector3> newNormals = new List<Vector3>();
        for (int i = 0; i < triangles.Count; i++) {
            newVertices.Add(vertices[triangles[i]]);
            triangles[i] = i;
        }
        vertices = newVertices;
        for (int i = 0; i < triangles.Count / 3; i++) {
            Vector3 normal = Vector3.Cross(vertices[i * 3 + 1] - vertices[i * 3], vertices[i * 3 + 2] - vertices[i * 3]);
            newNormals.Add(normal);
            newNormals.Add(normal);
            newNormals.Add(normal);
        }
        normals = newNormals;
    }
}