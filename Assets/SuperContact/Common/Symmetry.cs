using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Symmetry {

    public IntVector3 xyz;
    public int rotation;
    public bool reverse;
    public bool isNegative {
        get { return (xyz.x * xyz.y * xyz.z < 0) ^ reverse; }
    }

    public Symmetry(IntVector3 xyz, int rotation, bool reverse) {
        this.xyz = xyz;
        this.rotation = rotation;
        this.reverse = reverse;
    }

    public IntVector3 Apply(IntVector3 vector) {
        return (xyz * vector).Reversed(reverse) >> rotation;
    }

    public Vector3 Apply(Vector3 vector) {
        return (xyz * vector).Reversed(reverse).Shift(rotation);
    }

    public IntVector3[] Apply(params IntVector3[] vectorArray) {
        var result = vectorArray.Select(vector => Apply(vector));
        return isNegative ? result.Reverse().ToArray() : result.ToArray();
    }

    public Vector3[] Apply(params Vector3[] vectorArray) {
        var result = vectorArray.Select(vector => Apply(vector));
        return isNegative ? result.Reverse().ToArray() : result.ToArray();
    }

    public static IEnumerable<Symmetry> SymmetryGroup(string symmetryType) {
        ParseSymmetryType(symmetryType, out bool shouldRotate, out bool shouldReverse, out bool shouldFlipX, out bool shouldFlipY, out bool shouldFlipZ, out bool positiveOnly, out bool negativeOnly);

        for (int r = 0; r < (shouldRotate ? 3 : 1); r++) {
            for (int x = shouldFlipX ? -1 : 1; x <= 1; x += 2) {
                for (int y = shouldFlipY ? -1 : 1; y <= 1; y += 2) {
                    for (int z = shouldFlipZ ? -1 : 1; z <= 1; z += 2) {
                        for (int t = 0; t < (shouldReverse ? 2 : 1); t++) {
                            var result = new Symmetry(new IntVector3(x, y, z), r, t > 0 ? true : false);
                            if ((positiveOnly && result.isNegative) || (negativeOnly && !result.isNegative)) continue;
                            yield return result;
                        }
                    }
                }
            }
        }
    }

    private static void ParseSymmetryType(string symmetryType, out bool shouldRotate, out bool shouldReverse, out bool shouldFlipX, out bool shouldFlipY, out bool shouldFlipZ, out bool positiveOnly, out bool negativeOnly) {
        shouldRotate = symmetryType[0] != symmetryType[1] || symmetryType[1] != symmetryType[2] || symmetryType[2] != symmetryType[0];
        shouldReverse = symmetryType[0] != symmetryType[1] && symmetryType[1] != symmetryType[2] && symmetryType[2] != symmetryType[0];
        shouldFlipX = symmetryType[0] != '0';
        shouldFlipY = symmetryType[1] != '0';
        shouldFlipZ = symmetryType[2] != '0';
        positiveOnly = symmetryType.Length > 3 && symmetryType[3] == '+';
        negativeOnly = symmetryType.Length > 3 && symmetryType[3] == '-';
    }
}
