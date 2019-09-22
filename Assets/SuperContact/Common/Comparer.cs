using System.Collections.Generic;
using UnityEngine;

public static class Comparer {
    public static int CombinedCompare<T>(IComparer<T> comparer, params T[] XsAndYs) {
        int n = XsAndYs.Length / 2;
        for (int i = 0; i < n; i++) {
            int result = comparer.Compare(XsAndYs[i], XsAndYs[i + n]);
            if (result != 0) return result;
        }
        return 0;
    }

    public static int CombineCompareResults(params int[] compareResults) {
        foreach (int compareResult in compareResults) {
            if (compareResult != 0) return compareResult;
        }
        return 0;
    } 
}

public class ApproximateFloatComparer : IComparer<float> {
    private readonly float epsilonMultiplicative;
    private readonly float epsilonAdditive;

    public ApproximateFloatComparer(float epsilonMultiplicative = 5e-6f, float epsilonAdditive = 5e-6f) {
        this.epsilonMultiplicative = epsilonMultiplicative;
        this.epsilonAdditive = epsilonAdditive;
    }

    public int Compare(float x, float y) {
        if (x.Approximately(y, epsilonMultiplicative, epsilonAdditive)) return 0;
        return x < y ? -1 : 1;
    }
}