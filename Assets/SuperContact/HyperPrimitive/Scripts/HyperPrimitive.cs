using UnityEngine;
using SuperContact.MathExpression;

public class HyperPrimitive : MonoBehaviour {

    public enum HyperPrimitiveType {
        Plane = 0,
        Triangle = 1,
        Polygon = 2,
        PolygonFan = 3,
        Sphere = 100,
        Cylinder = 101,
        Capsule = 102,
        Cone = 103,
        Torus = 104,
        Spring = 105,
        Cube = 200,
        CubeStar = 201,
        CubeFrame = 401,
        BuildingBlock = 400,
        Tetrahedron = 202,
        TetrahedronStar = 203,
        Octahedron = 204,
        OctahedronStar = 205,
        Dodecahedron = 206,
        DodecahedronStar = 207,
        Icosahedron = 208,
        IcosahedronStar = 209,
        TrunctedTetrahedron = 210,
        TrunctedCubeOctahedron = 211,
        TrunctedIcosahedronDodecahedron = 212,
        Ramp = 300,
        Test = 999999,
    }

    public HyperPrimitiveType type = HyperPrimitiveType.Plane;

    public float sizeX = 1, sizeY = 1, sizeZ = 1, sizeR = 1, sizeR2 = 0.5f;
    public float extraSizeX = 0, extraSizeX2 = 0, extraSizeY = 0;
    public int segmentX = 1, segmentY = 1, segmentZ = 1, segmentP = 24, segmentP2 = 12;
    public float offset = 0;
    public float extrusion = 1;
    public float ratio = 0.5f, hollowRatio = 0;
    public float cutTop = 0, cutBottom = 0, cutAngle = 0;
    public bool cutEdge = false;
    public float angle = 360, angleOffset = 0;
    public float curvature = 0;
    public bool smooth = true, smoothX = true, smoothZ = true, smoothH = true, smoothV = true;
    public bool xyz = true, xyZ = true, xYz = true, xYZ = true, Xyz = true, XyZ = true, XYz = true, XYZ = true;
    public RampGeometries.RampType rampType = RampGeometries.RampType.Right;
    public RenderGeometry.Facing surfaceFacing = RenderGeometry.Facing.Normal;
    public RenderGeometry.GlobalSurfaceType globalSurfaceType = RenderGeometry.GlobalSurfaceType.Normal;
    public bool meshSaved = false;

    public string message = "";

    public RenderGeometry currentGeometry;

    void OnValidate() {
        UpdateMesh();
    }

