using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SpecialSurfaceComponentGeometries {

    public static SurfaceComponentGeometry CreateSingleSplitTriangleGeometry(
            float sizeBase, float sizeHeight, float offsetTop, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        var geometry = new SurfaceComponentGeometry();

        Vertex baseLeft = geometry.CreateVertex(new Vector3(-sizeBase / 2, 0, 0));
        Vertex baseCenter = geometry.CreateVertex(Vector3.zero);
        Vertex baseRight = geometry.CreateVertex(new Vector3(sizeBase / 2, 0, 0));
        Vertex top = geometry.CreateVertex(new Vector3(offsetTop * sizeBase, 0, sizeHeight));
        Vertex sideLeft = geometry.CreateVertex(Vector3.Lerp(baseLeft.p, top.p, 0.5f));
        Vertex sideRight = geometry.CreateVertex(Vector3.Lerp(baseRight.p, top.p, 0.5f));
        Vertex center = geometry.CreateVertex(top.p / 3);

        geometry.CreateFace(baseLeft, sideLeft, center, baseCenter);
        geometry.CreateFace(baseRight, baseCenter, center, sideRight);
        geometry.CreateFace(top, sideRight, center, sideLeft);

        geometry.DefineBoundaries(baseLeft, top, baseRight);
        return geometry;
    }

    public static SurfaceComponentGeometry CreateSplitTriangleGeometry(
            float sizeBase, float sizeHeight, float offsetTop, int segment, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        SurfaceComponentGeometry triangleGeometry = SurfaceComponentGeometries.CreateTriangleGeometry(sizeBase, sizeHeight, offsetTop, segment, false);
        List<Vertex> corners = triangleGeometry.corners;

        var structure = new StructureGeometry(triangleGeometry);
        structure.faces.ForEach(f => structure.SetFaceComponent(f, CreateSingleSplitTriangleGeometry(1, 1, 0, surfaceGroup, faceType), true));
        var geometry = new SurfaceComponentGeometry(structure.Build());
        geometry.DefineBoundaries(corners.Select(structure.GetBuiltVertex).ToArray());

        return geometry;
    }

    public static SurfaceComponentGeometry CreateAlternatingDiagonalSplitSquareGeometry(
            float sizeX, float sizeZ, int segmentX, int segmentZ, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        var geometry = SurfaceComponentGeometries.CreatePlaneGeometry(sizeX, sizeZ, segmentX, segmentZ, surfaceGroup, faceType);

        foreach (Face face in new List<Face>(geometry.faces)) {
            List<Halfedge> edges = face.edges;
            Vector3 facePoint = edges[0].prev.vertex.p;
            int x = Mathf.RoundToInt((facePoint.x - sizeX / 2) / (sizeX / segmentX));
            int z = Mathf.RoundToInt((facePoint.z - sizeZ / 2) / (sizeZ / segmentZ));
            if ((x + z) % 2 == 0) {
                geometry.SplitFace(edges[0].vertex, edges[2].vertex);
            } else {
                geometry.SplitFace(edges[1].vertex, edges[3].vertex);
            }
        }
        return geometry;
    }

    public static SurfaceComponentGeometry CreateXSplitSquareGeometry(
            float sizeX, float sizeZ, int segmentX, int segmentZ, float spikeHeight = 0, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        var planeGeometry = SurfaceComponentGeometries.CreatePlaneGeometry(sizeX, sizeZ, segmentX, segmentZ);
        List<Vertex> corners = planeGeometry.corners;

        var structure = new StructureGeometry(planeGeometry);
        var coneHeight = spikeHeight / (sizeZ / segmentZ) * Mathf.Sqrt(2);
        structure.faces.ForEach(f => {
            var component = SurfaceComponentGeometries.CreateConeCapGeometry(1, coneHeight, 4, 1, 0, surfaceGroup, faceType);
            component.SplitBoundaries();
            structure.SetFaceComponent(f, component, true);
        });
        var geometry = new SurfaceComponentGeometry(structure.Build());
        geometry.DefineBoundaries(corners.Select(structure.GetBuiltVertex).ToArray());

        return geometry;
    }

    public static SurfaceComponentGeometry CreateSingleDiamondCenterCrossSplitSquareGeometry(
            float sizeX, float sizeZ, float diamondRatio = 0.6f, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        var geometry = new SurfaceComponentGeometry();

        Vertex bottomLeft = geometry.CreateVertex(new Vector3(-sizeX / 2, 0, -sizeZ / 2));
        Vertex bottom = geometry.CreateVertex(new Vector3(0, 0, -sizeZ / 2));
        Vertex bottomRight = geometry.CreateVertex(new Vector3(sizeX / 2, 0, -sizeZ / 2));
        Vertex left = geometry.CreateVertex(new Vector3(-sizeX / 2, 0, 0));
        Vertex right = geometry.CreateVertex(new Vector3(sizeX / 2, 0, 0));
        Vertex topLeft = geometry.CreateVertex(new Vector3(-sizeX / 2, 0, sizeZ / 2));
        Vertex top = geometry.CreateVertex(new Vector3(0, 0, sizeZ / 2));
        Vertex topRight = geometry.CreateVertex(new Vector3(sizeX / 2, 0, sizeZ / 2));
        Vertex centerLeft = geometry.CreateVertex(new Vector3(-diamondRatio * sizeX / 2, 0, 0));
        Vertex centerRight = geometry.CreateVertex(new Vector3(diamondRatio * sizeX / 2, 0, 0));
        Vertex centerBottom = geometry.CreateVertex(new Vector3(0, 0, -diamondRatio * sizeZ / 2));
        Vertex centerTop = geometry.CreateVertex(new Vector3(0, 0, diamondRatio * sizeZ / 2));

        geometry.CreateFace(bottomLeft, left, centerLeft, centerBottom, bottom);
        geometry.CreateFace(topLeft, top, centerTop, centerLeft, left);
        geometry.CreateFace(topRight, right, centerRight, centerTop, top);
        geometry.CreateFace(bottomRight, bottom, centerBottom, centerRight, right);
        geometry.CreateFace(centerBottom, centerLeft, centerTop, centerRight);

        geometry.DefineBoundaries(bottomLeft, topLeft, topRight, bottomRight);
        return geometry;
    }

    public static SurfaceComponentGeometry CreateDiamondCenterCrossSplitSquareGeometry(
             float sizeX, float sizeZ, int segmentX, int segmentZ, float diamondRatio = 0.6f, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        SurfaceComponentGeometry planeGeometry = SurfaceComponentGeometries.CreatePlaneGeometry(sizeX, sizeZ, segmentX, segmentZ);
        List<Vertex> corners = planeGeometry.corners;

        var structure = new StructureGeometry(planeGeometry);
        structure.faces.ForEach(f => structure.SetFaceComponent(f, CreateSingleDiamondCenterCrossSplitSquareGeometry(1, 1, diamondRatio, surfaceGroup, faceType), true));
        var geometry = new SurfaceComponentGeometry(structure.Build());
        geometry.DefineBoundaries(corners.Select(structure.GetBuiltVertex).ToArray());

        return geometry;
    }

    public static SurfaceComponentGeometry CreateSingleDiamondCenterOctaSplitSquareGeometry(
           float sizeX, float sizeZ, float diamondRatio = 0.6f, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        var geometry = new SurfaceComponentGeometry();

        var outerRing = new Vertex[] {
            geometry.CreateVertex(new Vector3(0, 0, -sizeZ / 2)),
            geometry.CreateVertex(new Vector3(-sizeX / 2, 0, -sizeZ / 2)),
            geometry.CreateVertex(new Vector3(-sizeX / 2, 0, 0)),
            geometry.CreateVertex(new Vector3(-sizeX / 2, 0, sizeZ / 2)),
            geometry.CreateVertex(new Vector3(0, 0, sizeZ / 2)),
            geometry.CreateVertex(new Vector3(sizeX / 2, 0, sizeZ / 2)),
            geometry.CreateVertex(new Vector3(sizeX / 2, 0, 0)),
            geometry.CreateVertex(new Vector3(sizeX / 2, 0, -sizeZ / 2))
        };
        var innerRing = outerRing.Select((vertex, index) => geometry.CreateVertex(vertex.p * (index % 2 == 0 ? diamondRatio : diamondRatio / 2))).ToArray();

        for (int i = 0; i < 8; i++) {
            geometry.CreateFace(outerRing[i], outerRing[(i + 1) % 8], innerRing[(i + 1) % 8], innerRing[i]);
        }
        geometry.CreateFace(innerRing);

        geometry.DefineBoundaries(outerRing[1], outerRing[3], outerRing[5], outerRing[7]);
        return geometry;
    }

    public static SurfaceComponentGeometry CreateDiamondCenterOctaSplitSquareGeometry(
         float sizeX, float sizeZ, int segmentX, int segmentZ, float diamondRatio = 0.6f, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        SurfaceComponentGeometry planeGeometry = SurfaceComponentGeometries.CreatePlaneGeometry(sizeX, sizeZ, segmentX, segmentZ);
        List<Vertex> corners = planeGeometry.corners;

        var structure = new StructureGeometry(planeGeometry);
        var edgesToMerge = new List<Halfedge>();
        structure.faces.ForEach(f => {
            var component = CreateSingleDiamondCenterOctaSplitSquareGeometry(1, 1, diamondRatio, surfaceGroup, faceType);
            edgesToMerge.AddRange(component.boundaries.SelectMany(b => b).Select(e => e.opposite));
            structure.SetFaceComponent(f, component, true);
        });
        var geometry = new SurfaceComponentGeometry(structure.Build());
        foreach (Halfedge edge in edgesToMerge) {
            if (edge.index >= 0 && !edge.opposite.isBoundary) {
                geometry.MergeFaces(edge);
            }
        }
        geometry.DefineBoundaries(corners.Select(structure.GetBuiltVertex).ToArray());

        return geometry;
    }

    public static SurfaceComponentGeometry CreatePantagonSquareGeometry(
            float sizeX, float sizeZ, int segmentX, int segmentZ, float pantagonRatio = 0.3f, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        SurfaceComponentGeometry planeGeometry = SurfaceComponentGeometries.CreatePlaneGeometry(sizeX, sizeZ, segmentX * 2, segmentZ * 2, surfaceGroup, faceType);
        var originalVertexList = new List<Vertex>(planeGeometry.vertices);
        Vertex corner(int x, int z) => originalVertexList[x * (segmentZ * 2 + 1) + z];

        float deltaX = sizeX / (segmentX * 2) * pantagonRatio;
        float deltaZ = sizeZ / (segmentZ * 2) * pantagonRatio;
        for (int x = 1; x < segmentX * 2; x++) {
            for (int z = 1; z < segmentZ * 2; z++) {
                if (x % 2 == 0 && z % 2 == 1) {
                    corner(x, z).p += deltaX * ((x + z) % 4 == 3 ? Vector3.left : Vector3.right);
                } else if (x % 2 == 1 && z % 2 == 0) {
                    corner(x, z).p += deltaZ * ((x + z) % 4 == 3 ? Vector3.forward : Vector3.back);
                }
            }
        }
        for (int x = 0; x < segmentX; x++) {
            for (int z = 0; z < segmentZ; z++) {
                if ((x + z) % 2 == 0) {
                    planeGeometry.MergeFaces(planeGeometry.FindHalfedge(corner(x * 2 + 1, z * 2 + 1), corner(x * 2 + 1, z * 2)));
                    planeGeometry.MergeFaces(planeGeometry.FindHalfedge(corner(x * 2 + 1, z * 2 + 1), corner(x * 2 + 1, z * 2 + 2)));
                } else {
                    planeGeometry.MergeFaces(planeGeometry.FindHalfedge(corner(x * 2 + 1, z * 2 + 1), corner(x * 2, z * 2 + 1)));
                    planeGeometry.MergeFaces(planeGeometry.FindHalfedge(corner(x * 2 + 1, z * 2 + 1), corner(x * 2 + 2, z * 2 + 1)));
                }
            }
        }
        return planeGeometry;
    }
}
