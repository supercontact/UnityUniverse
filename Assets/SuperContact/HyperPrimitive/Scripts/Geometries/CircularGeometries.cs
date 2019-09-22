using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CircularGeometries {

    public static RenderGeometry CreateSphereGeometry(float radius, int segmentP, int segmentH, bool smoothH, bool smoothV, float cutTop = 0, float cutBottom = 0) {
        if (cutTop + cutBottom > 1) return new RenderGeometry();

        RenderGeometry.FaceType faceType = GetFaceType(smoothH, smoothV);
        StructureGeometry structure = new StructureGeometry();
        SurfaceComponentGeometry upperPart, lowerPart, middlePart;

        if (cutTop != 0) {
            upperPart = SurfaceComponentGeometries.CreateRegularPolygonGeometry(radius * Mathf.Sqrt(4 * cutTop * (1 - cutTop)), segmentP, 2);
            upperPart.ApplyOffset(radius * (1 - 2 * cutTop) * Vector3.up);
        } else {
            float angle = cutBottom != 0 ? Mathf.Acos(-1 + cutBottom * 2) : Mathf.PI * (segmentH / 2) / segmentH;
            upperPart = SurfaceComponentGeometries.CreateSphereCapGeometry(radius, segmentP, cutBottom != 0 ? segmentH : segmentH / 2, 2 * angle, 0, 1, faceType);
        }
        if (cutBottom != 0) {
            lowerPart = SurfaceComponentGeometries.CreateRegularPolygonGeometry(radius * Mathf.Sqrt(4 * cutBottom * (1 - cutBottom)), segmentP, 2);
            lowerPart.ApplyOffset(radius * (1 - 2 * cutBottom) * Vector3.up);
            lowerPart.ApplyRotation(Quaternion.AngleAxis(180, Vector3.right));
        } else {
            float angle = cutTop != 0 ? Mathf.Acos(-1 + cutTop * 2) : Mathf.PI * ((segmentH + 1) / 2) / segmentH;
            lowerPart = SurfaceComponentGeometries.CreateSphereCapGeometry(radius, segmentP, cutTop != 0 ? segmentH : (segmentH + 1) / 2, 2 * angle, 0, 1, faceType);
            lowerPart.ApplyRotation(Quaternion.AngleAxis(180, Vector3.right));
        }
        if (cutTop != 0 && cutBottom != 0) {
            float angle1 = Mathf.Acos(1 - cutTop * 2);
            float angle2 = Mathf.Acos(-1 + cutBottom * 2);
            middlePart = SurfaceComponentGeometries.CreateSphereSideGeometry(radius, angle1, angle2, segmentP, segmentH, 0, 1, faceType);

            Vertex cornerUp = structure.CreateVertex();
            Vertex cornerDown = structure.CreateVertex();
            structure.CreateFace(middlePart, false, cornerUp, cornerUp, cornerDown, cornerDown);
            structure.CreateFace(upperPart, false, cornerUp);
            structure.CreateFace(lowerPart, false, cornerDown);
        } else {
            Vertex corner = structure.CreateVertex();
            structure.CreateFace(upperPart, false, corner);
            structure.CreateFace(lowerPart, false, corner);
        }
        return structure.Build();
    }

    public static RenderGeometry CreateCylinderGeometry(float radius, float height, int segmentP, int segmentH, bool smoothH, bool smoothV, float cutAngle = 0, float hollowRatio = 0) {
        StructureGeometry structure = new StructureGeometry();
        RenderGeometry.FaceType faceType = GetFaceType(smoothH, smoothV);
        float hollowRadius = hollowRatio * radius;

        SurfaceComponentGeometry side = SurfaceComponentGeometries.CreateCylinderSideGeometry(radius, height, segmentP, segmentH, cutAngle, false, 1, faceType);
        SurfaceComponentGeometry sideInner = null;
        if (hollowRatio > 0) {
            sideInner = SurfaceComponentGeometries.CreateCylinderSideGeometry(hollowRadius, height, segmentP, segmentH, cutAngle, true, 1, faceType);
        }
        SurfaceComponentGeometry upperCap, lowerCap;
        if (hollowRatio > 0) {
            upperCap = SurfaceComponentGeometries.CreateRingCapGeometry(radius, hollowRadius, segmentP, 1, cutAngle, 2);
            lowerCap = SurfaceComponentGeometries.CreateRingCapGeometry(radius, hollowRadius, segmentP, 1, cutAngle, 2);
        } else if (cutAngle == 0) {
            upperCap = SurfaceComponentGeometries.CreateRegularPolygonGeometry(radius, segmentP, 2);
            lowerCap = SurfaceComponentGeometries.CreateRegularPolygonGeometry(radius, segmentP, 2);
        } else {
            upperCap = SurfaceComponentGeometries.CreateFanCapGeometry(radius, segmentP, 1, cutAngle, 2);
            lowerCap = SurfaceComponentGeometries.CreateFanCapGeometry(radius, segmentP, 1, cutAngle, 2);
        }
        lowerCap.ApplyRotation(Quaternion.AngleAxis(cutAngle * Mathf.Rad2Deg, Vector3.up) * Quaternion.AngleAxis(180, Vector3.right));

        if (cutAngle == 0) {
            Vertex cornerUp = structure.CreateVertex(new Vector3(radius, height / 2, 0));
            Vertex cornerDown = structure.CreateVertex(new Vector3(radius, -height / 2, 0));
            if (hollowRatio == 0) {
                structure.CreateFace(side, false, cornerUp, cornerUp, cornerDown, cornerDown);
                structure.CreateFace(upperCap, true, cornerUp);
                structure.CreateFace(lowerCap, true, cornerDown);
            } else {
                Vertex cornerUpInner = structure.CreateVertex(new Vector3(hollowRadius, height / 2, 0));
                Vertex cornerDownInner = structure.CreateVertex(new Vector3(hollowRadius, -height / 2, 0));
                structure.CreateFace(side, false, cornerUp, cornerUp, cornerDown, cornerDown);
                structure.CreateFace(sideInner, false, cornerDownInner, cornerDownInner, cornerUpInner, cornerUpInner);
                structure.CreateFace(upperCap, true, cornerUpInner, cornerUpInner, cornerUp, cornerUp);
                structure.CreateFace(lowerCap, true, cornerDownInner, cornerDownInner, cornerDown, cornerDown);
            }
        } else {
            SurfaceComponentGeometry wall1 = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, segmentH, 3);
            SurfaceComponentGeometry wall2 = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, 1, segmentH, 4);

            Vertex cornerUp1 = structure.CreateVertex(new Vector3(radius * Mathf.Cos(cutAngle), height / 2, -radius * Mathf.Sin(cutAngle)));
            Vertex cornerUp2 = structure.CreateVertex(new Vector3(radius, height / 2, 0));
            Vertex cornerDown1 = structure.CreateVertex(new Vector3(radius * Mathf.Cos(cutAngle), -height / 2, -radius * Mathf.Sin(cutAngle)));
            Vertex cornerDown2 = structure.CreateVertex(new Vector3(radius, -height / 2, 0));
            if (hollowRatio == 0) {
                Vertex cornerUpC = structure.CreateVertex(new Vector3(0, height / 2, 0));
                Vertex cornerDownC = structure.CreateVertex(new Vector3(0, -height / 2, 0));
                structure.CreateFace(side, false, cornerUp2, cornerUp1, cornerDown1, cornerDown2);
                structure.CreateFace(upperCap, true, cornerUp1, cornerUp2, cornerUpC);
                structure.CreateFace(lowerCap, true, cornerDown2, cornerDown1, cornerDownC);
                structure.CreateFace(wall1, true, cornerUpC, cornerDownC, cornerDown1, cornerUp1);
                structure.CreateFace(wall2, true, cornerDownC, cornerUpC, cornerUp2, cornerDown2);
            } else {
                Vertex cornerUp1Inner = structure.CreateVertex(new Vector3(hollowRadius * Mathf.Cos(cutAngle), height / 2, -hollowRadius * Mathf.Sin(cutAngle)));
                Vertex cornerUp2Inner = structure.CreateVertex(new Vector3(hollowRadius, height / 2, 0));
                Vertex cornerDown1Inner = structure.CreateVertex(new Vector3(hollowRadius * Mathf.Cos(cutAngle), -height / 2, -hollowRadius * Mathf.Sin(cutAngle)));
                Vertex cornerDown2Inner = structure.CreateVertex(new Vector3(hollowRadius, -height / 2, 0));
                structure.CreateFace(side, false, cornerUp2, cornerUp1, cornerDown1, cornerDown2);
                structure.CreateFace(sideInner, false, cornerDown2Inner, cornerDown1Inner, cornerUp1Inner, cornerUp2Inner);
                structure.CreateFace(upperCap, true, cornerUp2Inner, cornerUp1Inner, cornerUp1, cornerUp2);
                structure.CreateFace(lowerCap, true, cornerDown1Inner, cornerDown2Inner, cornerDown2, cornerDown1);
                structure.CreateFace(wall1, true, cornerUp1Inner, cornerDown1Inner, cornerDown1, cornerUp1);
                structure.CreateFace(wall2, true, cornerDown2Inner, cornerUp2Inner, cornerUp2, cornerDown2);
            }
        }
        return structure.Build();
    }

    public static RenderGeometry CreateCapsuleGeometry(float radius, float height, int segmentP, int segmentH1, int segmentH2, bool smoothH, bool smoothV) {
        StructureGeometry structure = new StructureGeometry();
        RenderGeometry.FaceType faceType = GetFaceType(smoothH, smoothV);

        SurfaceComponentGeometry side = SurfaceComponentGeometries.CreateCylinderSideGeometry(radius, height, segmentP, segmentH1, faceType: faceType);

        SurfaceComponentGeometry upperCap = SurfaceComponentGeometries.CreateSphereCapGeometry(radius, segmentP, segmentH2, faceType: faceType);
        SurfaceComponentGeometry lowerCap = SurfaceComponentGeometries.CreateSphereCapGeometry(radius, segmentP, segmentH2, faceType: faceType);
        lowerCap.ApplyRotation(Quaternion.AngleAxis(180, Vector3.right));

        Vertex cornerUp = structure.CreateVertex(new Vector3(radius, height / 2, 0));
        Vertex cornerDown = structure.CreateVertex(new Vector3(radius, -height / 2, 0));
        structure.CreateFace(side, false, cornerUp, cornerUp, cornerDown, cornerDown);
        structure.CreateFace(upperCap, true, cornerUp);
        structure.CreateFace(lowerCap, true, cornerDown);
        return structure.Build();
    }

    public static RenderGeometry CreateConeGeometry(float radius, float height, int segmentP, int segmentH, bool smoothH, bool smoothV, float cutTop = 0, float cutAngle = 0) {
        if (cutTop == 0) {
            StructureGeometry structure = new StructureGeometry();
            RenderGeometry.FaceType faceType = GetFaceType(smoothH, smoothV);

            SurfaceComponentGeometry coneCap = SurfaceComponentGeometries.CreateConeCapGeometry(radius, height, segmentP, segmentH, cutAngle, 1, faceType);
            if (cutAngle == 0) {
                SurfaceComponentGeometry bottom = SurfaceComponentGeometries.CreateRegularPolygonGeometry(radius, segmentP, 2);
                bottom.ApplyRotation(Quaternion.AngleAxis(180, Vector3.right));

                Vertex corner = structure.CreateVertex();
                structure.CreateFace(coneCap, false, corner);
                structure.CreateFace(bottom, false, corner);
            } else {
                SurfaceComponentGeometry bottom = SurfaceComponentGeometries.CreateFanCapGeometry(radius, segmentP, 1, cutAngle, 2);
                SurfaceComponentGeometry wall1 = SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, segmentH, true, 3);
                SurfaceComponentGeometry wall2 = SurfaceComponentGeometries.CreateTriangleGeometry(1, 1, 0, segmentH, true, 4);

                Vertex cornerUp = structure.CreateVertex(new Vector3(0, height, 0));
                Vertex cornerDownC = structure.CreateVertex(Vector3.zero);
                Vertex cornerDown1 = structure.CreateVertex(new Vector3(radius * Mathf.Cos(cutAngle), 0, -radius * Mathf.Sin(cutAngle)));
                Vertex cornerDown2 = structure.CreateVertex(new Vector3(radius, 0, 0));

                structure.CreateFace(coneCap, true, cornerDown1, cornerDown2, cornerUp);
                structure.CreateFace(bottom, true, cornerDown2, cornerDown1, cornerDownC);
                structure.CreateFace(wall1, true, cornerDown1, cornerUp, cornerDownC);
                structure.CreateFace(wall2, true, cornerDownC, cornerUp, cornerDown2);
            }
            return structure.Build();
        } else {
            RenderGeometry geometry = CreateCylinderGeometry(radius, height, segmentP, segmentH, smoothH, smoothV, cutAngle);
            geometry.ApplyOffset(Vector3.up * (height / 2));

            float shrinkCoeff = (1 - cutTop) / height;
            SpaceWarp warp = new SpaceWarp($"x*(1-y*{shrinkCoeff})", "y", $"z*(1-y*{shrinkCoeff})");
            geometry.ApplySpaceWarp(warp);
            return geometry;
        }
    }

    public static RenderGeometry CreateTorusGeometry(float ringRadius, float barRadius, int segmentRing, int segmentBar, bool smoothH, bool smoothV, float cutAngle = 0, float deltaAngle = 0) {
        StructureGeometry structure = new StructureGeometry();
        RenderGeometry.FaceType faceType = GetFaceType(smoothH, smoothV);

        SurfaceComponentGeometry face = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, segmentBar, segmentRing, 1, faceType);

        var warp = new SpaceWarp(
            $" a1=(z+0.5)*(2*PI-{cutAngle})",
            $" a2={deltaAngle}-x*(2*PI)",
            $" r={ringRadius}+{barRadius}*cos(a2)",
            $" X=r*cos(a1)",
            $" Y={barRadius}*sin(a2)",
            $" Z=r*sin(a1)"
        );
        face.ApplySpaceWarp(warp);

        if (cutAngle == 0) {
            Vertex corner = structure.CreateVertex();
            structure.CreateFace(face, false, corner, corner, corner, corner);
        } else {
            SurfaceComponentGeometry cap1 = SurfaceComponentGeometries.CreateRegularPolygonGeometry(barRadius, segmentBar, 2);
            SurfaceComponentGeometry cap2 = SurfaceComponentGeometries.CreateRegularPolygonGeometry(barRadius, segmentBar, 2);
            cap1.ApplyRotation(Quaternion.LookRotation(Vector3.down, Vector3.back) * Quaternion.AngleAxis(-deltaAngle, Vector3.up));
            cap1.ApplyOffset(Vector3.right * ringRadius);
            cap2.ApplyRotation(Quaternion.LookRotation(Vector3.up, Quaternion.AngleAxis(cutAngle * Mathf.Rad2Deg, Vector3.up) * Vector3.forward) * Quaternion.AngleAxis(deltaAngle * Mathf.Rad2Deg, Vector3.up));
            cap2.ApplyOffset(new Vector3(ringRadius * Mathf.Cos(cutAngle), 0, -ringRadius * Mathf.Sin(cutAngle)));

            Vertex corner1 = structure.CreateVertex();
            Vertex corner2 = structure.CreateVertex();
            structure.CreateFace(face, false, corner1, corner2, corner2, corner1);
            structure.CreateFace(cap1, false, corner1);
            structure.CreateFace(cap2, false, corner2);
        }
        return structure.Build();
    }

    public static RenderGeometry CreateSpringGeometry(float ringRadius, float barRadius, float heightPerCycle, int segmentPerCycle, int segmentBar, bool smoothH, bool smoothV, float angle = 0, float deltaAngle = 0) {
        StructureGeometry structure = new StructureGeometry();
        RenderGeometry.FaceType faceType = GetFaceType(smoothH, smoothV);
        float heightPerRad = heightPerCycle / (2 * Mathf.PI);
        float slope = heightPerRad / ringRadius;
        float scaleH = Mathf.Sqrt(1 + slope * slope);
        int segmentRing = Mathf.CeilToInt(segmentPerCycle * angle / (2 * Mathf.PI) - 1e-6f);

        SurfaceComponentGeometry face = SurfaceComponentGeometries.CreatePlaneGeometry(1, 1, segmentBar, segmentRing, 1, faceType);

        var warp = new SpaceWarp(
            $" a1=(z+0.5)*{angle}",
            $" a2={deltaAngle}-x*(2*PI)",
            $" r={ringRadius}+{barRadius}*cos(a2)",
            $" X=r*cos(a1)",
            $" Y={scaleH}*{barRadius}*sin(a2)+a1*{heightPerRad}",
            $" Z=r*sin(a1)"
        );
        face.ApplySpaceWarp(warp);

        SurfaceComponentGeometry cap1 = SurfaceComponentGeometries.CreateRegularPolygonGeometry(barRadius, segmentBar, 2);
        SurfaceComponentGeometry cap2 = SurfaceComponentGeometries.CreateRegularPolygonGeometry(barRadius, segmentBar, 2);
        cap1.ApplyLinearTransform(
            Matrix4x4.Translate(Vector3.right * ringRadius) *
            Matrix4x4.Scale(new Vector3(1, scaleH, 1)) *
            Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.down, Vector3.back) * Quaternion.AngleAxis(-deltaAngle * Mathf.Rad2Deg, Vector3.up)));
        cap2.ApplyLinearTransform(
            Matrix4x4.Translate(new Vector3(ringRadius * Mathf.Cos(angle), heightPerRad * angle, ringRadius * Mathf.Sin(angle))) *
            Matrix4x4.Scale(new Vector3(1, scaleH, 1)) *
            Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.up, Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.down) * Vector3.forward) * Quaternion.AngleAxis(deltaAngle * Mathf.Rad2Deg, Vector3.up)));

        Vertex corner1 = structure.CreateVertex();
        Vertex corner2 = structure.CreateVertex();
        structure.CreateFace(face, false, corner1, corner2, corner2, corner1);
        structure.CreateFace(cap1, false, corner1);
        structure.CreateFace(cap2, false, corner2);
        return structure.Build();
    }

    private static RenderGeometry.FaceType GetFaceType(bool smoothH, bool smoothV) {
        if (smoothH && smoothV) {
            return RenderGeometry.FaceType.Smooth;
        } else if (!smoothH && smoothV) {
            return RenderGeometry.FaceType.Directinal1;
        } else if (smoothH && !smoothV) {
            return RenderGeometry.FaceType.Directinal2;
        } else {
            return RenderGeometry.FaceType.Polygonal;
        }
    }
}
