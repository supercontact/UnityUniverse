using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtensions {
    public static void ToAngleAxisCorrected(this Quaternion rotation, out float angle, out Vector3 axis) {
        rotation.ToAngleAxis(out float angleTemp, out Vector3 axisTemp);
        if (float.IsInfinity(axisTemp.x)) {
            angle = 0;
            axis = Vector3.right;
            return;
        }
        angleTemp = angleTemp % 360;
        if (angleTemp > 180) {
            axisTemp = -axisTemp;
            angleTemp = 360 - angleTemp;
        }
        angle = angleTemp;
        axis = axisTemp;
    }
}
