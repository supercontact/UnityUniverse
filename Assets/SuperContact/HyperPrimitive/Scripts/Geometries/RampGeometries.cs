using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RampGeometries {
    public enum RampType {
        Left,
        Right,
        Valley,
        Peak,
    }

    public static RenderGeometry CreateStraightRampGeometry(
            float width, float height, float length, int segmentWidth, int segmentLength, bool smoothWidth, bool smoothLength, RampType rampType = RampType.Right, float curvature = 0,
            float baseHeight = 0, float highPlatformWidth = 0, float lowPlatformWidth = 0) {
        StructureGeometry structure = new StructureGeometry();
        RenderGeometry.FaceType rampFaceType = GetFaceType(smoothWidth, smoothLength);
        RenderGeometry.FaceType sideFaceType = GetFaceType(false, smoothLength);

        float leftPlatformHeight = (rampType == RampType.Left || rampType == RampType.Peak ? 0 : height) + baseHeight;
        float rightPlatformHeight = (rampType == RampType.Right || rampType == RampType.Peak ? 0 : height) + baseHeight;
        float middlePlatformHeight = (rampType == RampType.Valley ? 0 : height) + baseHeight;

        float leftPlatformWidth = ((rampType == RampType.Left || rampType == RampType.Peak) ? lowPlatformWidth : highPlatformWidth);
        float rightPlatformWidth = ((rampType == RampType.Right || rampType == RampType.Peak) ? lowPlatformWidth : highPlatformWidth);
        float middlePlatformWidth = (rampType == RampType.Valley ? lowPlatformWidth : (rampType == RampType.Peak ? highPlatformWidth : 0));

        float leftX = -width / 2 - middlePlatformWidth / 2;
        float rightX = width / 2 + middlePlatformWidth / 2;

        bool isLeftTopAndBottomSameVertex = (rampType == RampType.Left || rampType == RampType.Peak) && baseHeight == 0;
        bool isRightTopAndBottomSameVertex = (rampType == RampType.Right || rampType == RampType.Peak) && baseHeight == 0;
        Vertex cornerBottomLeft1 = structure.CreateVertex(new Vector3(leftX, 0, -length / 2));
        Vertex cornerBottomLeft2 = structure.CreateVertex(new Vector3(leftX, 0, length / 2));
        Vertex cornerBottomRight1 = structure.CreateVertex(new Vector3(rightX, 0, -length / 2));
        Vertex cornerBottomRight2 = structure.CreateVertex(new Vector3(rightX, 0, length / 2));
        Vertex cornerTopLeft1 = isLeftTopAndBottomSameVertex ? cornerBottomLeft1 : structure.CreateVertex(new Vector3(leftX, leftPlatformHeight, -length / 2));
        Vertex cornerTopLeft2 = isLeftTopAndBottomSameVertex ? cornerBottomLeft2 : structure.CreateVertex(new Vector3(leftX, leftPlatformHeight, length / 2));
        Vertex cornerTopRight1 = isRightTopAndBottomSameVertex ? cornerBottomRight1 : structure.CreateVertex(new Vector3(rightX, rightPlatformHeight, -length / 2));
        Vertex cornerTopRight2 = isRightTopAndBottomSameVertex ? cornerBottomRight2 : structure.CreateVertex(new Vector3(rightX, rightPlatformHeight, length / 2));

        if (rampType == RampType.Left || rampType == RampType.Right) {
            CreateStraightRampPart(
                structure, rampType == RampType.Left,
                cornerTopLeft1, cornerTopLeft2, cornerTopRight1, cornerTopRight2,
                cornerBottomLeft1, cornerBottomLeft2, cornerBottomRight1, cornerBottomRight2,
                leftPlatformWidth == 0, rightPlatformWidth == 0, segmentWidth, segmentLength, curvature, rampFaceType, sideFaceType);
        } else {
            bool isMiddleTopAndBottomSameVertex = rampType == RampType.Valley && baseHeight == 0;
            bool isMiddleLeftAndRightSameVertex = middlePlatformWidth == 0 && !isMiddleTopAndBottomSameVertex;
            Vertex cornerBottomMiddleLeft1 = structure.CreateVertex(new Vector3(-middlePlatformWidth / 2, 0, -length / 2));
            Vertex cornerBottomMiddleLeft2 = structure.CreateVertex(new Vector3(-middlePlatformWidth / 2, 0, length / 2));
            Vertex cornerBottomMiddleRight1 = isMiddleLeftAndRightSameVertex ? cornerBottomMiddleLeft1 : structure.CreateVertex(new Vector3(middlePlatformWidth / 2, 0, -length / 2));
            Vertex cornerBottomMiddleRight2 = isMiddleLeftAndRightSameVertex ? cornerBottomMiddleLeft2 : structure.CreateVertex(new Vector3(middlePlatformWidth / 2, 0, length / 2));
            Vertex cornerTopMiddleLeft1 = isMiddleTopAndBottomSameVertex ? cornerBottomMiddleLeft1 : structure.CreateVertex(new Vector3(-middlePlatformWidth / 2, middlePlatformHeight, -length / 2));
            Vertex cornerTopMiddleLeft2 = isMiddleTopAndBottomSameVertex ? cornerBottomMiddleLeft2 : structure.CreateVertex(new Vector3(-middlePlatformWidth / 2, middlePlatformHeight, length / 2));
            Vertex cornerTopMiddleRight1 = isMiddleTopAndBottomSameVertex ? cornerBottomMiddleRight1 : structure.CreateVertex(new Vector3(middlePlatformWidth / 2, middlePlatformHeight, -length / 2));
            Vertex cornerTopMiddleRight2 = isMiddleTopAndBottomSameVertex ? cornerBottomMiddleRight2 : structure.CreateVertex(new Vector3(middlePlatformWidth / 2, middlePlatformHeight, length / 2));
            CreateStraightRampPart(
                structure, rampType == RampType.Peak,
                cornerTopLeft1, cornerTopLeft2, cornerTopMiddleLeft1, cornerTopMiddleLeft2,
                cornerBottomLeft1, cornerBottomLeft2, cornerBottomMiddleLeft1, cornerBottomMiddleLeft2,
                leftPlatformWidth == 0, false, segmentWidth, segmentLength, curvature, rampFaceType, sideFaceType);
            CreateStraightRampPart(
                structure, rampType == RampType.Valley,
                cornerTopMiddleRight1, cornerTopMiddleRight2, cornerTopRight1, cornerTopRight2,
                cornerBottomMiddleRight1, cornerBottomMiddleRight2, cornerBottomRight1, cornerBottomRight2,
                false, rightPlatformWidth == 0, segmentWidth, segmentLength, curvature, rampFaceType, sideFaceType);
            if (middlePlatformWidth > 0) {
                CreateStraightPlatformPart(
                    structure,
                    cornerTopMiddleLeft1, cornerTopMiddleLeft2, cornerTopMiddleRight1, cornerTopMiddleRight2,
                    cornerBottomMiddleLeft1, cornerBottomMiddleLeft2, cornerBottomMiddleRight1, cornerBottomMiddleRight2,
                    false, false, segmentLength, sideFaceType);
            }
        }

        if (leftPlatformWidth > 0) {
            Vertex cornerBottomFarLeft1 = structure.CreateVertex(cornerBottomLeft1.p + leftPlatformWidth * Vector3.left);
            Vertex cornerBottomFarLeft2 = structure.CreateVertex(cornerBottomLeft2.p + leftPlatformWidth * Vector3.left);
            Vertex cornerTopFarLeft1 = cornerTopLeft1 == cornerBottomLeft1 ? cornerBottomFarLeft1 : structure.CreateVertex(cornerTopLeft1.p + leftPlatformWidth * Vector3.left);
            Vertex cornerTopFarLeft2 = cornerTopLeft2 == cornerBottomLeft2 ? cornerBottomFarLeft2 : structure.CreateVertex(cornerTopLeft2.p + leftPlatformWidth * Vector3.left);
            CreateStraightPlatformPart(
                structure,
                cornerTopFarLeft1, cornerTopFarLeft2, cornerTopLeft1, cornerTopLeft2,
                cornerBottomFarLeft1, cornerBottomFarLeft2, cornerBottomLeft1, cornerBottomLeft2,
                true, false, segmentLength, sideFaceType);
        }
        if (rightPlatformWidth > 0) {
            Vertex cornerBottomFarRight1 = structure.CreateVertex(cornerBottomRight1.p + rightPlatformWidth * Vector3.right);
            Vertex cornerBottomFarRight2 = structure.CreateVertex(cornerBottomRight2.p + rightPlatformWidth * Vector3.right);
            Vertex cornerTopFarRight1 = cornerTopRight1 == cornerBottomRight1 ? cornerBottomFarRight1 : structure.CreateVertex(cornerTopRight1.p + rightPlatformWidth * Vector3.right);
            Vertex cornerTopFarRight2 = cornerTopRight2 == cornerBottomRight2 ? cornerBottomFarRight2 : structure.CreateVertex(cornerTopRight2.p + rightPlatformWidth * Vector3.right);
            CreateStraightPlatformPart(
                structure,
                cornerTopRight1, cornerTopRight2, cornerTopFarRight1, cornerTopFarRight2,
                cornerBottomRight1, cornerBottomRight2, cornerBottomFarRight1, cornerBottomFarRight2,
                false, true, segmentLength, sideFaceType);
        }
        return structure.Build();
    }

    private static void CreateStraightRampPart(
            StructureGeometry structure, bool isLeftRamp,
            Vertex cornerTopLeft1, Vertex cornerTopLeft2, Vertex cornerTopRight1, Vertex cornerTopRight2,
            Vertex cornerBottomLeft1, Vertex cornerBottomLeft2, Vertex cornerBottomRight1, Vertex cornerBottomRight2,
            bool addLeftSide, bool addRightSide, int segmentWidth, int segmentLength, float curvature, RenderGeometry.FaceType rampFaceType, RenderGeometry.FaceType sideFaceType) {

        bool hasBase = (cornerTopLeft1 != cornerBottomLeft1) && (cornerTopRight1 != cornerBottomRight1);

        SurfaceComponentGeometry rampFace = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, segmentWidth, segmentLength, 0, rampFaceType);
        SurfaceComponentGeometry rampBottom = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, segmentLength, 1, sideFaceType);
        SurfaceComponentGeometry rampLeftSide = null, rampRightSide = null;
        if (addLeftSide && (cornerTopLeft1 != cornerBottomLeft1)) {
            rampLeftSide = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, segmentLength, 2, sideFaceType);
        }
        if (addRightSide && (cornerTopRight1 != cornerBottomRight1)) {
            rampRightSide = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, segmentLength, 3, sideFaceType);
        }

        if (curvature != 0) {
            float angle = -curvature * Mathf.PI / 2;
            rampFace.ApplySpaceWarp(new SpaceWarp($"sin(x*{angle})/{angle}", $"(1-cos(x*{angle}))/{angle}", "z"));
        }
        // Fit to a 45 degree ramp, then scale.
        rampFace.ApplyLinearTransform(
            Matrix4x4.Translate(cornerTopLeft1.p) *
            Matrix4x4.Scale(VectorUtil.Abs(cornerTopRight2.p - cornerTopLeft1.p)) *
            MatrixUtil.PointToPointTransform(
                rampFace.corners[0].p, rampFace.corners[3].p, rampFace.corners[1].p,
                Vector3.zero, new Vector3(1, isLeftRamp ? 1 : -1, 0), Vector3.forward));

        structure.CreateFace(rampFace, false, cornerTopLeft1, cornerTopLeft2, cornerTopRight2, cornerTopRight1);
        structure.CreateFace(rampBottom, true, cornerBottomRight1, cornerBottomRight2, cornerBottomLeft2, cornerBottomLeft1);
        if (rampLeftSide != null) {
            structure.CreateFace(rampLeftSide, true, cornerBottomLeft1, cornerBottomLeft2, cornerTopLeft2, cornerTopLeft1);
        }
        if (rampRightSide != null) {
            structure.CreateFace(rampRightSide, true, cornerTopRight1, cornerTopRight2, cornerBottomRight2, cornerBottomRight1);
        }

        var backVertices = new List<Vector3>();
        backVertices.Add(isLeftRamp ? cornerBottomRight1.p : cornerBottomLeft1.p);
        if (isLeftRamp && hasBase) {
            backVertices.Add(cornerBottomLeft1.p);
        }
        backVertices.Add(rampFace.boundaries[3].Last().prev.vertex.p);
        backVertices.AddRange(rampFace.boundaries[3].Select(e => e.vertex.p).Reverse());
        if (!isLeftRamp && hasBase) {
            backVertices.Add(cornerBottomRight1.p);
        }
        var movedBackVertices = backVertices.Select(p => p + (cornerTopLeft2.p - cornerTopLeft1.p));
        var frontVertices = movedBackVertices.Take(1).Concat(movedBackVertices.Reverse().Take(backVertices.Count - 1)).ToList();

        SurfaceComponentGeometry backFace = SurfaceComponentGeometries.CreateStarConvexPolygonGeometry(backVertices, 4, RenderGeometry.FaceType.Smooth);
        SurfaceComponentGeometry frontFace = SurfaceComponentGeometries.CreateStarConvexPolygonGeometry(frontVertices, 5, RenderGeometry.FaceType.Smooth);
        if (hasBase) {
            if (isLeftRamp) {
                backFace.CombineBoundaries(1, 1, segmentWidth, 1);
                frontFace.CombineBoundaries(1, segmentWidth, 1, 1);
                structure.CreateFace(backFace, false, cornerBottomRight1, cornerBottomLeft1, cornerTopLeft1, cornerTopRight1);
                structure.CreateFace(frontFace, false, cornerBottomRight2, cornerTopRight2, cornerTopLeft2, cornerBottomLeft2);
            } else {
                backFace.CombineBoundaries(1, segmentWidth, 1, 1);
                frontFace.CombineBoundaries(1, 1, segmentWidth, 1);
                structure.CreateFace(backFace, false, cornerBottomLeft1, cornerTopLeft1, cornerTopRight1, cornerBottomRight1);
                structure.CreateFace(frontFace, false, cornerBottomLeft2, cornerBottomRight2, cornerTopRight2, cornerTopLeft2);
            }
        } else {
            backFace.CombineBoundaries(1, segmentWidth, 1);
            frontFace.CombineBoundaries(1, segmentWidth, 1);
            structure.CreateFace(backFace, false, isLeftRamp ? cornerBottomRight1 : cornerBottomLeft1, cornerTopLeft1, cornerTopRight1);
            structure.CreateFace(frontFace, false, isLeftRamp ? cornerBottomRight2 : cornerBottomLeft2, cornerTopRight2, cornerTopLeft2);
        }
    }

    private static void CreateStraightPlatformPart(
            StructureGeometry structure,
            Vertex cornerTopLeft1, Vertex cornerTopLeft2, Vertex cornerTopRight1, Vertex cornerTopRight2,
            Vertex cornerBottomLeft1, Vertex cornerBottomLeft2, Vertex cornerBottomRight1, Vertex cornerBottomRight2,
            bool addLeftSide, bool addRightSide, int segmentLength, RenderGeometry.FaceType sideFaceType) {

        SurfaceComponentGeometry topFace = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, segmentLength, 0, sideFaceType);
        SurfaceComponentGeometry bottomFace = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, segmentLength, 1, sideFaceType);

        if (cornerTopLeft1 == cornerBottomLeft1) {
            // Copy vertices since it is a flat face.
            Vertex cornerLeft1 = structure.CreateVertex(cornerBottomLeft1.p);
            Vertex cornerLeft2 = structure.CreateVertex(cornerBottomLeft2.p);
            Vertex cornerRight1 = structure.CreateVertex(cornerBottomRight1.p);
            Vertex cornerRight2 = structure.CreateVertex(cornerBottomRight2.p);
            structure.CreateFace(topFace, true, cornerLeft1, cornerLeft2, cornerRight2, cornerRight1);
            structure.CreateFace(bottomFace, true, cornerRight1, cornerRight2, cornerLeft2, cornerLeft1);
        } else {
            SurfaceComponentGeometry backFace = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, 1, 4, RenderGeometry.FaceType.Polygonal);
            SurfaceComponentGeometry frontFace = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, 1, 5, RenderGeometry.FaceType.Polygonal);
            structure.CreateFace(topFace, true, cornerTopLeft1, cornerTopLeft2, cornerTopRight2, cornerTopRight1);
            structure.CreateFace(bottomFace, true, cornerBottomRight1, cornerBottomRight2, cornerBottomLeft2, cornerBottomLeft1);
            structure.CreateFace(backFace, true, cornerBottomLeft1, cornerTopLeft1, cornerTopRight1, cornerBottomRight1);
            structure.CreateFace(frontFace, true, cornerBottomRight2, cornerTopRight2, cornerTopLeft2, cornerBottomLeft2);

            if (addLeftSide) {
                SurfaceComponentGeometry leftFace = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, segmentLength, 2, sideFaceType);
                structure.CreateFace(leftFace, true, cornerBottomLeft1, cornerBottomLeft2, cornerTopLeft2, cornerTopLeft1);
            }
            if (addRightSide) {
                SurfaceComponentGeometry rightFace = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, segmentLength, 3, sideFaceType);
                structure.CreateFace(rightFace, true, cornerTopRight1, cornerTopRight2, cornerBottomRight2, cornerBottomRight1);
            }
        }
    }



    public static RenderGeometry CreateTurningRampGeometry(
            float width, float height, float angle, float extraRadius, int segmentWidth, int segmentLength, bool smoothWidth, bool smoothLength, RampType rampType = RampType.Right, float curvature = 0,
            float baseHeight = 0, float highSideWidth = 0, float lowSideWidth = 0) {
        StructureGeometry structure = new StructureGeometry();
        RenderGeometry.FaceType rampFaceType = GetFaceType(smoothWidth, smoothLength);
        RenderGeometry.FaceType sideFaceType = GetFaceType(false, smoothLength);

        return structure.Build();
    }

    private static RenderGeometry.FaceType GetFaceType(bool smoothWidth, bool smoothLength) {
        if (smoothWidth && smoothLength) {
            return RenderGeometry.FaceType.Smooth;
        } else if (!smoothWidth && smoothLength) {
            return RenderGeometry.FaceType.Directinal2;
        } else if (smoothWidth && !smoothLength) {
            return RenderGeometry.FaceType.Directinal1;
        } else {
            return RenderGeometry.FaceType.Polygonal;
        }
    }
}
