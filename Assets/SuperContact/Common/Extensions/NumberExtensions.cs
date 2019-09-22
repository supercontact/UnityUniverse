using UnityEngine;
using System.Collections.Generic;

public static class NumberExtensions {

    public static int Pow(this int num, int exp) {
        int result = 1;
        while (exp > 0) {
            if (exp % 2 == 1) {
                result *= num;
            }
            exp >>= 1;
            num *= num;
        }
        return result;
    }

    public static int Div(this int k, int n) {
        return k >= 0 ? k / n : (k + 1) / n - 1;
    }

    public static void Div(this int k, int n, out int q, out int r) {
        q = k >= 0 ? k / n : (k + 1) / n - 1;
        r = k - q * n;
    }

    public static int Div(this float k, float n) {
        return Mathf.FloorToInt(k / n);
    }

    public static void Div(this float k, float n, out int q, out float r) {
        q = Mathf.FloorToInt(k / n);
        r = k - q * n;
    }

    public static int Mod(this int k, int n) {
        return k >= 0 ? k % n : (k + 1) % n + n - 1;
    }

    public static float Mod(this float k, float n) {
        return k - Div(k, n) * n;
    }

    public static bool Approximately(this float a, float b, float epsilonMultiplicative = 5e-6f, float epsilonAdditive = 5e-6f) {
        return Mathf.Abs(b - a) < Mathf.Max(epsilonMultiplicative * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)), epsilonAdditive);
    }

    public static uint SetBit(this uint number, bool bitIsOne, int pos) {
        if (bitIsOne) {
            return number | (1u << pos);
        } else {
            return number & ~(1u << pos);
        }
    }

    public static bool GetBit(this uint number, int pos) {
        return ((number >> pos) & 1u) == 1u;
    }

    public static int NumberOfBit(this uint number) {
        int result = 0;
        while (number != 0) {
            number >>= 1;
            result++;
        }
        return result;
    }
}

public static class NumberUtil {
    public static float NormalizeAngleDeg(float angleInDegree) {
        return LimitInCircularInterval(angleInDegree, -180, 180);
    }

    public static float NormalizeAngleRad(float angleInRadian) {
        return LimitInCircularInterval(angleInRadian, -Mathf.PI, Mathf.PI);
    }

    public static float LimitInCircularInterval(float number, float min, float max) {
        return min + (number - min).Mod(max - min);
    }
}