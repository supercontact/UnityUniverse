using UnityEngine;
using System.Collections.Generic;

public struct IntBox {
    public static IntBox empty = new IntBox(IntVector3.zero, -IntVector3.one);

    public IntVector3 min;
    public IntVector3 max;

    public IntBox(IntVector3 size) {
        min = IntVector3.zero;
        max = size - IntVector3.one;
    }

    public IntBox(IntVector3 min, IntVector3 max) {
        this.min = min;
        this.max = max;
    }

    public IntBox(int minX, int minY, int minZ, int maxX, int maxY, int maxZ) {
        min = new IntVector3(minX, minY, minZ);
        max = new IntVector3(maxX, maxY, maxZ);
    }

    public IntVector3 size {
        get { return max - min + IntVector3.one; }
    }

    public bool isEmpty {
        get { return max.x - min.x < 0 || max.y - min.y < 0 || max.z - min.z < 0; }
    }

    public IEnumerable<IntVector3> allPointsInside {
        get {
            for (int x = min.x; x <= max.x; x++) {
                for (int y = min.y; y <= max.y; y++) {
                    for (int z = min.z; z <= max.z; z++) {
                        yield return new IntVector3(x, y, z);
                    }
                }
            }
        }
    }

    public IEnumerable<IntVector3> allPointsInsideWall {
        get {
            if (isEmpty) yield break;
            if (min.x == max.x) {
                for (int y = min.y; y <= max.y; y++) {
                    for (int z = min.z; z <= max.z; z++) {
                        yield return new IntVector3(min.x, y, z);
                    }
                }
            } else if (min.y == max.y) {
                for (int x = min.x; x <= max.x; x++) {
                    for (int z = min.z; z <= max.z; z++) {
                        yield return new IntVector3(x, min.y, z);
                    }
                }
            } else if (min.z == max.z) {
                for (int x = min.x; x <= max.x; x++) {
                    for (int y = min.y; y <= max.y; y++) {
                        yield return new IntVector3(x, y, min.z);
                    }
                }
            } else {
                for (int x = min.x; x <= max.x; x++) {
                    for (int y = min.y; y <= max.y; y++) {
                        yield return new IntVector3(x, y, min.z);
                        yield return new IntVector3(x, y, max.z);
                    }
                    for (int z = min.z + 1; z < max.z; z++) {
                        yield return new IntVector3(x, min.y, z);
                        yield return new IntVector3(x, max.y, z);
                    }
                }
                for (int y = min.y + 1; y < max.y; y++) {
                    for (int z = min.z + 1; z < max.z; z++) {
                        yield return new IntVector3(min.x, y, z);
                        yield return new IntVector3(max.x, y, z);
                    }
                }
            }
        }
    }

    public IEnumerable<IntVector3> allPointsOutsideWall {
        get {
            for (int x = min.x; x <= max.x; x++) {
                for (int y = min.y; y <= max.y; y++) {
                    yield return new IntVector3(x, y, min.z - 1);
                    yield return new IntVector3(x, y, max.z + 1);
                }
                for (int z = min.z; z <= max.z; z++) {
                    yield return new IntVector3(x, min.y - 1, z);
                    yield return new IntVector3(x, max.y + 1, z);
                }
            }
            for (int y = min.y; y <= max.y; y++) {
                for (int z = min.z; z <= max.z; z++) {
                    yield return new IntVector3(min.x - 1, y, z);
                    yield return new IntVector3(max.x + 1, y, z);
                }
            }
        }
    }

    public bool Intersects(IntBox other) {
        return !Intersection(this, other).isEmpty;
    }

    public bool Contains(IntVector3 point) {
        return point >= min && point <= max;
    }

    public bool Contains(IntBox other) {
        if (other.isEmpty) return true;
        if (isEmpty) return false;
        return other.min >= min && other.max <= max;
    }

    public IntBox Expand(int distance) {
        IntVector3 delta = new IntVector3(distance, distance, distance);
        return new IntBox(min - delta, max + delta);
    }

    public IntBox Offset(IntVector3 offset) {
        return new IntBox(min + offset, max + offset);
    }

