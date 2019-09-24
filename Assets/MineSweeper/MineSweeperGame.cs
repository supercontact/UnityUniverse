using System;
using System.Collections.Generic;
using UnityEngine;

public class MineSweeperGame : MonoBehaviour {
    public static readonly float TILE_SIZE = 0.33f;
    public static readonly float TILE_SIZE_2 = 0.44f;

    public Dictionary<string, MineSweeperLevel> levels;

    public string levelCode = "1-1";
    public int size = 3;
    public float minePercentage = 0.16f;

    private void Start() {
        //SetUpLevels();
        //MineFieldControl.instance.InitField(levels[toPlay].geometry, levels[toPlay].numberOfMines);

        SetUpLevel(levelCode, size, minePercentage);
    }

    private int lastNumber = 0;
    private void Update() {
        bool changed = false;
        if (Input.GetKeyDown(KeyCode.W)) {
            size++;
            changed = true;
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            size--;
            changed = true;
        }
        if (Input.GetKeyDown(KeyCode.A)) {
            minePercentage -= 0.02f;
            minePercentage = Mathf.Max(0, minePercentage);
            changed = true;
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            minePercentage += 0.02f;
            minePercentage = Mathf.Min(1, minePercentage);
            changed = true;
        }
        for (int i = 1; i <= 9; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
                if (lastNumber == 0) {
                    lastNumber = i;
                } else {
                    levelCode = $"{lastNumber}-{i}";
                    lastNumber = 0;
                    changed = true;
                }
            }
        }

