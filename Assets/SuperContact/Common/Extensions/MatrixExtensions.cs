using UnityEngine;

public static class MatrixExtensions {

    public static Matrix4x4 CofactorMatrix(this Matrix4x4 matrix) {
        Matrix4x4 result = new Matrix4x4();
        for (int r = 0; r < 3; r++) {
            for (int c = 0; c < 3; c++) {
                result[r, c] =
                    matrix[(r + 1) % 3, (c + 1) % 3] *
                    matrix[(r + 2) % 3, (c + 2) % 3] -
                    matrix[(r + 1) % 3, (c + 2) % 3] *
                    matrix[(r + 2) % 3, (c + 1) % 3];
            }
        }
        result.m33 = 1;
        return result;
    }

    public static Matrix4x4 Adjugate(this Matrix4x4 matrix) {
        return matrix.CofactorMatrix().transpose;
    }

    public static bool IsTranslationOnly(this Matrix4x4 matrix) {
        return new Matrix4x4(matrix.GetColumn(0), matrix.GetColumn(1), matrix.GetColumn(2), new Vector4(0, 0, 0, matrix.m33)).isIdentity;
    }
}

public static class MatrixUtil {

    public static Matrix4x4 FromMatrix(Vector3 origin, Vector3 xAxis, Vector3 yAxis, Vector3 zAxis) {
        return ToMatrix(origin, xAxis, yAxis, zAxis).inverse;
    }

    public static Matrix4x4 ToMatrix(Vector3 origin, Vector3 xAxis, Vector3 yAxis, Vector3 zAxis) {
        Matrix4x4 mat = new Matrix4x4();
        mat.SetColumn(0, xAxis);
        mat.SetColumn(1, yAxis);
        mat.SetColumn(2, zAxis);
        mat.SetColumn(3, origin);
        mat.m33 = 1;
        return mat;
    }

    public static Matrix4x4 FromToMatrix(Vector3 originFrom, Vector3 xAxisFrom, Vector3 yAxisFrom, Vector3 zAxisFrom, Vector3 originTo, Vector3 xAxisTo, Vector3 yAxisTo, Vector3 zAxisTo) {
        return ToMatrix(originTo, xAxisTo, yAxisTo, zAxisTo) * FromMatrix(originFrom, xAxisFrom, yAxisFrom, zAxisFrom);
    }

    public static Matrix4x4 PointToPointTransform(Vector3 p1From, Vector3 p1To) {
        return Matrix4x4.Translate(p1To - p1From);
    }

    public static Matrix4x4 PointToPointTransform(Vector3 p1From, Vector3 p2From, Vector3 p1To, Vector3 p2To) {
        Vector3 vFrom = p2From - p1From;
        Vector3 vTo = p2To - p1To;
        return
            Matrix4x4.Translate(p1To) *
            Matrix4x4.Scale(Vector3.one * (vTo.magnitude / vFrom.magnitude)) *
            Matrix4x4.Rotate(Quaternion.FromToRotation(vFrom, vTo)) *
            Matrix4x4.Translate(-p1From);
    }

    public static Matrix4x4 PointToPointTransform(Vector3 p1From, Vector3 p2From, Vector3 p3From, Vector3 p1To, Vector3 p2To, Vector3 p3To) {
        Vector3 xAxisFrom = p2From - p1From;
        Vector3 yAxisFrom = p3From - p1From;
        Vector3 zAxisFrom = Vector3.Cross(xAxisFrom, yAxisFrom).normalized * xAxisFrom.magnitude;
        Vector3 xAxisTo = p2To - p1To;
        Vector3 yAxisTo = p3To - p1To;
        Vector3 zAxisTo = Vector3.Cross(xAxisTo, yAxisTo).normalized * xAxisTo.magnitude;
        return FromToMatrix(p1From, xAxisFrom, yAxisFrom, zAxisFrom, p1To, xAxisTo, yAxisTo, zAxisTo);
    }

    public static Matrix4x4 PointToPointTransform(Vector3 p1From, Vector3 p2From, Vector3 p3From, Vector3 p4From, Vector3 p1To, Vector3 p2To, Vector3 p3To, Vector3 p4To) {
        return FromToMatrix(p1From, p2From - p1From, p3From - p1From, p4From - p1From, p1To, p2To - p1To, p3To - p1To, p4To - p1To);
    }
}