    /* public bool IntersectsLine(Vector3 p1, Vector3 p2, float tolerance = 0) {
        Vector3 c = center;
        float r = size / 2 - tolerance;
        p1 -= c;
        p2 -= c;
        float xm, xp, ym, yp, zm, zp;
        xm = Mathf.Min(p1.x, p2.x);
        xp = Mathf.Max(p1.x, p2.x);
        ym = Mathf.Min(p1.y, p2.y);
        yp = Mathf.Max(p1.y, p2.y);
        zm = Mathf.Min(p1.z, p2.z);
        zp = Mathf.Max(p1.z, p2.z);
        if (xm >= r || xp < -r || ym >= r || yp < -r || zm >= r || zp < -r) return false;

        for (int i = 0; i < 3; i++) {
            Vector3 a = Vector3.zero;
            a[i] = 1;
            a = Vector3.Cross(a, p2 - p1);
            float d = Mathf.Abs(Vector3.Dot(p1, a));
            float rr = r * (Mathf.Abs(a[(i + 1) % 3]) + Mathf.Abs(a[(i + 2) % 3]));
            if (d > rr) return false;
        }

        return true;
    }

    public bool IntersectTriangle(Vector3 p1, Vector3 p2, Vector3 p3, float tolerance = 0) {
        Vector3 c = center;
        float r = size / 2 - tolerance;
        p1 -= c;
        p2 -= c;
        p3 -= c;
        float xm, xp, ym, yp, zm, zp;
        xm = Mathf.Min(p1.x, p2.x, p3.x);
        xp = Mathf.Max(p1.x, p2.x, p3.x);
        ym = Mathf.Min(p1.y, p2.y, p3.y);
        yp = Mathf.Max(p1.y, p2.y, p3.y);
        zm = Mathf.Min(p1.z, p2.z, p3.z);
        zp = Mathf.Max(p1.z, p2.z, p3.z);
        if (xm >= r || xp < -r || ym >= r || yp < -r || zm >= r || zp < -r) return false;

        Vector3 n = Vector3.Cross(p2 - p1, p3 - p1);
        float d = Mathf.Abs(Vector3.Dot(p1, n));
        if (d > r * (Mathf.Abs(n.x) + Mathf.Abs(n.y) + Mathf.Abs(n.z))) return false;

        Vector3[] p = { p1, p2, p3 };
        Vector3[] f = { p3 - p2, p1 - p3, p2 - p1 };
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                Vector3 a = Vector3.zero;
                a[i] = 1;
                a = Vector3.Cross(a, f[j]);
                float d1 = Vector3.Dot(p[j], a);
                float d2 = Vector3.Dot(p[(j + 1) % 3], a);
                float rr = r * (Mathf.Abs(a[(i + 1) % 3]) + Mathf.Abs(a[(i + 2) % 3]));
                if (Mathf.Min(d1, d2) > rr || Mathf.Max(d1, d2) < -rr) return false;
            }
        }
        return true;
    }

    public bool IntersectSphere(Vector3 sphereCenter, float radius, bool sphereSurfaceOnly = false) {
        Vector3 c1 = corner(0);
        Vector3 c2 = corner(7);
        float r2 = radius * radius;
        for (int i = 0; i < 3; i++) {
            if (sphereCenter[i] < c1[i]) r2 -= (sphereCenter[i] - c1[i]) * (sphereCenter[i] - c1[i]);
            else if (sphereCenter[i] > c2[i]) r2 -= (sphereCenter[i] - c2[i]) * (sphereCenter[i] - c2[i]);
        }
        return r2 > 0 && !(sphereSurfaceOnly && IsInSphere(sphereCenter, radius));
    }

    public bool ContainSphere(Vector3 sphereCenter, float radius) {
        Vector3 c1 = corner(0);
        Vector3 c2 = corner(7);
        return
            sphereCenter.x > c1.x + radius && sphereCenter.x < c2.x - radius &&
            sphereCenter.y > c1.y + radius && sphereCenter.y < c2.y - radius &&
            sphereCenter.z > c1.z + radius && sphereCenter.z < c2.z - radius;
    }

    public bool IsInSphere(Vector3 sphereCenter, float radius) {
        Vector3 v1 = corner(0) - sphereCenter;
        Vector3 v2 = corner(7) - sphereCenter;
        float xmax = Mathf.Max(Mathf.Abs(v1.x), Mathf.Abs(v2.x));
        float ymax = Mathf.Max(Mathf.Abs(v1.y), Mathf.Abs(v2.y));
        float zmax = Mathf.Max(Mathf.Abs(v1.z), Mathf.Abs(v2.z));
        return xmax * xmax + ymax * ymax + zmax * zmax < radius * radius;
    } */

    public override string ToString() {
        return string.Format("[Box: {0} -> {1}]", min, max);
    }

    public static IntBox Intersection(IntBox b1, IntBox b2) {
        return new IntBox(IntVector3.Max(b1.min, b2.min), IntVector3.Min(b1.max, b2.max));
    }

    public static IntBox OuterBound(IntBox b1, IntBox b2) {
        if (b1.isEmpty) return b2;
        if (b2.isEmpty) return b1;
        return new IntBox(IntVector3.Min(b1.min, b2.min), IntVector3.Max(b1.max, b2.max));
    }
}