    public void UpdateMesh() {
        if (meshSaved) return;

        RenderGeometry g = null;
        if (type == HyperPrimitiveType.Plane) {
            g = SurfaceComponentGeometries.CreatePlaneGeometry(sizeX, sizeZ, segmentX, segmentZ);
        } else if (type == HyperPrimitiveType.Triangle) {
            g = SurfaceComponentGeometries.CreateTriangleGeometry(sizeX, sizeZ, offset, segmentX);
        } else if (type == HyperPrimitiveType.Polygon) {
            g = SurfaceComponentGeometries.CreateRegularPolygonGeometry(sizeR, segmentP);
        } else if (type == HyperPrimitiveType.PolygonFan) {
            g = SurfaceComponentGeometries.CreateFanCapGeometry(sizeR, segmentP, segmentY, Rad(cutAngle));
        } else if (type == HyperPrimitiveType.Sphere) {
            g = CircularGeometries.CreateSphereGeometry(sizeR, segmentP, segmentP2, smoothH, smoothV, cutTop, cutBottom);
        } else if (type == HyperPrimitiveType.Cylinder) {
            g = CircularGeometries.CreateCylinderGeometry(sizeR, sizeY, segmentP, segmentY, smoothH, smoothV, Rad(cutAngle), hollowRatio);
        } else if (type == HyperPrimitiveType.Capsule) {
            g = CircularGeometries.CreateCapsuleGeometry(sizeR, sizeY, segmentP, segmentY, segmentP2, smoothH, smoothV);
        } else if (type == HyperPrimitiveType.Cone) {
            g = CircularGeometries.CreateConeGeometry(sizeR, sizeY, segmentP, segmentP2, smoothH, smoothV, cutTop, Rad(cutAngle));
        } else if (type == HyperPrimitiveType.Torus) {
            g = CircularGeometries.CreateTorusGeometry(sizeR, sizeR2, segmentP, segmentP2, smoothH, smoothV, Rad(cutAngle), Rad(angleOffset));
        } else if (type == HyperPrimitiveType.Spring) {
            g = CircularGeometries.CreateSpringGeometry(sizeR, sizeR2, sizeY, segmentP, segmentP2, smoothH, smoothV, Rad(angle), Rad(angleOffset));
        } else if (type == HyperPrimitiveType.Cube) {
            g = PolyhedronGeometries.CreateCubeGeometry(new Vector3(sizeX, sizeY, sizeZ), new int[] { segmentX, segmentY, segmentZ });
        } else if (type == HyperPrimitiveType.CubeStar) {
            g = PolyhedronGeometries.CreateCubeStarGeometry(sizeX, extrusion, cutTop);
        } else if (type == HyperPrimitiveType.CubeFrame) {
            g = PolyhedronGeometries.CreateCubeFrameGeometry(sizeX, ratio);
        } else if (type == HyperPrimitiveType.BuildingBlock) {
            g = PolyhedronGeometries.CreateBuildingBlockGeometry(new Vector3(sizeX, sizeY, sizeZ), new bool[,,] { { { xyz, xyZ }, { xYz, xYZ } }, { { Xyz, XyZ }, { XYz, XYZ } } });
        } else if (type == HyperPrimitiveType.Tetrahedron) {
            g = PolyhedronGeometries.CreateTetrahedronGeometry(sizeX, segmentX);
        } else if (type == HyperPrimitiveType.TetrahedronStar) {
            g = PolyhedronGeometries.CreateTetrahedronStarGeometry(sizeX, extrusion, cutTop);
        } else if (type == HyperPrimitiveType.Octahedron) {
            g = PolyhedronGeometries.CreateOctahedronGeometry(sizeX, segmentX);
        } else if (type == HyperPrimitiveType.OctahedronStar) {
            g = PolyhedronGeometries.CreateOctahedronStarGeometry(sizeX, extrusion, cutTop);
        } else if (type == HyperPrimitiveType.Dodecahedron) {
            g = PolyhedronGeometries.CreateDodecahedronGeometry(sizeX);
        } else if (type == HyperPrimitiveType.DodecahedronStar) {
            g = PolyhedronGeometries.CreateDodecahedronStarGeometry(sizeX, extrusion, cutTop);
        } else if (type == HyperPrimitiveType.Icosahedron) {
            g = PolyhedronGeometries.CreateIcosahedronGeometry(sizeX, segmentX);
        } else if (type == HyperPrimitiveType.IcosahedronStar) {
            g = PolyhedronGeometries.CreateIcosahedronStarGeometry(sizeX, extrusion, cutTop);
        } else if (type == HyperPrimitiveType.TrunctedTetrahedron) {
            g = PolyhedronGeometries.CreateTetrahedronToTetrahedronTransitionGeometry(sizeX, ratio, cutEdge);
        } else if (type == HyperPrimitiveType.TrunctedCubeOctahedron) {
            g = PolyhedronGeometries.CreateCubeToOctahedronTransitionGeometry(sizeX, ratio, cutEdge);
        } else if (type == HyperPrimitiveType.TrunctedIcosahedronDodecahedron) {
            g = PolyhedronGeometries.CreateIcosahedronToDodecahedronTransitionGeometry(sizeX, ratio, cutEdge);
        } else if (type == HyperPrimitiveType.Ramp) {
            g = RampGeometries.CreateStraightRampGeometry(sizeX, sizeY, sizeZ, segmentX, segmentZ, smoothX, smoothZ, rampType, curvature, extraSizeY, extraSizeX, extraSizeX2);
        } else if (type == HyperPrimitiveType.Test) {
            g = SpecialSurfaceComponentGeometries.CreateSplitTriangleGeometry(1, 1, 0, 3);
        } else {
            g = new RenderGeometry();
        }

        foreach (HyperModifier modifier in GetComponents<HyperModifier>()) {
            modifier.Apply(g);
        }

        GetComponent<MeshFilter>().sharedMesh = g.ToMesh(surfaceFacing, globalSurfaceType);
        currentGeometry = g;
    }

    public static GameObject CreatePrimitive() {
        GameObject newPrimitive = Instantiate(Resources.Load<GameObject>("defaultobject"));
        newPrimitive.name = "Primitive";
        newPrimitive.AddComponent<HyperPrimitive>();
        return newPrimitive;
    }

    private static float Rad(float angleInDegrees) {
        return angleInDegrees * Mathf.Deg2Rad;
    }
}
