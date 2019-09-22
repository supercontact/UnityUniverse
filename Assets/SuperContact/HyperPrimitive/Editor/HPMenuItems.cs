using UnityEngine;
using UnityEditor;
using System.Collections;

public class HPMenuItems : MonoBehaviour {

#if UNITY_4_5
	[MenuItem("GameObject/Create Other/HyperPrimitive", false, -1)]
#else
    [MenuItem("GameObject/3D Object/HyperPrimitive", false, -1)]
#endif
    private static void CreateHyperPrimitive(MenuCommand command) {
        GameObject newPrimitive = HyperPrimitive.CreatePrimitive();

        Object selectedGameObject = command.context;
        Vector3 newPosition = Vector3.zero;
        for (int i = 0; i < SceneView.sceneViews.Count; i++) {
            if (SceneView.sceneViews[i] != null) {
                if (selectedGameObject == null)
                    newPosition = ((SceneView)SceneView.sceneViews[i]).pivot;
                break;
            }
        }
        newPrimitive.transform.rotation = Quaternion.identity;
        if (selectedGameObject != null)
            newPrimitive.transform.parent = Selection.activeGameObject.transform;
        newPrimitive.transform.localPosition = newPosition;
        Selection.activeGameObject = newPrimitive;
    }
}
