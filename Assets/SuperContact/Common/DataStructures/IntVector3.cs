using ProtoBuf;
using System;
using System.Linq;
using UnityEngine;

[ProtoContract]
public struct IntVector3 {
    public static IntVector3 zero = new IntVector3(0, 0, 0);
    public static IntVector3 one = new IntVector3(1, 1, 1);
    public static IntVector3 right = new IntVector3(1, 0, 0);
    public static IntVector3 left = new IntVector3(-1, 0, 0);
    public static IntVector3 up = new IntVector3(0, 1, 0);
    public static IntVector3 down = new IntVector3(0, -1, 0);
    public static IntVector3 fwd = new IntVector3(0, 0, 1);
    public static IntVector3 back = new IntVector3(0, 0, -1);
    public static IntVector3[] allAxialDirections = { right, left, up, down, fwd, back };
    public static IntVector3[] allBiaxialDirections = {
        new IntVector3(1, 1, 0), new IntVector3(0, 1, 1),  new IntVector3(1, 0, 1),
        new IntVector3(1, -1, 0), new IntVector3(0, 1, -1), new IntVector3(1, 0, -1),
        new IntVector3(-1, 1, 0), new IntVector3(0, -1, 1), new IntVector3(-1, 0, 1),
        new IntVector3(-1, -1, 0), new IntVector3(0, -1, -1), new IntVector3(-1, 0, -1)};
    public static IntVector3[] allTriaxialDirections = {
        new IntVector3(1, 1, 1), new IntVector3(1, 1, -1), new IntVector3(1, -1, 1), new IntVector3(1, -1, -1),
        new IntVector3(-1, 1, 1), new IntVector3(-1, 1, -1), new IntVector3(-1, -1, 1), new IntVector3(-1, -1, -1)};
    public static IntVector3[] allDirections = allAxialDirections.Concat(allBiaxialDirections).Concat(allTriaxialDirections).ToArray();

    [ProtoMember(1)]
    public int x;
    [ProtoMember(2)]
    public int y;
    [ProtoMember(3)]
    public int z;

