using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HyperModifier))]
[CanEditMultipleObjects]
public class HyperModifierEditor : Editor {

    private EditorHelper editor = new EditorHelper(typeof(HyperModifier));

    void OnEnable() {
        editor.SetUpObject(serializedObject);
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        if (serializedObject.targetObjects.Select(obj => (obj as HyperModifier).GetComponent<HyperPrimitive>().meshSaved).Any(saved => saved)) {
            return;
        }

        EditorGUILayout.Space();
        editor.AddField("type");

        SerializedProperty type = editor.GetProperty("type");
        if (!type.hasMultipleDifferentValues) {
            switch ((HyperModifier.HyperModifierType)type.intValue) {
            case HyperModifier.HyperModifierType.CommonTransform:
                editor.AddField("translation");
                editor.AddField("rotation");
                editor.AddField("scale");
                EditorGUILayout.Space();
                editor.AddField("pivot");
                break;
            case HyperModifier.HyperModifierType.MatrixTransform:
                editor.AddLabel("Transform Matrix");
                for (int r = 0; r < 4; r++) {
                    editor.BeginVector();
                    for (int c = 0; c < 4; c++) {
                        editor.AddField($"m{r}{c}", "");
                    }
                    editor.EndVector();
                }
                break;
            case HyperModifier.HyperModifierType.BendTransform:
                editor.AddField("curvature");
                editor.AddField("axis1", "Axis");
                editor.AddField("direction");
                EditorGUILayout.Space();
                editor.AddField("pivot");
                break;
            case HyperModifier.HyperModifierType.TwistTransform:
                editor.AddField("angle");
                editor.AddField("axis2", "Axis");
                EditorGUILayout.Space();
                editor.AddField("pivot");
                break;
            case HyperModifier.HyperModifierType.CustomTransform:
                editor.AddField("exprX", "X=", 24);
                editor.AddField("exprY", "Y=", 24);
                editor.AddField("exprZ", "Z=", 24);
                break;
            case HyperModifier.HyperModifierType.FaceCurving:
                editor.AddField("curvature");
                break;
            case HyperModifier.HyperModifierType.FaceMerging:
                editor.AddField("angleThresholdSmall", "Angle Threshold");
                break;
            case HyperModifier.HyperModifierType.EdgeSmoothing:
                editor.AddField("smoothRadius").SetRange(0f);
                editor.AddField("angleThreshold").SetRange(0f, 180f);
                break;
            }
        }

        SerializedProperty message = editor.GetProperty("message");
        if (!message.hasMultipleDifferentValues && message.stringValue != "") {
            EditorGUILayout.HelpBox(message.stringValue, MessageType.Error);
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Delete modifier", GUILayout.Width(128))) {
            foreach (Object obj in serializedObject.targetObjects) {
                var hyperModifier = obj as HyperModifier;
                var hyperPrimitive = hyperModifier.GetComponent<HyperPrimitive>();
                Undo.DestroyObjectImmediate(hyperModifier);
                if (hyperPrimitive != null) {
                    hyperPrimitive.UpdateMesh();
                }
            }
            return;
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
