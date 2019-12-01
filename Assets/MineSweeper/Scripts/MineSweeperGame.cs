using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MineSweeper {
    public class MineSweeperGame : MonoBehaviour {

        public static readonly float TILE_SIZE_SQUARE = 0.33f;
        public static readonly float TILE_SIZE_SQUARE_TRIANGLE = 0.38f;
        public static readonly float TILE_SIZE_TRIANGLE = 0.44f;

        public Dictionary<string, MineSweeperLevel> levels;

        public string levelCode = "1-1";
        public int size = 3;
        public float minePercentage = 0.16f;

        private FocusableInput input = new FocusableInput();

        private void Start() {
            SetUpLevel(levelCode, size, minePercentage);
        }

        private int lastNumber = 0;
        private void Update() {
            bool changed = false;
            if (input.GetKeyDown(KeyCode.W)) {
                size++;
                changed = true;
            }
            if (input.GetKeyDown(KeyCode.S)) {
                size--;
                changed = true;
            }
            if (input.GetKeyDown(KeyCode.A)) {
                minePercentage -= 0.02f;
                minePercentage = Mathf.Max(0, minePercentage);
                changed = true;
            }
            if (input.GetKeyDown(KeyCode.D)) {
                minePercentage += 0.02f;
                minePercentage = Mathf.Min(1, minePercentage);
                changed = true;
            }
            for (int i = 1; i <= 9; i++) {
                if (input.GetKeyDown(KeyCode.Alpha0 + i)) {
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
                geometry = PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE_SQUARE * size, new[] { size, size, size });
                break;
            case "1-2":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE_SQUARE * size * 2, new[] { 1, 1, 1 }),
                    f => SpecialSurfaceComponentGeometries.CreateDiamondCenterCrossSplitSquareGeometry(1, 1, size, size));
                break;
            case "1-3":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE_SQUARE * size * 2, new[] { 1, 1, 1 }),
                    f => SpecialSurfaceComponentGeometries.CreateDiamondCenterOctaSplitSquareGeometry(1, 1, size, size));
                break;
            case "1-4":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE_SQUARE * size * 1.5f, new[] { 1, 1, 1 }),
                    f => SpecialSurfaceComponentGeometries.CreatePantagonSquareGeometry(1, 1, size, size, 0.4f));
                break;
            case "1-5":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE_SQUARE * size * 1.2f, new[] { 1, 1, 1 }),
                    f => SpecialSurfaceComponentGeometries.CreateAlternatingDiagonalSplitSquareGeometry(1, 1, size, size));
                break;
            case "1-6":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateCubeGeometry(Vector3.one * TILE_SIZE_SQUARE * size * 1.5f, new[] { 1, 1, 1 }),
                    f => SpecialSurfaceComponentGeometries.CreateXSplitSquareGeometry(1, 1, size, size, 0.2f / size));
                break;
            case "2-1":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateCubeStarGeometry(TILE_SIZE_SQUARE * size, 2, 1),
                    f => SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, size, size));
                break;
            case "3-1":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateCubeFrameGeometry(TILE_SIZE_SQUARE * (size + 2), 1 - ((size + 2) / 3 * 2) / (float)(size + 2)),
                    f => SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, Mathf.RoundToInt(f.edge.prev.length / TILE_SIZE_SQUARE), Mathf.RoundToInt(f.edge.length / TILE_SIZE_SQUARE)));
                break;
            case "4-1":
                geometry = PolyhedronGeometries.CreateTetrahedronGeometry(TILE_SIZE_TRIANGLE * size / Mathf.Sqrt(2), size);
                break;
            case "4-2":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateTetrahedronGeometry(TILE_SIZE_TRIANGLE * size * Mathf.Sqrt(2), 1),
                    f => SpecialSurfaceComponentGeometries.CreateSplitTriangleGeometry(1, 1, 0, size));
                break;
            case "4-3":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateOctahedronStarGeometry(TILE_SIZE_TRIANGLE * size * Mathf.Sqrt(2), 2, 0),
                    f => SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size));
                break;
            case "4-4":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateOctahedronStarGeometry(TILE_SIZE_TRIANGLE * size * 2 * Mathf.Sqrt(2), 2, 0),
                    f => SpecialSurfaceComponentGeometries.CreateSplitTriangleGeometry(1, 1, 0, size));
                break;
            case "5-1":
                geometry = PolyhedronGeometries.CreateOctahedronGeometry(TILE_SIZE_TRIANGLE * size * Mathf.Sqrt(2), size);
                break;
            case "5-2":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateCuboctahedronGeometry(TILE_SIZE_SQUARE_TRIANGLE * size * Mathf.Sqrt(2), 0.5f),
                    f => f.edges.Count == 4
                        ? SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, size, size)
                        : SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size));
                break;
            case "5-3":
                geometry = ReplaceAllFace(
                    PolyhedronGeometries.CreateRhombicuboctahedronGeometry(TILE_SIZE_SQUARE_TRIANGLE * size * (1 + Mathf.Sqrt(2)), Mathf.Sqrt(2) / (1 + Mathf.Sqrt(2))),
                    f => f.edges.Count == 4
                        ? SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, size, size)
                        : SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size));
                break;
            case "6-1":
                geometry = PolyhedronGeometries.CreateIcosahedronGeometry(TILE_SIZE_TRIANGLE * size * goldenNumber, size);
                break;
            case "6-2":
                geometry = ReplaceAllFace(
                     PolyhedronGeometries.CreateDodecahedronGeometry(TILE_SIZE_TRIANGLE * size * goldenNumber * goldenNumber),
                     f => SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(1, 5, () => SpecialSurfaceComponentGeometries.CreateWallTiledTriangleGeometry(1, 1, 0, size).ShiftBoundaries(1)));
                break;
            case "6-3":
                geometry = ReplaceAllFace(
                     PolyhedronGeometries.CreateIcosidodecahedronGeometry(TILE_SIZE_TRIANGLE * size * goldenNumber * 2),
                     f => f.edges.Count == 5
                         ? SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(1, 5, () => SpecialSurfaceComponentGeometries.CreateWallTiledTriangleGeometry(1, 1, 0, size).ShiftBoundaries(1))
                         : SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size));
                break;
            case "6-4":
                geometry = ReplaceAllFace(
                     PolyhedronGeometries.CreateTrunctedIcosahedronGeometry(TILE_SIZE_TRIANGLE * size * goldenNumber * goldenNumber * 2, 2 / 3f),
                     f => f.edges.Count == 5
                         ? SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(1, 5, () => SpecialSurfaceComponentGeometries.CreateWallTiledTriangleGeometry(1, 1, 0, size).ShiftBoundaries(1))
                         : SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(1, 6, () => SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, size)));
                break;
            case "7-1":
                geometry = CircularGeometries.CreateTorusGeometry(TILE_SIZE_SQUARE * (3 + size) * 0.4f, TILE_SIZE_SQUARE * (3 + size) * 0.16f, (3 + size) * 2, 3 + size, false, false);
                break;
            case "9-9":
                geometry = SpecialSurfaceComponentGeometries.CreateTrianglesCombinedRegularPolygonGeometry(TILE_SIZE_SQUARE * size, 5, () => SpecialSurfaceComponentGeometries.CreateWallTiledTriangleGeometry(1, 1, 0, size).ShiftBoundaries(1));
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
    }

    public class MineSweeperLevel {
        public RenderGeometry geometry;
        public int numberOfMines;

        public MineSweeperLevel(RenderGeometry geometry, int numberOfMines) {
            this.geometry = geometry;
            this.numberOfMines = numberOfMines;
        }
    }
}