    public IntVector3(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public int this[int index] {
        get {
            switch (index) {
            case 0:
                return x;
            case 1:
                return y;
            case 2:
                return z;
            default:
                throw new IndexOutOfRangeException("Invalid index: " + index);
            }
        }
        set {
            switch (index) {
            case 0:
                x = value;
                break;
            case 1:
                y = value;
                break;
            case 2:
                z = value;
                break;
            default:
                throw new IndexOutOfRangeException("Invalid index: " + index);
            }
        }
    }

    public void Set(int newX, int newY, int newZ) {
        x = newX;
        y = newY;
        z = newZ;
    }

    public void Clamp(IntVector3 min, IntVector3 max) {
        for (int i = 0; i < 3; i++) {
            this[i] = Mathf.Max(this[i], min[i]);
            this[i] = Mathf.Min(this[i], max[i]);
        }
    }

    public IntVector3 Clamped(IntVector3 min, IntVector3 max) {
        return Min(Max(this, min), max);
    }

    public IntVector3 Reversed(bool reverse = true) {
        return reverse ? new IntVector3(z, y, x) : this;
    }

    public int Norm1() {
        return Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
    }

    public float Norm2() {
        return Mathf.Sqrt(SqrNorm2());
    }

    public int SqrNorm2() {
        return x * x + y * y + z * z;
    }

    public int NormInf() {
        return Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z));
    }

    public Vector3 ToVector3() {
        return new Vector3(x, y, z);
    }

    public override bool Equals(object other) {
        return other is IntVector3 && this == (IntVector3)other;
    }

    public override int GetHashCode() {
        return HashCode.Combine(x, y, z);
    }

    public override string ToString() {
        return "[" + x + ", " + y + ", " + z + "]";
    }

    public static IntVector3 operator +(IntVector3 a, IntVector3 b) {
        return new IntVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static IntVector3 operator -(IntVector3 a) {
        return new IntVector3(-a.x, -a.y, -a.z);
    }

    public static IntVector3 operator -(IntVector3 a, IntVector3 b) {
        return new IntVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static IntVector3 operator *(int d, IntVector3 a) {
        return new IntVector3(d * a.x, d * a.y, d * a.z);
    }

    public static IntVector3 operator *(IntVector3 a, int d) {
        return new IntVector3(d * a.x, d * a.y, d * a.z);
    }

    public static Vector3 operator *(float d, IntVector3 a) {
        return new Vector3(d * a.x, d * a.y, d * a.z);
    }

    public static Vector3 operator *(IntVector3 a, float d) {
        return new Vector3(d * a.x, d * a.y, d * a.z);
    }

    public static IntVector3 operator *(IntVector3 a, IntVector3 b) {
        return new IntVector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 operator *(IntVector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 operator *(Vector3 a, IntVector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static IntVector3 operator /(IntVector3 a, int d) {
        return new IntVector3(a.x.Div(d), a.y.Div(d), a.z.Div(d));
    }

    public static IntVector3 operator /(IntVector3 a, IntVector3 b) {
        return new IntVector3(a.x.Div(b.x), a.y.Div(b.y), a.z.Div(b.z));
    }

    public static IntVector3 operator %(IntVector3 a, int d) {
        return new IntVector3(a.x.Mod(d), a.y.Mod(d), a.z.Mod(d));
    }

    public static IntVector3 operator %(IntVector3 a, IntVector3 b) {
        return new IntVector3(a.x.Mod(b.x), a.y.Mod(b.y), a.z.Mod(b.z));
    }

    public void Div(int d, out IntVector3 q, out IntVector3 r) {
        x.Div(d, out q.x, out r.x);
        y.Div(d, out q.y, out r.y);
        z.Div(d, out q.z, out r.z);
    }

    public void Div(IntVector3 b, out IntVector3 q, out IntVector3 r) {
        x.Div(b.x, out q.x, out r.x);
        y.Div(b.y, out q.y, out r.y);
        z.Div(b.z, out q.z, out r.z);
    }

    public static IntVector3 operator >>(IntVector3 a, int rotation) {
        rotation = rotation.Mod(3);
        if (rotation == 0) {
            return a;
        } else if (rotation == 1) {
            return new IntVector3(a.z, a.x, a.y);
        } else {
            return new IntVector3(a.y, a.z, a.x);
        }
    }

    public static IntVector3 operator <<(IntVector3 a, int rotation) {
        return a >> -rotation;
    }

    public static bool operator ==(IntVector3 a, IntVector3 b) {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(IntVector3 a, IntVector3 b) {
        return a.x != b.x || a.y != b.y || a.z != b.z;
    }

    public static bool operator >(IntVector3 a, IntVector3 b) {
        return a.x > b.x && a.y > b.y && a.z > b.z;
    }

    public static bool operator <(IntVector3 a, IntVector3 b) {
        return a.x < b.x && a.y < b.y && a.z < b.z;
    }

    public static bool operator >=(IntVector3 a, IntVector3 b) {
        return a.x >= b.x && a.y >= b.y && a.z >= b.z;
    }

    public static bool operator <=(IntVector3 a, IntVector3 b) {
        return a.x <= b.x && a.y <= b.y && a.z <= b.z;
    }

    public static int Dot(IntVector3 a, IntVector3 b) {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static IntVector3 Cross(IntVector3 a, IntVector3 b) {
        return new IntVector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }

    public static IntVector3 Max(IntVector3 a, IntVector3 b) {
        return new IntVector3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
    }

    public static IntVector3 Min(IntVector3 a, IntVector3 b) {
        return new IntVector3(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
    }

    public static IntVector3 Abs(IntVector3 a) {
        return new IntVector3(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));
    }

    public static IntVector3 Floor(Vector3 v) {
        return new IntVector3(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
    }

    public static IntVector3 Ceil(Vector3 v) {
        return new IntVector3(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z));
    }

    public static IntVector3 Round(Vector3 v) {
        return new IntVector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }

    public static bool IsCollinear(IntVector3 a, IntVector3 b) {
        return a.x * b.y == a.y * b.x && a.y * b.z == a.z * b.y && a.z * b.x == a.x * b.z;
    }

    public static bool IsCollinearSameDirection(IntVector3 a, IntVector3 b) {
        return IsCollinear(a, b) && (a.x * b.x > 0 || a.y * b.y > 0 || a.z * b.z > 0);
    }

    public static bool IsCollinearOppositeDirection(IntVector3 a, IntVector3 b) {
        return IsCollinear(a, b) && (a.x * b.x < 0 || a.y * b.y < 0 || a.z * b.z < 0);
    }

    public static IntVector3 GetRandomVectorInRange(IntBox boxRange) {
        return new IntVector3(
            UnityEngine.Random.Range(boxRange.min.x, boxRange.max.x + 1),
            UnityEngine.Random.Range(boxRange.min.y, boxRange.max.y + 1),
            UnityEngine.Random.Range(boxRange.min.z, boxRange.max.z + 1));
    }

    public static IntVector3 GetRandomVectorInRange(IntBox boxRange, System.Random random) {
        return new IntVector3(
            random.Next(boxRange.min.x, boxRange.max.x + 1),
            random.Next(boxRange.min.y, boxRange.max.y + 1),
            random.Next(boxRange.min.z, boxRange.max.z + 1));
    }
}