        if (changed) {
            SetUpLevel(levelCode, size, minePercentage);
        }
    }

    public void SetUpLevel(string levelCode, int size, float minePercentage) {
        RenderGeometry geometry;
        var goldenNumber = (Mathf.Sqrt(5) + 1) / 2;

        switch (levelCode) {
        case "1-1":
            geometry = PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * size, new[] { size, size, size });
            break;
        case "1-2":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * size * 2, new[] { 1, 1, 1 }),
                f => SpecialSurfaceComponentGeometries.CreateDiamondCenterCrossSplitSquareGeometry(1, 1, size, size));
            break;
        case "1-3":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * size * 2, new[] { 1, 1, 1 }),
                f => SpecialSurfaceComponentGeometries.CreateDiamondCenterOctaSplitSquareGeometry(1, 1, size, size));
            break;
        case "1-4":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * size * 1.5f, new[] { 1, 1, 1 }),
                f => SpecialSurfaceComponentGeometries.CreatePantagonSquareGeometry(1, 1, size, size, 0.4f));
            break;
        case "1-5":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * size * 1.2f, new[] { 1, 1, 1 }),
                f => SpecialSurfaceComponentGeometries.CreateAlternatingDiagonalSplitSquareGeometry(1, 1, size, size));
            break;
        case "1-6":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * size * 1.5f, new[] { 1, 1, 1 }),
                f => SpecialSurfaceComponentGeometries.CreateXSplitSquareGeometry(1, 1, size, size, 0.2f / size));
            break;
        case "2-1":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateCubeStarGeometry(TILE_SIZE * size, 2, 1),
                f => SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, size, size));
            break;
        case "3-1":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateCubeFrameGeometry(TILE_SIZE * (size + 2), 1 - ((size + 2) / 3 * 2) / (float)(size + 2)),
                f => SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, Mathf.RoundToInt(f.edge.prev.length / TILE_SIZE), Mathf.RoundToInt(f.edge.length / TILE_SIZE)));
            break;
        case "4-1":
            geometry = PolyhedronGeometries.CreateTetrahedronGeometry(TILE_SIZE_2 * size / Mathf.Sqrt(2), size);
            break;
        case "4-2":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateTetrahedronGeometry(TILE_SIZE_2 * size * Mathf.Sqrt(2), 1),
                f => SpecialSurfaceComponentGeometries.CreateSplitTriangleGeometry(1, 1, 0, size));
            break;
        case "4-3":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateOctahedronStarGeometry(TILE_SIZE_2 * size * Mathf.Sqrt(2), 2, 0),
                f => SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size));
            break;
        case "4-4":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateOctahedronStarGeometry(TILE_SIZE_2 * size * 2 * Mathf.Sqrt(2), 2, 0),
                f => SpecialSurfaceComponentGeometries.CreateSplitTriangleGeometry(1, 1, 0, size));
            break;
        case "5-1":
            geometry = PolyhedronGeometries.CreateOctahedronGeometry(TILE_SIZE_2 * size * Mathf.Sqrt(2), size);
            break;
        case "5-2":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateCuboctahedronGeometry(TILE_SIZE * size * Mathf.Sqrt(2), 0.5f),
                f => f.edges.Count == 4
                    ? SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, size, size)
                    : SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size));
            break;
        case "5-3":
            geometry = ReplaceAllFace(
                PolyhedronGeometries.CreateRhombicuboctahedronGeometry(TILE_SIZE * size * (1 + Mathf.Sqrt(2)), Mathf.Sqrt(2) / (1 + Mathf.Sqrt(2))),
                f => f.edges.Count == 4
                    ? SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, size, size)
                    : SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size));
            break;
        case "6-1":
            geometry = PolyhedronGeometries.CreateIcosahedronGeometry(TILE_SIZE_2 * size * goldenNumber, size);
            break;
        case "6-2":
            geometry = ReplaceAllFace(
                 PolyhedronGeometries.CreateDodecahedronGeometry(TILE_SIZE * size * goldenNumber * goldenNumber),
                 f => SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(1, 5, () => SpecialSurfaceComponentGeometries.CreateWallTiledTriangleGeometry(1, 1, 0, size).ShiftBoundaries(1)));
            break;
        case "6-3":
            geometry = ReplaceAllFace(
                 PolyhedronGeometries.CreateIcosidodecahedronGeometry(TILE_SIZE * size * goldenNumber * 2),
                 f => f.edges.Count == 5
                     ? SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(1, 5, () => SpecialSurfaceComponentGeometries.CreateWallTiledTriangleGeometry(1, 1, 0, size).ShiftBoundaries(1))
                     : SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size));
            break;
        case "6-4":
            geometry = ReplaceAllFace(
                 PolyhedronGeometries.CreateTrunctedIcosahedronGeometry(TILE_SIZE * size * goldenNumber * goldenNumber * 2, 2 / 3f),
                 f => f.edges.Count == 5
                     ? SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(1, 5, () => SpecialSurfaceComponentGeometries.CreateWallTiledTriangleGeometry(1, 1, 0, size).ShiftBoundaries(1))
                     : SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(1, 6, () => SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size)));
            break;
        case "7-1":
            geometry = CircularGeometries.CreateTorusGeometry(TILE_SIZE * (3 + size) * 0.4f, TILE_SIZE * (3 + size) * 0.16f, (3 + size) * 2, 3 + size, false, false);
            break;
        case "9-9":
            geometry = SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(TILE_SIZE * size, 5, () => SpecialSurfaceComponentGeometries.CreateWallTiledTriangleGeometry(1, 1, 0, size).ShiftBoundaries(1));
            break;
        default:
            return;
        }

        MineFieldControl.instance.InitField(geometry, Mathf.RoundToInt(geometry.faces.Count * minePercentage));
    }

    private RenderGeometry ReplaceAllFace(RenderGeometry original, Func<Face, SurfaceComponentGeometry> surfaceProvider) {
        var structure = new StructureGeometry(original);
        structure.faces.ForEach(f => structure.SetFaceComponent(f, surfaceProvider(f), true));
        return structure.Build();
    }

    //Dictionary<string, MineSweeperLevel> SetUpLevels() {
    //    levels = new Dictionary<string, MineSweeperLevel>();

    //    levels["1-1"] = new MineSweeperLevel(PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * 4, new[] { 4, 4, 4 }), 12);
    //    levels["1-2"] = new MineSweeperLevel(PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * 5, new[] { 5, 5, 5 }), 25);
    //    levels["1-3"] = new MineSweeperLevel(PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * 6, new[] { 6, 6, 6 }), 40);

    //    StructureGeometry cube;
    //    cube = new StructureGeometry(PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * 6, new[] { 1, 1, 1 }));
    //    cube.faces.ForEach(f => cube.SetFaceComponent(f, SpecialSurfaceComponentGeometries.CreateDiamondCenterCrossSplitSquareGeometry(1, 1, 3, 3), true));
    //    levels["1-4"] = new MineSweeperLevel(cube.Build(), 54);

    //    cube = new StructureGeometry(PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * 6, new[] { 1, 1, 1 }));
    //    cube.faces.ForEach(f => cube.SetFaceComponent(f, SpecialSurfaceComponentGeometries.CreateDiamondCenterOctaSplitSquareGeometry(1, 1, 3, 3), true));
    //    levels["1-5"] = new MineSweeperLevel(cube.Build(), 54);

    //    cube = new StructureGeometry(PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * 6, new[] { 1, 1, 1 }));
    //    cube.faces.ForEach(f => cube.SetFaceComponent(f, SpecialSurfaceComponentGeometries.CreatePantagonSquareGeometry(1, 1, 4, 4, 0.4f), true));
    //    levels["1-6"] = new MineSweeperLevel(cube.Build(), 35);

    //    cube = new StructureGeometry(PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * 6, new[] { 1, 1, 1 }));
    //    cube.faces.ForEach(f => cube.SetFaceComponent(f, SpecialSurfaceComponentGeometries.CreateAlternatingDiagonalSplitSquareGeometry(1, 1, 5, 5), true));
    //    levels["1-7"] = new MineSweeperLevel(cube.Build(), 50);

    //    cube = new StructureGeometry(PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE * 6, new[] { 1, 1, 1 }));
    //    cube.faces.ForEach(f => cube.SetFaceComponent(f, SpecialSurfaceComponentGeometries.CreateXSplitSquareGeometry(1, 1, 4, 4, 0.05f), true));
    //    levels["1-8"] = new MineSweeperLevel(cube.Build(), 64);

    //    var plus = new StructureGeometry(PolyhedronGeometries.CreateCubeStarGeometry(TILE_SIZE * 2, 2, 1));
    //    plus.faces.ForEach(f => plus.SetFaceComponent(f, SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 2, 2), true));
    //    levels["2-1"] = new MineSweeperLevel(plus.Build(), 15);

    //    var plus2 = new StructureGeometry(PolyhedronGeometries.CreateCubeStarGeometry(TILE_SIZE * 3, 2, 1));
    //    plus2.faces.ForEach(f => plus2.SetFaceComponent(f, SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 3, 3), true));
    //    levels["2-2"] = new MineSweeperLevel(plus2.Build(), 45);

    //    var plus3 = new StructureGeometry(PolyhedronGeometries.CreateCubeStarGeometry(TILE_SIZE * 4, 2, 1));
    //    plus3.faces.ForEach(f => plus3.SetFaceComponent(f, SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 4, 4), true));
    //    levels["2-3"] = new MineSweeperLevel(plus3.Build(), 88);

    //    var frame = new StructureGeometry(PolyhedronGeometries.CreateCubeFrameGeometry(TILE_SIZE * 7, 3f / 7f));
    //    frame.faces.ForEach(f => frame.SetFaceComponent(f, SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, Mathf.RoundToInt(f.edge.prev.length / TILE_SIZE), Mathf.RoundToInt(f.edge.length / TILE_SIZE)), true));
    //    levels["3-1"] = new MineSweeperLevel(frame.Build(), 80);

    //    //var frame2 = new StructureGeometry(PolyhedronGeometries.CreateCubeFrameGeometry(TILE_SIZE * 7, 3f / 7f));
    //    //frame2.faces.ForEach(f => frame2.SetFaceComponent(f, SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, Mathf.RoundToInt(f.edge.prev.length / TILE_SIZE), Mathf.RoundToInt(f.edge.length / TILE_SIZE)), true));
    //    //frame2.ApplySpaceWarp(new SpaceWarp("r=x*x+y*y+z*z", "X=3/r", "Y=3/r", "Z=3/r"));
    //    //levels["3-2"] = new MineSweeperLevel(frame2.Build(), 80);

    //    levels["4-1"] = new MineSweeperLevel(PolyhedronGeometries.CreateTetrahedronGeometry(TILE_SIZE_2 * 4 / Mathf.Sqrt(2), 4), 8);
    //    levels["4-2"] = new MineSweeperLevel(PolyhedronGeometries.CreateTetrahedronGeometry(TILE_SIZE_2 * 5 / Mathf.Sqrt(2), 5), 15);
    //    levels["4-3"] = new MineSweeperLevel(PolyhedronGeometries.CreateTetrahedronGeometry(TILE_SIZE_2 * 6 / Mathf.Sqrt(2), 6), 24);

    //    var octaStar = new StructureGeometry(PolyhedronGeometries.CreateOctahedronStarGeometry(TILE_SIZE_2 * 3 * Mathf.Sqrt(2), 2, 0));
    //    octaStar.faces.ForEach(f => octaStar.SetFaceComponent(f, SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, 3), true));
    //    levels["4-4"] = new MineSweeperLevel(octaStar.Build(), 36);

    //    var tetrahedron = new StructureGeometry(PolyhedronGeometries.CreateTetrahedronGeometry(TILE_SIZE_2 * 6 / Mathf.Sqrt(2), 1));
    //    tetrahedron.faces.ForEach(f => tetrahedron.SetFaceComponent(f, SpecialSurfaceComponentGeometries.CreateSplitTriangleGeometry(1, 1, 0, 3), true));
    //    levels["4-5"] = new MineSweeperLevel(tetrahedron.Build(), 20);

    //    levels["5-1"] = new MineSweeperLevel(PolyhedronGeometries.CreateOctahedronGeometry(TILE_SIZE_2 * 3 * Mathf.Sqrt(2), 3), 9);
    //    levels["5-2"] = new MineSweeperLevel(PolyhedronGeometries.CreateOctahedronGeometry(TILE_SIZE_2 * 4 * Mathf.Sqrt(2), 4), 18);
    //    levels["5-3"] = new MineSweeperLevel(PolyhedronGeometries.CreateOctahedronGeometry(TILE_SIZE_2 * 5 * Mathf.Sqrt(2), 5), 30);

    //    var goldenNumber = (Mathf.Sqrt(5) + 1) / 2;
    //    levels["6-1"] = new MineSweeperLevel(PolyhedronGeometries.CreateIcosahedronGeometry(TILE_SIZE_2 * 2 * goldenNumber, 2), 10);
    //    levels["6-2"] = new MineSweeperLevel(PolyhedronGeometries.CreateIcosahedronGeometry(TILE_SIZE_2 * 3 * goldenNumber, 3), 25);
    //    levels["6-3"] = new MineSweeperLevel(PolyhedronGeometries.CreateIcosahedronGeometry(TILE_SIZE_2 * 4 * goldenNumber, 4), 50);

    //    var rhombicube = new StructureGeometry(PolyhedronGeometries.CreateRhombicuboctahedronGeometry(TILE_SIZE * 3 * (1 + Mathf.Sqrt(2)), Mathf.Sqrt(2) / (1 + Mathf.Sqrt(2))));
    //    rhombicube.faces.ForEach(f => rhombicube.SetFaceComponent(f, 
    //        f.edges.Count == 4 ?
    //        SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 3, 3) :
    //        SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, 3), true));
    //    levels["7-1"] = new MineSweeperLevel(rhombicube.Build(), 45);

    //    var rhombicube2 = new StructureGeometry(PolyhedronGeometries.CreateRhombicuboctahedronGeometry(TILE_SIZE * 4 * (1 + Mathf.Sqrt(2)), Mathf.Sqrt(2) / (1 + Mathf.Sqrt(2))));
    //    rhombicube2.faces.ForEach(f => rhombicube2.SetFaceComponent(f,
    //        f.edges.Count == 4 ?
    //        SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 4, 4) :
    //        SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, 4), true));
    //    levels["7-2"] = new MineSweeperLevel(rhombicube2.Build(), 90);

    //    levels["9-9"] = new MineSweeperLevel(SpecialSurfaceComponentGeometries.CreateWallTiledTriangleGeometry(1, 0.7f, 0, 4), 1);

    //    return levels;
    //}
}


public class MineSweeperLevel {
    public RenderGeometry geometry;
    public int numberOfMines;

    public MineSweeperLevel(RenderGeometry geometry, int numberOfMines) {
        this.geometry = geometry;
        this.numberOfMines = numberOfMines;
    }
}