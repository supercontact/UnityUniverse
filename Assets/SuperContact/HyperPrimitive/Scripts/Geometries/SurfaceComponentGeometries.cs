using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SurfaceComponentGeometries {

    public static SurfaceComponentGeometry CreatePlaneGeometry(
            float sizeX, float sizeZ, int segmentX, int segmentZ, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth) {
        SurfaceComponentGeometry geometry = new SurfaceComponentGeometry();
        bool normalRequired = (faceType != RenderGeometry.FaceType.Polygonal && faceType != RenderGeometry.FaceType.Triangular);
        bool tangentRequired = (faceType == RenderGeometry.FaceType.Directinal1 || faceType == RenderGeometry.FaceType.Directinal2);

        Vector3 origin = new Vector3(-sizeX / 2, 0, -sizeZ / 2);
        Vector3 d = new Vector3(sizeX / segmentX, 0, sizeZ / segmentZ);
        for (int x = 0; x <= segmentX; x++) {
            for (int z = 0; z <= segmentZ; z++) {
                geometry.CreateVertex(origin + new Vector3(d.x * x, 0, d.z * z));
            }
        }
        Vertex vertice(int x, int z) => geometry.vertices[x * (segmentZ + 1) + z];

        for (int x = 0; x < segmentX; x++) {
            for (int z = 0; z < segmentZ; z++) {
                Face f = geometry.CreateFace(vertice(x, z), vertice(x, z + 1), vertice(x + 1, z + 1), vertice(x + 1, z));
                f.surfaceGroup = surfaceGroup;
                geometry.SetFaceType(f, faceType);
            }
        }

        if (normalRequired) {
            foreach (Halfedge e in geometry.halfedges) {
                geometry.SetNormal(e, Vector3.up);
            }
        }
        if (tangentRequired) {
            foreach (Halfedge e in geometry.halfedges) {
                geometry.SetTangent(e, -e.vector.normalized);
            }
        }
        geometry.DefineBoundaries(vertice(0, 0), vertice(0, segmentZ), vertice(segmentX, segmentZ), vertice(segmentX, 0));
        return geometry;
    }

    public static SurfaceComponentGeometry CreateTriangleGeometry(
            float sizeBase, float sizeHeight, float offsetTop, int segment, bool divideHeightOnly = false, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth) {
        SurfaceComponentGeometry geometry = new SurfaceComponentGeometry();
        bool normalRequired = (faceType != RenderGeometry.FaceType.Polygonal && faceType != RenderGeometry.FaceType.Triangular);

        Vector3 corner = new Vector3(-sizeBase / 2, 0, 0);
        Vector3 dx = new Vector3(sizeBase / segment, 0, 0);
        Vector3 dz = new Vector3((offsetTop + 0.5f) * sizeBase / segment, 0, sizeHeight / segment);

        if (divideHeightOnly) {
            for (int z = 0; z < segment; z++) {
                geometry.CreateVertex(corner + dz * z);
                geometry.CreateVertex(corner + dz * z + dx * (segment - z));
            }
            geometry.CreateVertex(corner + dz * segment);

            for (int z = 0; z < segment - 1; z++) {
                geometry.CreateFace(geometry.vertices[2 * z], geometry.vertices[2 * z + 2], geometry.vertices[2 * z + 3], geometry.vertices[2 * z + 1]);
            }
            geometry.CreateFace(geometry.vertices[2 * segment - 2], geometry.vertices[2 * segment], geometry.vertices[2 * segment - 1]);
            geometry.DefineBoundaries(geometry.vertices[0], geometry.vertices[2 * segment], geometry.vertices[1]);
        } else {
            // Full triangular subdivision
            for (int x = 0; x <= segment; x++) {
                for (int z = 0; z <= segment - x; z++) {
                    geometry.CreateVertex(corner + dx * x + dz * z);
                }
            }
            Vertex vertice(int x, int z) => geometry.vertices[x * (2 * (segment + 1) - (x - 1)) / 2 + z];

            for (int x = 0; x < segment; x++) {
                for (int z = 0; z < segment - x; z++) {
                    geometry.CreateFace(vertice(x, z), vertice(x, z + 1), vertice(x + 1, z));
                    if (x + z < segment - 1) {
                        geometry.CreateFace(vertice(x + 1, z), vertice(x, z + 1), vertice(x + 1, z + 1));
                    }
                }
            }
            geometry.DefineBoundaries(vertice(0, 0), vertice(0, segment), vertice(segment, 0));
        }

        foreach (Face f in geometry.faces) {
            f.surfaceGroup = surfaceGroup;
            geometry.SetFaceType(f, faceType);
        }
        if (normalRequired) {
            foreach (Halfedge e in geometry.halfedges) {
                geometry.SetNormal(e, Vector3.up);
            }
        }
        return geometry;
    }

    public static SurfaceComponentGeometry CreateStarConvexPolygonGeometry(List<Vector3> vertices, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal) {
        SurfaceComponentGeometry geometry = new SurfaceComponentGeometry();
        bool normalRequired = (faceType != RenderGeometry.FaceType.Polygonal && faceType != RenderGeometry.FaceType.Triangular);

        foreach (Vector3 vertex in vertices) {
            geometry.CreateVertex(vertex);
        }
        for (int i = 1; i < vertices.Count - 1; i++) {
            Face f = geometry.CreateFace(geometry.vertices[0], geometry.vertices[i], geometry.vertices[i + 1]);
            f.surfaceGroup = surfaceGroup;
            geometry.SetFaceType(f, faceType);
        }
        if (normalRequired) {
            Vector3 normal = Vector3.Cross(geometry.vertices[1].p - geometry.vertices[0].p, geometry.vertices[vertices.Count - 1].p - geometry.vertices[0].p).normalized;
            foreach (Halfedge e in geometry.halfedges) {
                geometry.SetNormal(e, normal);
            }
        }
        geometry.DefineBoundaries(geometry.vertices[0]);
        geometry.SplitBoundaries();
        return geometry;
    }

    public static SurfaceComponentGeometry CreateRegularPolygonGeometry(
            float radius, int segmentP, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Polygonal, bool splitBoundary = false) {
        SurfaceComponentGeometry geometry = new SurfaceComponentGeometry();
        bool normalRequired = (faceType != RenderGeometry.FaceType.Polygonal && faceType != RenderGeometry.FaceType.Triangular);

        float dth = 2 * Mathf.PI / segmentP;
        for (int i = 0; i < segmentP; i++) {
            geometry.CreateVertex(new Vector3(radius * Mathf.Cos(dth * i), 0, -radius * Mathf.Sin(dth * i)));
        }

        Face f = geometry.CreateFace(geometry.vertices.ToArray());
        f.surfaceGroup = surfaceGroup;
        geometry.SetFaceType(f, faceType);

        if (normalRequired) {
            foreach (Halfedge e in geometry.halfedges) {
                geometry.SetNormal(e, Vector3.up);
            }
        }
        geometry.DefineBoundaries(geometry.vertices[0]);
        if (splitBoundary) {
            geometry.SplitBoundaries();
        }
        return geometry;
    }

    public static SurfaceComponentGeometry CreateFanCapGeometry(
            float radius, int segmentP, int segmentR, float cutAngle = 0, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth,
            bool splitBoundary = false) {
        return CreateConeCapGeometry(radius, 0, segmentP, segmentR, cutAngle, surfaceGroup, faceType, splitBoundary);
    }

    public static SurfaceComponentGeometry CreateConeCapGeometry(
           float radius, float height, int segmentP, int segmentR, float cutAngle = 0, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth,
           bool splitBoundary = false) {

        SurfaceComponentGeometry geometry = new SurfaceComponentGeometry();
        bool normalRequired = (faceType != RenderGeometry.FaceType.Polygonal && faceType != RenderGeometry.FaceType.Triangular);
        bool tangentRequired = (faceType == RenderGeometry.FaceType.Directinal1 || faceType == RenderGeometry.FaceType.Directinal2);

        int n = cutAngle > 0 ? segmentP + 1 : segmentP;
        float deltaAngle = (2 * Mathf.PI - cutAngle) / segmentP;
        float Angle(float th) => cutAngle + deltaAngle * th;

        for (int rr = 0; rr < segmentR; rr++) {
            for (int th = 0; th < n; th++) {
                float r = radius * (segmentR - rr) / segmentR;
                float h = height * rr / segmentR;
                geometry.CreateVertex(new Vector3(r * Mathf.Cos(Angle(th)), h, -r * Mathf.Sin(Angle(th))));
            }
        }
        Vertex vertice(int rr, int th) => geometry.vertices[rr * n + th];
        Vertex center = geometry.CreateVertex(new Vector3(0, height, 0));

        for (int rr = 0; rr < segmentR - 1; rr++) {
            for (int th = 0; th < segmentP; th++) {
                int th2 = (th + 1) % n;
                Face f = geometry.CreateFace(vertice(rr, th), vertice(rr, th2), vertice(rr + 1, th2), vertice(rr + 1, th));
                if (tangentRequired) {
                    List<Halfedge> edges = f.edges;
                    geometry.SetTangent(edges[1], -edges[1].vector.normalized);
                    geometry.SetTangent(edges[3], -edges[3].vector.normalized);
                    geometry.SetTangent(edges[0], new Vector3(Mathf.Sin(Angle(th2)), 0, Mathf.Cos(Angle(th2))));
                    geometry.SetTangent(edges[2], new Vector3(-Mathf.Sin(Angle(th)), 0, -Mathf.Cos(Angle(th))));
                    if (rr == 0) {
                        geometry.SetTangent(edges[0].opposite, new Vector3(-Mathf.Sin(Angle(th)), 0, -Mathf.Cos(Angle(th))));
                    }
                }
            }
        }
        for (int th = 0; th < segmentP; th++) {
            int th2 = (th + 1) % n;
            Face f = geometry.CreateFace(vertice(segmentR - 1, th), vertice(segmentR - 1, th2), center);
            if (tangentRequired) {
                List<Halfedge> edges = f.edges;
                geometry.SetTangent(edges[1], -edges[1].vector.normalized);
                geometry.SetTangent(edges[2], -edges[2].vector.normalized);
                geometry.SetTangent(edges[0], new Vector3(Mathf.Sin(Angle(th2)), 0, Mathf.Cos(Angle(th2))));
            }
        }
        foreach (Face f in geometry.faces) {
            f.surfaceGroup = surfaceGroup;
            geometry.SetFaceType(f, faceType);
        }

        if (normalRequired) {
            if (height == 0) {
                foreach (Halfedge e in geometry.halfedges) {
                    geometry.SetNormal(e, Vector3.up);
                }
            } else {
                for (int rr = 0; rr < segmentR; rr++) {
                    for (int th = 0; th < n; th++) {
                        geometry.SetNormal(vertice(rr, th), new Vector3(Mathf.Cos(Angle(th)), radius / height, -Mathf.Sin(Angle(th))).normalized, surfaceGroup);
                    }
                }
                foreach (Halfedge e in center.edges) {
                    geometry.SetNormal(e, Vector3.Cross(e.vector, e.next.vector).normalized);
                }
            }
        }

        if (cutAngle == 0) {
            geometry.DefineBoundaries(vertice(0, 0));
        } else {
            geometry.DefineBoundaries(vertice(0, 0), vertice(0, n - 1), center);
        }
        if (splitBoundary) {
            geometry.SplitBoundaries();
        }
        return geometry;
    }

    public static SurfaceComponentGeometry CreateSphereCapGeometry(
            float radius, int segmentP, int segmentH, float angle = Mathf.PI, float cutAngle = 0, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth,
            bool splitBoundary = false) {
        SurfaceComponentGeometry geometry = CreateFanCapGeometry(1, segmentP, segmentH, cutAngle, surfaceGroup, faceType, splitBoundary);

        Vector3 spherify(Vector3 pos, out Matrix4x4 localTransform) {
            float th = pos.magnitude * angle / 2;
            float phi = Mathf.Atan2(pos.z, pos.x);

            Vector3 newPos = radius * new Vector3(Mathf.Sin(th) * Mathf.Cos(phi), Mathf.Cos(th), Mathf.Sin(th) * Mathf.Sin(phi));
            localTransform = Matrix4x4.Rotate(Quaternion.FromToRotation(Vector3.up, newPos));
            return newPos;
        }
        geometry.ApplyPositionTransform(spherify);
        return geometry;
    }

    public static SurfaceComponentGeometry CreateExtrudedPolygonCapGeometry(
            float radius, float height, int segmentP, int segmentH, float topRatio = 1, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth,
            bool splitBoundary = false) {
        if (topRatio == 0) {
            return CreateConeCapGeometry(radius, height, segmentP, segmentH, 0, surfaceGroup, faceType, splitBoundary);
        }
        if (topRatio == 1 && height == 0) {
            return CreateRegularPolygonGeometry(radius, segmentP, surfaceGroup, RenderGeometry.FaceType.Polygonal, true);
        }

        StructureGeometry structure = new StructureGeometry();

        SurfaceComponentGeometry upperCap = CreateRegularPolygonGeometry(radius * topRatio, segmentP, surfaceGroup + 1);
        SurfaceComponentGeometry side = CreateConeSideGeometry(radius, radius * topRatio, height, segmentP, segmentH, 0, false, surfaceGroup, faceType);
        side.ApplyOffset(Vector3.up * (height / 2));

        Vertex cornerUp = structure.CreateVertex(new Vector3(radius * topRatio, height, 0));
        Vertex cornerDown = structure.CreateVertex(new Vector3(radius, 0, 0));
        structure.CreateFace(side, false, cornerUp, cornerUp, cornerDown, cornerDown);
        structure.CreateFace(upperCap, true, cornerUp);

        SurfaceComponentGeometry geometry = new SurfaceComponentGeometry(structure.Build());
        geometry.DefineBoundaries(structure.GetBuiltVertex(cornerDown));
        if (splitBoundary) {
            geometry.SplitBoundaries();
        }
        return geometry;
    }

    public static SurfaceComponentGeometry CreateRingCapGeometry(
            float outerRadius, float innerRadius, int segmentP, int segmentR, float cutAngle = 0, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth) {
        return CreateConeSideGeometry(outerRadius, innerRadius, 0, segmentP, segmentR, cutAngle, false, surfaceGroup, faceType);
    }

    public static SurfaceComponentGeometry CreateCylinderSideGeometry(
            float radius, float height, int segmentP, int segmentH, float cutAngle = 0, bool flipped = false, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth) {
        return CreateConeSideGeometry(radius, radius, height, segmentP, segmentH, cutAngle, flipped, surfaceGroup, faceType);
    }

    public static SurfaceComponentGeometry CreateConeSideGeometry(
            float radiusBottom, float radiusTop, float height, int segmentP, int segmentH, float cutAngle = 0, bool flipped = false, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth) {
        SurfaceComponentGeometry geometry = CreatePlaneGeometry(1, 1, segmentH, segmentP, 1, faceType);

        SpaceWarp warp = new SpaceWarp(
            $" phi=(z+0.5)*(2*PI-{cutAngle})",
            $" r={(radiusBottom + radiusTop) / 2}+x*{radiusBottom - radiusTop}",
            $" X=r*cos(phi)",
            $" Y={(flipped ? height : -height)}*x",
            $" Z=r*sin(phi)");
        geometry.ApplySpaceWarp(warp);
        return geometry;
    }

    public static SurfaceComponentGeometry CreateSphereSideGeometry(
            float radius, float angleTop, float angleBottom, int segmentP, int segmentH, float cutAngle = 0, int surfaceGroup = 0, RenderGeometry.FaceType faceType = RenderGeometry.FaceType.Smooth) {
        SurfaceComponentGeometry geometry = CreatePlaneGeometry(1, 1, segmentH, segmentP, 1, faceType);

        SpaceWarp warp = new SpaceWarp(
            $" th={angleTop}+(x+0.5)*{angleBottom - angleTop}",
            $" phi=(z+0.5)*(2*PI-{cutAngle})",
            $" X={radius}*sin(th)*cos(phi)",
            $" Y={radius}*cos(th)",
            $" Z={radius}*sin(th)*sin(phi)");
        geometry.ApplySpaceWarp(warp);
        return geometry;
    }
}
