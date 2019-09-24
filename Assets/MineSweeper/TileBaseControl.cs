using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileBaseControl : MonoBehaviour {

    private static readonly float BASE_CURVATURE = -0.1f;

    public TileControl parent;
    public Material normalBaseMaterial;
    public Material explodedBaseMaterial;

    private MeshRenderer meshRenderer;

    private void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Init() {
        Mesh baseMesh = BuildTileBaseGeometry().ToMesh(RenderGeometry.Facing.TwoSided);
        GetComponent<MeshFilter>().sharedMesh = baseMesh;
        Mesh baseColliderMesh = BuildTileBaseColliderGeometry().ToMesh(RenderGeometry.Facing.TwoSided);
        GetComponent<MeshCollider>().sharedMesh = baseColliderMesh;
    }

    public void SetExploded(bool isExploded) {
        meshRenderer.sharedMaterial = isExploded ? explodedBaseMaterial : normalBaseMaterial;
    }

    private RenderGeometry BuildTileBaseGeometry() {
        var geometry = new RenderGeometry();

        parent.tile.face.edges.ForEach(e => geometry.CreateVertex(e.vertex.p - parent.faceCenter));
        geometry.CreateFace(geometry.vertices.ToArray());

        new FaceCurving(BASE_CURVATURE).Apply(geometry);
        return geometry;
    }

    private RenderGeometry BuildTileBaseColliderGeometry() {
        RenderGeometry geometry1 = BuildTileBaseGeometry();
        RenderGeometry geometry2 = BuildTileBaseGeometry();
        geometry2.ApplyScale(0.5f * Vector3.one);
        geometry2.ApplyOffset(-0.01f * parent.faceNormal);
        geometry1.CombineGeometry(geometry2);

        return geometry1;
    }

    private void OnDestroy() {
        Destroy(GetComponent<MeshFilter>().sharedMesh);
        Destroy(GetComponent<MeshCollider>().sharedMesh);
    }
}
