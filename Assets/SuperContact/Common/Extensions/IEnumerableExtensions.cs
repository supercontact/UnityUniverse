using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class IEnumerableExtensions {

    public static string ToString<T>(this IEnumerable<T> objects, string separator) {
        return string.Join(separator, objects);
    }

    public static void ForEach<T>(this IEnumerable<T> objects, Action<T> action) {
        foreach (T item in objects) {
            action(item);
        }
    }

    public static IEnumerable<T> Rotate<T>(this IEnumerable<T> objects, int rotateAmount) {
        int count = objects.Count();
        rotateAmount = rotateAmount.Mod(count);
        return objects.Skip(count - rotateAmount).Concat(objects.Take(count - rotateAmount));
    }

    public static Vector3 Sum(this IEnumerable<Vector3> vectors) {
        return vectors.Aggregate(Vector3.zero, (v1, v2) => v1 + v2);
    }

    public static IntVector3 Sum(this IEnumerable<IntVector3> vectors) {
        return vectors.Aggregate(IntVector3.zero, (v1, v2) => v1 + v2);
    }

    public static Vector3 Sum<T>(this IEnumerable<T> objects, Func<T, Vector3> sumSelector) {
        return objects.Select(sumSelector).Sum();
    }

    public static IntVector3 Sum<T>(this IEnumerable<T> objects, Func<T, IntVector3> sumSelector) {
        return objects.Select(sumSelector).Sum();
    }

    public static Vector3 Average(this IEnumerable<Vector3> vectors) {
        return vectors.Sum() / vectors.Count();
    }

    public static IntVector3 Average(this IEnumerable<IntVector3> vectors) {
        return vectors.Sum() / vectors.Count();
    }

    public static Vector3 Average<T>(this IEnumerable<T> objects, Func<T, Vector3> averageSelector) {
        return objects.Select(averageSelector).Average();
    }

    public static IntVector3 Average<T>(this IEnumerable<T> objects, Func<T, IntVector3> averageSelector) {
        return objects.Select(averageSelector).Average();
    }
}
