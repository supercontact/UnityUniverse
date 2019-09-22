using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileBlock : MonoBehaviour {

    private static readonly float BASE_CURVATURE = -0.1f;
    private static readonly float BLOCK_CURVATURE = 0.1f;
    private static readonly float BLOCK_SMOOTH_RADIUS = 0.01f;
    private static readonly float MINE_ELEVATION = 0.15f;
    private static readonly float MINE_SCALE = 1.2f;

    public MineFieldModel.Tile tile;

    public GameObject tileBlock;
    public GameObject tileBase;
    public GameObject tileBaseExploded;
    public GameObject number;
    public GameObject flag;
    public GameObject mark;
    public GameObject mine;

    public Vector3 faceCenter;
    public Vector3 faceNormal;
    public float tileHeight;
    public float tileTopToEdgeDistange;

    private bool numberInitialized = false;

    public void InitBlock(MineFieldModel.Tile tile, float tileHeight = 0.1f, float tileSideAngle = 60f) {
        this.tile = tile;
        faceCenter = tile.face.CalculateCenter();
        faceNormal = tile.face.CalculateNormal();
        this.tileHeight = tileHeight;
        tileTopToEdgeDistange = tileHeight * Mathf.Cos(tileSideAngle * Mathf.Deg2Rad);

        transform.position = faceCenter;
        Mesh blockMesh = BuildTileBlockGeometry().ToMesh();
        tileBlock.GetComponent<MeshFilter>().sharedMesh = blockMesh;
        tileBlock.GetComponent<MeshCollider>().sharedMesh = blockMesh;
        Mesh baseMesh = BuildTileBaseGeometry().ToMesh(RenderGeometry.Facing.TwoSided);
        tileBase.GetComponent<MeshFilter>().sharedMesh = baseMesh;
        tileBase.GetComponent<MeshCollider>().sharedMesh = baseMesh;
        tileBaseExploded.GetComponent<MeshFilter>().sharedMesh = baseMesh;
        tileBaseExploded.GetComponent<MeshCollider>().sharedMesh = baseMesh;

        mark.GetComponent<NumberLabel>().InitWithMark();
        flag.GetComponent<FlagControl>().Init();
        InitMine();
        numberInitialized = false;
    }

    private void InitMine() {
        mine.transform.position = faceCenter + faceNormal * MINE_ELEVATION * GetAverageRadius();
        mine.transform.rotation = Quaternion.LookRotation(faceNormal);
        mine.transform.localScale = MINE_SCALE * GetAverageRadius() * Vector3.one;
    }

    public void UpdateState() {
        tileBlock.GetComponent<Renderer>().enabled = !tile.isPressed;
        tileBlock.SetActive(!tile.isOpened);
        tileBase.SetActive(tile.state != MineFieldModel.TileState.Exploded);
        tileBaseExploded.SetActive(tile.state == MineFieldModel.TileState.Exploded);
        bool numberShown = tile.state == MineFieldModel.TileState.Discovered && tile.mineNumber > 0;
        number.SetActive(numberShown);
        if (numberShown && !numberInitialized) {
            number.GetComponent<NumberLabel>().InitWithNumber(tile.mineNumber);
        }
        mark.SetActive(tile.state == MineFieldModel.TileState.Marked);
        flag.SetActive(tile.state == MineFieldModel.TileState.Flagged || tile.state == MineFieldModel.TileState.WronglyFlagged);
        flag.GetComponent<FlagControl>().SetWrongFlag(tile.state == MineFieldModel.TileState.WronglyFlagged);
        mine.SetActive(tile.state == MineFieldModel.TileState.Exploded || tile.state == MineFieldModel.TileState.MineRevealed);
    }

    public void SetCrazy(bool isCrazy) {
        number.GetComponent<NumberLabel>().isCrazy = isCrazy;
    }

    public float GetIncircleRadius() {
        return tile.face.edges.Select(e => Vector3.ProjectOnPlane(faceCenter - e.vertex.p, e.vector).magnitude).Min();
    }

    public float GetAverageRadius() {
        return tile.face.edges.Select(e => (e.vertex.p - faceCenter).magnitude).Average();
    }

    private RenderGeometry BuildTileBlockGeometry() {
        var geometry = new RenderGeometry();
        tile.face.edges.ForEach(e => geometry.CreateVertex(e.vertex.p - faceCenter));
        tile.face.edges.ForEach(e => geometry.CreateVertex(CalculateTileTopVertexPosition(e)));

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

    private RenderGeometry BuildTileBaseGeometry() {
        var geometry = new RenderGeometry();
        tile.face.edges.ForEach(e => geometry.CreateVertex(e.vertex.p - faceCenter));
        geometry.CreateFace(geometry.vertices.ToArray());

        new FaceCurving(BASE_CURVATURE).Apply(geometry);
        return geometry;
    }

    private Vector3 CalculateTileTopVertexPosition(Halfedge e) {
        float cornerAngle = Vector3.Angle(-e.vector, e.next.vector) * Mathf.Deg2Rad;
        float d = tileTopToEdgeDistange / Mathf.Tan(cornerAngle / 2);
        return e.vertex.p - faceCenter - e.vector.normalized * d + Vector3.Cross(faceNormal, e.vector).normalized * tileTopToEdgeDistange + faceNormal * tileHeight;
    }
}
