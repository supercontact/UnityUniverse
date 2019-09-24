using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileBlockControl : MonoBehaviour {

    private static readonly float BLOCK_CURVATURE = 0.1f;
    private static readonly float BLOCK_SMOOTH_RADIUS = 0.01f;

    public TileControl parent;

    public void Init() {
        Mesh blockMesh = BuildTileBlockGeometry().ToMesh();
        GetComponent<MeshFilter>().sharedMesh = blockMesh;
        GetComponent<MeshCollider>().sharedMesh = blockMesh;
    }

    public void SetPressed(bool isPressed) {
        GetComponent<Renderer>().enabled = !isPressed;
    }

    private RenderGeometry BuildTileBlockGeometry() {
        var geometry = new RenderGeometry();

        parent.tile.face.edges.ForEach(e => geometry.CreateVertex(e.vertex.p - parent.faceCenter));
        parent.tile.face.edges.ForEach(e => geometry.CreateVertex(CalculateTileTopVertexPosition(e)));

        int n = geometry.vertices.Count / 2;
        for (int i = 0; i < n; i++) {
            geometry.CreateFace(geometry.vertices[i], geometry.vertices[(i + 1) % n], geometry.vertices[(i + 1) % n + n], geometry.vertices[i + n]);
        }
        geometry.CreateFace(Enumerable.Range(n, n).Select(i => geometry.vertices[i]).ToArray());

        new FaceMerging(1f).Apply(geometry);
        new FaceCurving(BLOCK_CURVATURE).Apply(geometry);
        new EdgeSmoothing(BLOCK_SMOOTH_RADIUS, 10).Apply(geometry);
        return geometry;
    }

    private Vector3 CalculateTileTopVertexPosition(Halfedge e) {
        float cornerAngle = Vector3.Angle(-e.vector, e.next.vector) * Mathf.Deg2Rad;
        float d = parent.tileTopToEdgeDistange / Mathf.Tan(cornerAngle / 2);
        return e.vertex.p - parent.faceCenter - e.vector.normalized * d + Vector3.Cross(parent.faceNormal, e.vector).normalized * parent.tileTopToEdgeDistange + parent.faceNormal * parent.tileHeight;
    }

    private void OnDestroy() {
        Destroy(GetComponent<MeshFilter>().sharedMesh);
    }
}
