using UnityEngine;

public static class VectorExtensions {

    public static float Norm1(this Vector3 a) {
        return Mathf.Abs(a.x) + Mathf.Abs(a.y) + Mathf.Abs(a.z);
    }

    public static float NormInf(this Vector3 a) {
        return Mathf.Max(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));
    }

    public static Vector3 Times(this Vector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 Div(this Vector3 a, Vector3 b) {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static void Div(this Vector3 a, float d, out IntVector3 q, out Vector3 r) {
        a.x.Div(d, out q.x, out r.x);
        a.y.Div(d, out q.y, out r.y);
        a.z.Div(d, out q.z, out r.z);
    }

    public static void Div(this Vector3 a, Vector3 b, out IntVector3 q, out Vector3 r) {
        a.x.Div(b.x, out q.x, out r.x);
        a.y.Div(b.y, out q.y, out r.y);
        a.z.Div(b.z, out q.z, out r.z);
    }

    public static Vector3 Mod(this Vector3 a, float d) {
        return new Vector3(a.x.Mod(d), a.y.Mod(d), a.z.Mod(d));
    }

    public static Vector3 Mod(this Vector3 a, Vector3 b) {
        return new Vector3(a.x.Mod(b.x), a.y.Mod(b.y), a.z.Mod(b.z));
    }

    public static Vector3 Shift(this Vector3 a, int rotation) {
        rotation = rotation.Mod(3);
        if (rotation == 0) {
            return a;
        } else if (rotation == 1) {
            return new Vector3(a.z, a.x, a.y);
        } else {
            return new Vector3(a.y, a.z, a.x);
        }
    }

    public static Vector3 Reversed(this Vector3 a, bool reverse = true) {
        return reverse ? new Vector3(a.z, a.y, a.x) : a;
    }

    public static bool GreaterThan(this Vector3 a, Vector3 b) {
        return a.x > b.x && a.y > b.y && a.z > b.z;
    }

    public static bool LessThan(this Vector3 a, Vector3 b) {
        return a.x < b.x && a.y < b.y && a.z < b.z;
    }

    public static bool GreaterThanOrEqualTo(this Vector3 a, Vector3 b) {
        return a.x >= b.x && a.y >= b.y && a.z >= b.z;
    }

    public static bool LessThanOrEqualTo(this Vector3 a, Vector3 b) {
        return a.x <= b.x && a.y <= b.y && a.z <= b.z;
    }

    public static bool Approximately(this Vector3 a, Vector3 b, float epsilonMultiplicative = 5e-6f, float epsilonAdditive = 5e-6f) {
        return (b - a).NormInf() < Mathf.Max(epsilonMultiplicative * Mathf.Max(a.NormInf(), b.NormInf()), epsilonAdditive);
    }
}

public static class VectorUtil {

    public static Vector3 Abs(Vector3 v) {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
}