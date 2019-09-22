using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(HyperPrimitive))]
[CanEditMultipleObjects]
public class HyperPrimitiveEditor : Editor {

    EditorHelper editor = new EditorHelper(typeof(HyperPrimitive));

    void OnEnable() {
        editor.SetUpObject(serializedObject);
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        SerializedProperty meshSaved = editor.GetProperty("meshSaved");
        if (meshSaved.hasMultipleDifferentValues) {
            return;
        }
        if (meshSaved.boolValue) {
            EditorGUILayout.HelpBox($"The mesh of this primitive has been saved and linked. Editing this primitive again will break the link to the saved mesh.", MessageType.Info);
            if (GUILayout.Button("Edit this primitive again")) {
                foreach (Object obj in serializedObject.targetObjects) {
                    var hyperPrimitive = obj as HyperPrimitive;
                    hyperPrimitive.meshSaved = false;
                    hyperPrimitive.UpdateMesh();
                }
            }
            return;
        }

        EditorGUILayout.Space();
        editor.AddField("type");

        SerializedProperty type = editor.GetProperty("type");
        if (!type.hasMultipleDifferentValues) {
            switch ((HyperPrimitive.HyperPrimitiveType)type.intValue) {
            case HyperPrimitive.HyperPrimitiveType.Plane:
                editor.BeginVector("Size");
                editor.AddFieldCompact("sizeX", "X").SetRange(0f);
                editor.AddFieldCompact("sizeZ", "Z").SetRange(0f);
                editor.EndVector();

                editor.BeginVector("Segment Count");
                editor.AddFieldCompact("segmentX", "X").SetRange(1);
                editor.AddFieldCompact("segmentZ", "Z").SetRange(1);
                editor.EndVector();
                break;

            case HyperPrimitive.HyperPrimitiveType.Triangle:
                editor.BeginVector("Size");
                editor.AddFieldCompact("sizeX", "W").SetRange(0f);
                editor.AddFieldCompact("sizeZ", "H").SetRange(0f);
                editor.EndVector();

                editor.AddField("offset", "Top Offset");
                editor.AddField("segmentX", "Segment Count").SetRange(1);
                break;

            case HyperPrimitive.HyperPrimitiveType.Polygon:
                editor.AddField("sizeR", "Radius").SetRange(0f);
                editor.AddField("segmentP", "Sides").SetRange(3);
                break;

            case HyperPrimitive.HyperPrimitiveType.PolygonFan:
                editor.AddField("sizeR", "Radius").SetRange(0f);
                editor.AddField("segmentP", "Sides").SetRange(3);
                editor.AddField("segmentY", "Segment Count").SetRange(1);
                editor.AddField("cutAngle", "Cut Angle").SetRange(0f, 360f);
                break;

            case HyperPrimitive.HyperPrimitiveType.Sphere:
                editor.AddField("sizeR", "Radius");

                editor.BeginVector("Segment Count");
                editor.AddFieldCompact("segmentP", "P").SetRange(3);
                editor.AddFieldCompact("segmentP2", "H").SetRange(2);
                editor.EndVector();

                editor.AddField("cutTop").SetRange(0f, 1f);
                editor.AddField("cutBottom").SetRange(0f, 1f);
                editor.AddField("smoothH");
                editor.AddField("smoothV");
                break;

            case HyperPrimitive.HyperPrimitiveType.Cylinder:
                editor.BeginVector("Size");
                editor.AddFieldCompact("sizeR", "R").SetRange(0f);
                editor.AddFieldCompact("sizeY", "H").SetRange(0f);
                editor.EndVector();

                editor.BeginVector("Segment Count");
                editor.AddFieldCompact("segmentP", "P").SetRange(3);
                editor.AddFieldCompact("segmentY", "H").SetRange(1);
                editor.EndVector();

                editor.AddField("cutAngle").SetRange(0f, 360f);
                editor.AddField("hollowRatio", "Hollow Ratio").SetRange(0f, 1f);
                editor.AddField("smoothH");
                editor.AddField("smoothV");
                break;

            case HyperPrimitive.HyperPrimitiveType.Capsule:
                editor.BeginVector("Size");
                editor.AddFieldCompact("sizeR", "R").SetRange(0f);
                editor.AddFieldCompact("sizeY", "H").SetRange(0f);
                editor.EndVector();

                editor.BeginVector("Segment Count");
                editor.AddFieldCompact("segmentP", "P").SetRange(3);
                editor.AddFieldCompact("segmentY", "H1").SetRange(1);
                editor.AddFieldCompact("segmentP2", "H2").SetRange(1);
                editor.EndVector();

                editor.AddField("smoothH");
                editor.AddField("smoothV");
                break;

            case HyperPrimitive.HyperPrimitiveType.Cone:
                editor.BeginVector("Size");
                editor.AddFieldCompact("sizeR", "R").SetRange(0f);
                editor.AddFieldCompact("sizeY", "H").SetRange(0f);
                editor.EndVector();

                editor.BeginVector("Segment Count");
                editor.AddFieldCompact("segmentP", "P").SetRange(3);
                editor.AddFieldCompact("segmentP2", "H").SetRange(1);
                editor.EndVector();

                editor.AddField("cutTop").SetRange(0f, 1f);
                editor.AddField("cutAngle").SetRange(0f, 360f);
                editor.AddField("smoothH");
                editor.AddField("smoothV");
                break;

            case HyperPrimitive.HyperPrimitiveType.Torus:
                editor.AddField("sizeR", "Ring Radius").SetRange(0f);
                editor.AddField("sizeR2", "Bar Radius").SetRange(0f);

                editor.BeginVector("Segment Count");
                editor.AddFieldCompact("segmentP", "P1").SetRange(3);
                editor.AddFieldCompact("segmentP2", "P2").SetRange(3);
                editor.EndVector();

                editor.AddField("cutAngle").SetRange(0f, 360f);
                editor.AddField("angleOffset", "Adjust Angle").SetRange(0f, 360f);
                editor.AddField("smoothH");
                editor.AddField("smoothV");
                break;

            case HyperPrimitive.HyperPrimitiveType.Spring:
                editor.AddField("sizeR", "Ring Radius").SetRange(0f);
                editor.AddField("sizeR2", "Bar Radius").SetRange(0f);
                editor.AddField("sizeY", "Height per Cycle");

                editor.BeginVector("Segment Count");
                editor.AddFieldCompact("segmentP", "P1").SetRange(3);
                editor.AddFieldCompact("segmentP2", "P2").SetRange(3);
                editor.EndVector();

                editor.AddField("angle").SetRange(0f);
                editor.AddField("angleOffset", "Adjust Angle").SetRange(0f, 360f);
                editor.AddField("smoothH");
                editor.AddField("smoothV");
                break;

            case HyperPrimitive.HyperPrimitiveType.Cube:
                editor.BeginVector("Size");
                editor.AddFieldCompact("sizeX", "X").SetRange(0f);
                editor.AddFieldCompact("sizeY", "Y").SetRange(0f);
                editor.AddFieldCompact("sizeZ", "Z").SetRange(0f);
                editor.EndVector();

                editor.BeginVector("Segment Count");
                editor.AddFieldCompact("segmentX", "X").SetRange(1);
                editor.AddFieldCompact("segmentY", "Y").SetRange(1);
                editor.AddFieldCompact("segmentZ", "Z").SetRange(1);
                editor.EndVector();
                break;

            case HyperPrimitive.HyperPrimitiveType.Tetrahedron:
            case HyperPrimitive.HyperPrimitiveType.Octahedron:
            case HyperPrimitive.HyperPrimitiveType.Icosahedron:
                editor.AddField("sizeX", "Size").SetRange(0f);
                editor.AddField("segmentX", "Segment Count").SetRange(1);
                break;

            case HyperPrimitive.HyperPrimitiveType.Dodecahedron:
                editor.AddField("sizeX", "Size").SetRange(0f);
                break;

            case HyperPrimitive.HyperPrimitiveType.CubeStar:
            case HyperPrimitive.HyperPrimitiveType.TetrahedronStar:
            case HyperPrimitive.HyperPrimitiveType.OctahedronStar:
            case HyperPrimitive.HyperPrimitiveType.DodecahedronStar:
            case HyperPrimitive.HyperPrimitiveType.IcosahedronStar:
                editor.AddField("sizeX", "Size").SetRange(0f);
                editor.AddField("extrusion", "Extrusion");
                editor.AddField("cutTop").SetRange(0f);
                break;

            case HyperPrimitive.HyperPrimitiveType.TrunctedTetrahedron:
            case HyperPrimitive.HyperPrimitiveType.TrunctedCubeOctahedron:
            case HyperPrimitive.HyperPrimitiveType.TrunctedIcosahedronDodecahedron:
                editor.AddField("sizeX", "Size").SetRange(0f);
                editor.AddField("ratio").SetRange(0f, 1f);
                editor.AddField("cutEdge");
                break;

            case HyperPrimitive.HyperPrimitiveType.CubeFrame:
                editor.AddField("sizeX", "Size").SetRange(0f);
                editor.AddField("ratio").SetRange(0f, 1f);
                break;

            case HyperPrimitive.HyperPrimitiveType.BuildingBlock:
                editor.AddLabel("Top");
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                editor.AddFieldCompact("xYZ", "LB");
                editor.AddFieldCompact("XYZ", "RB");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                editor.AddFieldCompact("xYz", "LF");
                editor.AddFieldCompact("XYz", "RF");
                EditorGUILayout.EndHorizontal();

                editor.AddLabel("Bottom");
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                editor.AddFieldCompact("xyZ", "LB");
                editor.AddFieldCompact("XyZ", "RB");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                editor.AddFieldCompact("xyz", "LF");
                editor.AddFieldCompact("Xyz", "RF");
                EditorGUILayout.EndHorizontal();

                editor.BeginVector("Size");
                editor.AddFieldCompact("sizeX", "X").SetRange(0f);
                editor.AddFieldCompact("sizeY", "Y").SetRange(0f);
                editor.AddFieldCompact("sizeZ", "Z").SetRange(0f);
                editor.EndVector();
                break;

            case HyperPrimitive.HyperPrimitiveType.Ramp:
                editor.AddField("rampType");

                editor.BeginVector("Size");
                editor.AddFieldCompact("sizeX", "W").SetRange(0f);
                editor.AddFieldCompact("sizeY", "H").SetRange(0f);
                editor.AddFieldCompact("sizeZ", "L").SetRange(0f);
                editor.EndVector();

                editor.BeginVector("Segment Count");
                editor.AddFieldCompact("segmentX", "W").SetRange(1);
                editor.AddFieldCompact("segmentZ", "L").SetRange(1);
                editor.EndVector();

                editor.AddField("curvature").SetRange(-1f, 1f);

                var curvature = editor.GetProperty("curvature");
                var segmentX = editor.GetProperty("segmentX");
                if (!curvature.hasMultipleDifferentValues && curvature.floatValue != 0 && !segmentX.hasMultipleDifferentValues && segmentX.intValue == 1) {
                    EditorGUILayout.LabelField("You need more width segments to display curvature!");
                }

                editor.AddField("extraSizeY", "Base Height").SetRange(0f);
                editor.AddField("extraSizeX", "High Platform Width").SetRange(0f);
                editor.AddField("extraSizeX2", "Low Platform Width").SetRange(0f);

                editor.AddField("smoothX", "Smooth Width");
                editor.AddField("smoothZ", "Smooth Length");
                break;
            }
        }

        EditorGUILayout.Space();
        editor.AddField("surfaceFacing");
        editor.AddField("globalSurfaceType");

        EditorGUILayout.Space();
        if (GUILayout.Button("Add a modifier")) {
            foreach (Object obj in serializedObject.targetObjects) {
                var hyperPrimitive = obj as HyperPrimitive;
                Undo.AddComponent<HyperModifier>(hyperPrimitive.gameObject);
            }
        }

        if (GUILayout.Button("Save mesh as asset")) {
            SaveMeshAsAsset();
        }

        if (!serializedObject.isEditingMultipleObjects) {
            EditorGUILayout.Space();
            HyperPrimitive obj = serializedObject.targetObject as HyperPrimitive;
            RenderGeometry geometry = obj.currentGeometry;
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            EditorGUILayout.LabelField("Geometry vertex count = " + geometry.vertices.Count);
            EditorGUILayout.LabelField("Geometry polygon count = " + geometry.faces.Count);
            EditorGUILayout.LabelField("Mesh vertex count = " + mesh.vertexCount);
            EditorGUILayout.LabelField("Mesh triangle count = " + mesh.triangles.Length / 3);
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void SaveMeshAsAsset() {
        var objects = serializedObject.targetObjects.Select(obj => obj as HyperPrimitive);

        if (!AssetDatabase.IsValidFolder("Assets/PrimitiveExports")) {
            AssetDatabase.CreateFolder("Assets", "PrimitiveExports");
        }
        foreach (HyperPrimitive obj in objects) {
            AssetDatabase.CreateAsset(obj.GetComponent<MeshFilter>().sharedMesh, AssetDatabase.GenerateUniqueAssetPath($"Assets/PrimitiveExports/{obj.gameObject.name}.asset"));
            obj.meshSaved = true;
        }
        AssetDatabase.SaveAssets();
    }
}
