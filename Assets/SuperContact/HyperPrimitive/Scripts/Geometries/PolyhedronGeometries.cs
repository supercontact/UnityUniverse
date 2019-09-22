using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PolyhedronGeometries {

    public static RenderGeometry CreateCubeGeometry(Vector3 size, int[] segments) {
        return CreateCubeGeometryInternal(size, dim => SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, segments[(dim + 2) % 3], segments[(dim + 1) % 3], dim));
    }

    public static RenderGeometry CreateCubeStarGeometry(float size, float extrusion = 1, float cutTop = 0) {
        float extrusionScale = Mathf.Sqrt(2f) / 2;
        return CreateCubeGeometryInternal(
            size * Vector3.one, dim => SurfaceComponentGeometries.CreateExtrudedPolygonCapGeometry(1, extrusion * extrusionScale, 4, 1, cutTop, dim, RenderGeometry.FaceType.Polygonal, true));
    }

    private static RenderGeometry CreateCubeGeometryInternal(Vector3 size, Func<int, SurfaceComponentGeometry> surfaceGenerator) {
        StructureGeometry structure = new StructureGeometry();

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in VertexSymmetryGroup("111", Key(1, 1, 1), Vector3.one)) {
            corners[keyAndPosition.Key] = structure.CreateVertex(keyAndPosition.Value.Times(size / 2));
        }
        foreach (Symmetry symmetry in Symmetry.SymmetryGroup("100")) {
            Vertex[] verts = symmetry.Apply(Key(1, -1, -1), Key(1, 1, -1), Key(1, 1, 1), Key(1, -1, 1)).Select(key => corners[key]).ToArray();
            structure.CreateFace(surfaceGenerator.Invoke(symmetry.rotation), true, verts);
        }
        return structure.Build();
    }

    public static RenderGeometry CreateCubeFrameGeometry(float size, float ratio) {
        RenderGeometry geometry = new RenderGeometry();

        var corners = new Dictionary<IntVector3, Vertex>();
        var lengths = new Dictionary<int, float> { [-2] = -size / 2, [-1] = -size / 2 * ratio, [1] = size / 2 * ratio , [2] = size / 2 };
        foreach (int x in lengths.Keys) {
            foreach (int y in lengths.Keys) {
                foreach (int z in lengths.Keys) {
                    corners[Key(x, y, z)] = geometry.CreateVertex(new Vector3(lengths[x], lengths[y], lengths[z]));
                }
            }
        }

        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("211", Key(2, 1, 1), Key(2, 2, 1), Key(2, 2, 2), Key(2, 1, 2)),
                FaceSymmetryGroup("210", Key(2, 1, -1), Key(2, 2, -1), Key(2, 2, 1), Key(2, 1, 1)),
                FaceSymmetryGroup("210", Key(1, 1, -1), Key(2, 1, -1), Key(2, 1, 1), Key(1, 1, 1)))) {
            geometry.CreateFace(keys.Select(i => corners[i]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateBuildingBlockGeometry(Vector3 size, bool[,,] corners) {
        RenderGeometry geometry = new RenderGeometry();

        bool cornerUsed(IntVector3 key) => corners[(key.x + 1) / 2, (key.y + 1) / 2, (key.z + 1) / 2];
        var cornerVerts = new Dictionary<IntVector3, Vertex>();
        foreach (IntVector3 key in IntVector3.allTriaxialDirections) {
            if (cornerUsed(key)) {
                cornerVerts[key] = geometry.CreateVertex(key * (size / 2));
            }
        }

        void construct(string symmetryType, IntVector3[] faceKeys, IntVector3[] outsideKeys) {
            int n = faceKeys.Length;
            foreach (Symmetry symmetry in Symmetry.SymmetryGroup(symmetryType)) {
                if (symmetry.Apply(outsideKeys).Any(key => cornerUsed(key))) continue;

                IntVector3[] newFaceKeys = symmetry.Apply(faceKeys);
                if (newFaceKeys.All(key => cornerUsed(key))) {
                    geometry.CreateFace(newFaceKeys.Select(key => cornerVerts[key]).ToArray());
                } else if (n == 4) {
                    for (int i = 0; i < 4; i++) {
                        IntVector3[] subFaceKeys = new[] { newFaceKeys[i], newFaceKeys[(i + 1) % 4], newFaceKeys[(i + 2) % 4] };
                        if (subFaceKeys.All(key => cornerUsed(key))) {
                            geometry.CreateFace(subFaceKeys.Select(key => cornerVerts[key]).ToArray());
                        }
                    }
                }
            }
        }
        construct("100",
            new[] { Key(1, -1, -1), Key(1, 1, -1), Key(1, 1, 1), Key(1, -1, 1) },
            new IntVector3[0]);
        construct("100",
            new[] { Key(-1, -1, -1), Key(-1, 1, -1), Key(-1, 1, 1), Key(-1, -1, 1) },
            new[] { Key(1, -1, -1), Key(1, 1, -1), Key(1, 1, 1), Key(1, -1, 1) });
        construct("110",
            new[] { Key(-1, 1, -1), Key(-1, 1, 1), Key(1, -1, 1), Key(1, -1, -1) },
            new[] { Key(1, 1, -1), Key(1, 1, 1) });
        construct("111",
            new[] { Key(-1, 1, 1), Key(1, -1, 1), Key(1, 1, -1) },
            new[] { Key(1, 1, 1) });
        construct("111",
            new[] { Key(1, -1, -1), Key(-1, 1, -1), Key(-1, -1, 1) },
            new[] { Key(-1, 1, 1), Key(1, -1, 1), Key(1, 1, -1), Key(1, 1, 1) });

        return geometry;
    }

    public static RenderGeometry CreateTetrahedronGeometry(float size, int segment) {
        int currentGroup = 0;
        return CreateTetrahedronGeometryInternal(size, () => SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, segment, false, currentGroup++));
    }

    public static RenderGeometry CreateTetrahedronStarGeometry(float size, float extrusion, float cutTop) {
        int currentGroup = 0;
        float extrusionScale = 1 / Mathf.Sqrt(2);
        return CreateTetrahedronGeometryInternal(
            size, () => SurfaceComponentGeometries.CreateExtrudedPolygonCapGeometry(1, extrusion * extrusionScale, 3, 1, cutTop, currentGroup++, RenderGeometry.FaceType.Polygonal, true));
    }

    private static RenderGeometry CreateTetrahedronGeometryInternal(float size, Func<SurfaceComponentGeometry> surfaceGenerator) {
        StructureGeometry structure = new StructureGeometry();

        Vertex v1 = structure.CreateVertex(new Vector3(-1, -1, 1) * (size / 2));
        Vertex v2 = structure.CreateVertex(new Vector3(-1, 1, -1) * (size / 2));
        Vertex v3 = structure.CreateVertex(new Vector3(1, -1, -1) * (size / 2));
        Vertex v4 = structure.CreateVertex(new Vector3(1, 1, 1) * (size / 2));

        structure.CreateFace(surfaceGenerator.Invoke(), true, v1, v2, v3);
        structure.CreateFace(surfaceGenerator.Invoke(), true, v1, v4, v2);
        structure.CreateFace(surfaceGenerator.Invoke(), true, v2, v4, v3);
        structure.CreateFace(surfaceGenerator.Invoke(), true, v3, v4, v1);
        return structure.Build();
    }

    public static RenderGeometry CreateOctahedronGeometry(float size, int segment) {
        int currentGroup = 0;
        return CreateOctahedronGeometryInternal(size, () => SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, segment, false, currentGroup++));
    }

    public static RenderGeometry CreateOctahedronStarGeometry(float size, float extrusion, float cutTop) {
        int currentGroup = 0;
        float extrusionScale = Mathf.Sqrt(2) / 2;
        return CreateOctahedronGeometryInternal(
            size, () => SurfaceComponentGeometries.CreateExtrudedPolygonCapGeometry(1, extrusion * extrusionScale, 3, 1, cutTop, currentGroup++, RenderGeometry.FaceType.Polygonal, true));
    }

    private static RenderGeometry CreateOctahedronGeometryInternal(float size, Func<SurfaceComponentGeometry> surfaceGenerator) {
        StructureGeometry structure = new StructureGeometry();

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in VertexSymmetryGroup("100", Key(1, 0, 0), Vector3.right * (size / 2))) {
            corners[keyAndPosition.Key] = structure.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in FaceSymmetryGroup("111", Key(1, 0, 0), Key(0, 1, 0), Key(0, 0, 1))) {
            structure.CreateFace(surfaceGenerator.Invoke(), true, keys.Select(key => corners[key]).ToArray());
        }
        return structure.Build();
    }

    public static RenderGeometry CreateIcosahedronGeometry(float size, int segment) {
        int currentGroup = 0;
        return CreateIcosahedronGeometryInternal(size, () => SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, segment, false, currentGroup++));
    }

    public static RenderGeometry CreateIcosahedronStarGeometry(float size, float extrusion, float cutTop) {
        int currentGroup = 0;
        float extrusionScale = Mathf.Sqrt(3) * (Mathf.Sqrt(5) + 1) / 4;
        return CreateIcosahedronGeometryInternal(
            size, () => SurfaceComponentGeometries.CreateExtrudedPolygonCapGeometry(1, extrusion * extrusionScale, 3, 1, cutTop, currentGroup++, RenderGeometry.FaceType.Polygonal, true));
    }

    private static RenderGeometry CreateIcosahedronGeometryInternal(float size, Func<SurfaceComponentGeometry> surfaceGenerator) {
        StructureGeometry structure = new StructureGeometry();

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float dL = size / 2;
        float dS = dL / goldenRatio;

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in VertexSymmetryGroup("110", Key(1, 1, 0), Vec(dL, dS, 0))) {
            corners[keyAndPosition.Key] = structure.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("101", Key(1, 1, 0), Key(1, 0, 1), Key(1, -1, 0)),
                FaceSymmetryGroup("111", Key(1, 1, 0), Key(0, 1, 1), Key(1, 0, 1)))) {
            structure.CreateFace(surfaceGenerator.Invoke(), true, keys.Select(key => corners[key]).ToArray());
        }
        return structure.Build();
    }

    public static RenderGeometry CreateDodecahedronGeometry(float size) {
        int currentGroup = 0;
        return CreateDodecahedronGeometryInternal(size, () => SurfaceComponentGeometries.CreateRegularPolygonGeometry(1, 5, currentGroup++, splitBoundary: true));
    }

    public static RenderGeometry CreateDodecahedronStarGeometry(float size, float extrusion, float cutTop) {
        int currentGroup = 0;
        float extrusionScale = Mathf.Sqrt(3) * (Mathf.Sqrt(5) + 1) / 4;
        return CreateDodecahedronGeometryInternal(
            size, () => SurfaceComponentGeometries.CreateExtrudedPolygonCapGeometry(1, extrusion * extrusionScale, 5, 1, cutTop, currentGroup++, RenderGeometry.FaceType.Polygonal, true));
    }

    private static RenderGeometry CreateDodecahedronGeometryInternal(float size, Func<SurfaceComponentGeometry> surfaceGenerator) {
        StructureGeometry structure = new StructureGeometry();

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float dL = size / 2;
        float dM = dL / goldenRatio;
        float dS = dM / goldenRatio;

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in Combine(
                VertexSymmetryGroup("111", Key(1, 1, 1), dM * Vector3.one),
                VertexSymmetryGroup("101", Key(1, 0, 1), Vec(dL, 0, dS)))) {
            corners[keyAndPosition.Key] = structure.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in FaceSymmetryGroup("110", Key(1, 0, -1), Key(1, 1, -1), Key(1, 1, 0), Key(1, 1, 1), Key(1, 0, 1))) {
            structure.CreateFace(surfaceGenerator.Invoke(), true, keys.Select(key => corners[key]).ToArray());
        }
        return structure.Build();
    }

    public static RenderGeometry CreateTrunctedTetrahedronGeometry(float size, float ratio) {
        if (ratio <= 0) {
            return CreateTetrahedronGeometry(size, 1);
        }
        if (ratio >= 1) {
            return CreateOctahedronGeometry(size, 1);
        }

        RenderGeometry geometry = new RenderGeometry();

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in VertexSymmetryGroup("211+", Key(2, 1, 1), Vec(1, 1 - ratio, 1 - ratio) * (size / 2))) {
            corners[keyAndPosition.Key] = geometry.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("111+", Key(2, 1, 1), Key(1, 2, 1), Key(1, 1, 2)),
                FaceSymmetryGroup("111-", Key(1, 2, -1), Key(-1, 2, 1), Key(-1, 1, 2), Key(1, -1, 2), Key(2, -1, 1), Key(2, 1, -1)))) {
            geometry.CreateFace(keys.Select(key => corners[key]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateTrunctedCubeGeometry(float size, float ratio) {
        if (ratio <= 0) {
            return CreateCubeGeometry(Vector3.one * size, new int[] { 1, 1, 1 });
        }
        if (ratio >= 1) {
            return CreateCuboctahedronGeometry(size, 0.5f);
        }

        RenderGeometry geometry = new RenderGeometry();

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in VertexSymmetryGroup("221", Key(2, 2, 1), Vec(1, 1, 1 - ratio) * (size / 2))) {
            corners[keyAndPosition.Key] = geometry.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("100", Key(2, -2, -1), Key(2, -1, -2), Key(2, 1, -2), Key(2, 2, -1), Key(2, 2, 1), Key(2, 1, 2), Key(2, -1, 2), Key(2, -2, 1)),
                FaceSymmetryGroup("111", Key(2, 2, 1), Key(1, 2, 2), Key(2, 1, 2)))) {
            geometry.CreateFace(keys.Select(key => corners[key]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateTrunctedOctahedronGeometry(float size, float ratio) {
        if (ratio <= 0) {
            return CreateOctahedronGeometry(size, 1);
        }
        if (ratio >= 1) {
            return CreateCuboctahedronGeometry(size / 2, 0.5f);
        }

        RenderGeometry geometry = new RenderGeometry();

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in VertexSymmetryGroup("210", Key(2, 1, 0), Vec(1, ratio, 0) * (size / 2))) {
            corners[keyAndPosition.Key] = geometry.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("100", Key(2, 0, -1), Key(2, 1, 0), Key(2, 0, 1), Key(2, -1, 0)),
                FaceSymmetryGroup("111", Key(2, 1, 0), Key(1, 2, 0), Key(0, 2, 1), Key(0, 1, 2), Key(1, 0, 2), Key(2, 0, 1)))) {
            geometry.CreateFace(keys.Select(key => corners[key]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateCuboctahedronGeometry(float size, float ratio) {
        if (ratio <= 0) {
            return CreateTetrahedronGeometry(size, 1);
        }
        if (ratio >= 1) {
            RenderGeometry result = CreateTetrahedronGeometry(size, 1);
            result.ApplyRotation(Quaternion.AngleAxis(90, Vector3.up));
            return result;
        }

        RenderGeometry geometry = new RenderGeometry();

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var symmetry in Symmetry.SymmetryGroup("110")) {
            IntVector3 key = symmetry.Apply(Key(1, 1, 0));
            Vector3 position = symmetry.Apply(Vec(1, 1, (1 - ratio * 2) * (symmetry.isNegative ? -1 : 1)) * (size / 2));
            corners[key] = geometry.CreateVertex(position);
        }
        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("100", Key(1, 0, -1), Key(1, 1, 0), Key(1, 0, 1), Key(1, -1, 0)),
                FaceSymmetryGroup("111", Key(1, 1, 0), Key(0, 1, 1), Key(1, 0, 1)))) {
            geometry.CreateFace(keys.Select(key => corners[key]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateRhombicuboctahedronGeometry(float size, float ratio) {
        if (ratio <= 0) {
            return CreateCubeGeometry(Vector3.one * size, new int[] { 1, 1, 1 });
        }
        if (ratio >= 1) {
            return CreateOctahedronGeometry(size, 1);
        }

        RenderGeometry geometry = new RenderGeometry();

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in VertexSymmetryGroup("211", Key(2, 1, 1), Vec(1, 1 - ratio, 1 - ratio) * (size / 2))) {
            corners[keyAndPosition.Key] = geometry.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("100", Key(2, -1, -1), Key(2, 1, -1), Key(2, 1, 1), Key(2, -1, 1)),
                FaceSymmetryGroup("110", Key(2, 1, -1), Key(1, 2, -1), Key(1, 2, 1), Key(2, 1, 1)),
                FaceSymmetryGroup("111", Key(2, 1, 1), Key(1, 2, 1), Key(1, 1, 2)))) {
            geometry.CreateFace(keys.Select(key => corners[key]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateTrunctedIcosahedronGeometry(float size, float ratio) {
        if (ratio <= 0) {
            return CreateIcosahedronGeometry(size, 1);
        }
        if (ratio >= 1) {
            return CreateIcosidodecahedronGeometry(size);
        }

        RenderGeometry geometry = new RenderGeometry();

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float dL = size / 2;
        float dS = dL / goldenRatio;
        float dLtoS = Mathf.Lerp(dL, dS, ratio / 2);
        float dLto0 = Mathf.Lerp(dL, 0, ratio / 2);
        float dStoL = Mathf.Lerp(dS, dL, ratio / 2);
        float dSto0 = Mathf.Lerp(dS, 0, ratio / 2);
        float dStoNegS = Mathf.Lerp(dS, -dS, ratio / 2);
        float d0toL = dL * ratio / 2;
        float d0toS = dS * ratio / 2;

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in Combine(
                VertexSymmetryGroup("110", Key(3, 1, 0), Vec(dL, dStoNegS, 0)),
                VertexSymmetryGroup("211", Key(3, 1, 1), Vec(dLtoS, dSto0, d0toL)),
                VertexSymmetryGroup("211", Key(3, 2, 1), Vec(dLto0, dStoL, d0toS)))) {
            corners[keyAndPosition.Key] = geometry.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("101", Key(3, 1, 0), Key(3, 1, 1), Key(2, 1, 3), Key(2, -1, 3), Key(3, -1, 1), Key(3, -1, 0)),
                FaceSymmetryGroup("111", Key(3, 1, 1), Key(3, 2, 1), Key(1, 3, 1), Key(1, 3, 2), Key(1, 1, 3), Key(2, 1, 3)),
                FaceSymmetryGroup("110", Key(3, 2, 1), Key(3, 1, 1), Key(3, 1, 0), Key(3, 1, -1), Key(3, 2, -1)))) {
            geometry.CreateFace(keys.Select(key => corners[key]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateTrunctedDodecahedronGeometry(float size, float ratio) {
        if (ratio <= 0) {
            return CreateDodecahedronGeometry(size);
        }
        if (ratio >= 1) {
            return CreateIcosidodecahedronGeometry(size);
        }

        RenderGeometry geometry = new RenderGeometry();

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float dL = size / 2;
        float dM = dL / goldenRatio;
        float dS = dM / goldenRatio;
        float dLtoM = Mathf.Lerp(dL, dM, ratio / 2);
        float dMtoL = Mathf.Lerp(dM, dL, ratio / 2);
        float dMtoS = Mathf.Lerp(dM, dS, ratio / 2);
        float dMto0 = Mathf.Lerp(dM, 0, ratio / 2);
        float dStoM = Mathf.Lerp(dS, dM, ratio / 2);
        float dStoNegS = Mathf.Lerp(dS, -dS, ratio / 2);
        float d0toM = dM * (ratio / 2);

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in Combine(
                VertexSymmetryGroup("211", Key(2, 1, 1), Vec(dMtoL, dMto0, dMtoS)),
                VertexSymmetryGroup("101", Key(3, 0, 1), Vec(dL, 0, dStoNegS)),
                VertexSymmetryGroup("211", Key(3, 1, 1), Vec(dLtoM, d0toM, dStoM)))) {
            corners[keyAndPosition.Key] = geometry.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in Combine(
               FaceSymmetryGroup("111", Key(2, 1, 1), Key(1, 2, 1), Key(1, 1, 2)),
               FaceSymmetryGroup("101", Key(3, 0, 1), Key(3, 1, 1), Key(3, -1, 1)),
               FaceSymmetryGroup("110", Key(3, 0, -1), Key(3, 1, -1), Key(2, 1, -1), Key(1, 2, -1), Key(1, 3, -1), Key(1, 3, 1), Key(1, 2, 1), Key(2, 1, 1), Key(3, 1, 1), Key(3, 0, 1)))) {
            geometry.CreateFace(keys.Select(key => corners[key]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateIcosidodecahedronGeometry(float size) {
        RenderGeometry geometry = new RenderGeometry();

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float dL = size / 2;
        float dM = dL / 2;
        float dS = dM / goldenRatio;
        float dL2 = Mathf.Lerp(dL, dS * 2, 0.5f);

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in Combine(
                VertexSymmetryGroup("100", Key(2, 0, 0), Vec(dL, 0, 0)),
                VertexSymmetryGroup("211", Key(2, 1, 1), Vec(dL2, dS, dM)))) {
            corners[keyAndPosition.Key] = geometry.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("101", Key(2, 0, 0), Key(2, 1, 1), Key(2, -1, 1)),
                FaceSymmetryGroup("111", Key(2, 1, 1), Key(1, 2, 1), Key(1, 1, 2)),
                FaceSymmetryGroup("110", Key(1, 2, 1), Key(2, 1, 1), Key(2, 0, 0), Key(2, 1, -1), Key(1, 2, -1)))) {
            geometry.CreateFace(keys.Select(key => corners[key]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateRhombicosidodecahedronGeometry(float size, float ratio) {
        if (ratio <= 0) {
            return CreateIcosahedronGeometry(size, 1);
        }
        if (ratio >= 1) {
            return CreateDodecahedronGeometry(size);
        }

        RenderGeometry geometry = new RenderGeometry();

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float dL = size / 2;
        float dM = dL / goldenRatio;
        float dS = dM / goldenRatio;
        float dLtoM = Mathf.Lerp(dL, dM, ratio);
        float dLtoS = Mathf.Lerp(dL, dS, ratio);
        float dMtoL = Mathf.Lerp(dM, dL, ratio);
        float dMto0 = Mathf.Lerp(dM, 0, ratio);
        float d0toM = dM * ratio;
        float d0toS = dS * ratio;

        var corners = new Dictionary<IntVector3, Vertex>();
        foreach (var keyAndPosition in Combine(
                VertexSymmetryGroup("110", Key(3, 2, 0), Vec(dLtoS, dMtoL, 0)),
                VertexSymmetryGroup("211", Key(3, 1, 1), Vec(dL, dMto0, d0toS)),
                VertexSymmetryGroup("211", Key(3, 2, 1), Vec(dLtoM, dM, d0toM)))) {
            corners[keyAndPosition.Key] = geometry.CreateVertex(keyAndPosition.Value);
        }
        foreach (IntVector3[] keys in Combine(
                FaceSymmetryGroup("101", Key(3, -1, 1), Key(3, 1, 1), Key(2, 0, 3)),
                FaceSymmetryGroup("111", Key(3, 2, 1), Key(1, 3, 2), Key(2, 1, 3)),
                FaceSymmetryGroup("110", Key(3, 1, -1), Key(3, 2, -1), Key(3, 2, 0), Key(3, 2, 1), Key(3, 1, 1)),
                FaceSymmetryGroup("211", Key(3, 1, 1), Key(3, 2, 1), Key(2, 1, 3), Key(2, 0, 3)),
                FaceSymmetryGroup("100", Key(3, -1, -1), Key(3, 1, -1), Key(3, 1, 1), Key(3, -1, 1)))) {
            geometry.CreateFace(keys.Select(key => corners[key]).ToArray());
        }
        return geometry;
    }

    public static RenderGeometry CreateTetrahedronToTetrahedronTransitionGeometry(float size, float ratio, bool cutEdge) {
        if (cutEdge) {
            return CreateCuboctahedronGeometry(size, ratio);
        }
        if (ratio <= 0.5f) {
            return CreateTrunctedTetrahedronGeometry(size, ratio * 2);
        } else {
            RenderGeometry geometry = CreateTrunctedTetrahedronGeometry(size, (1 - ratio) * 2);
            geometry.ApplyRotation(Quaternion.AngleAxis(90, Vector3.up));
            return geometry;
        }
    }

    public static RenderGeometry CreateCubeToOctahedronTransitionGeometry(float size, float ratio, bool cutEdge) {
        if (cutEdge) {
            return CreateRhombicuboctahedronGeometry(size, ratio);
        }
        if (ratio <= 0.5f) {
            return CreateTrunctedCubeGeometry(size, ratio * 2);
        } else {
            return CreateTrunctedOctahedronGeometry(size, (1 - ratio) * 2);
        }
    }

    public static RenderGeometry CreateIcosahedronToDodecahedronTransitionGeometry(float size, float ratio, bool cutEdge) {
        if (cutEdge) {
            return CreateRhombicosidodecahedronGeometry(size, ratio);
        }
        if (ratio <= 0.5f) {
            return CreateTrunctedIcosahedronGeometry(size, ratio * 2);
        } else {
            return CreateTrunctedDodecahedronGeometry(size, (1 - ratio) * 2);
        }
    }

    private static IntVector3 Key(int x, int y, int z, int r = 0) {
        return new IntVector3(x, y, z) >> r;
    }

    private static Vector3 Vec(float x, float y, float z, int r = 0) {
        return new Vector3(x, y, z).Shift(r);
    }

    private static IEnumerable<KeyValuePair<IntVector3, Vector3>> VertexSymmetryGroup(string symmetryType, IntVector3 key, Vector3 position) {
        foreach (Symmetry symmetry in Symmetry.SymmetryGroup(symmetryType)) {
            yield return new KeyValuePair<IntVector3, Vector3>(symmetry.Apply(key), symmetry.Apply(position));
        }
    }

    private static IEnumerable<IntVector3[]> FaceSymmetryGroup(string symmetryType, params IntVector3[] vertexKeys) {
        foreach (Symmetry symmetry in Symmetry.SymmetryGroup(symmetryType)) {
            yield return symmetry.Apply(vertexKeys);
        }
    }

    private static IEnumerable<T> Combine<T>(params IEnumerable<T>[] enumerables) {
        return enumerables.Aggregate((e1, e2) => e1.Concat(e2));
    }
}
