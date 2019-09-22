using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class EditorHelper {

    public class Field {
        public SerializedProperty property;
        public Field(SerializedProperty property) {
            this.property = property;
        }

        public void SetRange(int min) {
            SetRange(min, int.MaxValue);
        }

        public void SetRange(int min, int max) {
            if (!property.hasMultipleDifferentValues) {
                property.intValue = Mathf.Clamp(property.intValue, min, max);
            }
        }

        public void SetRange(float min) {
            SetRange(min, float.MaxValue);
        }

        public void SetRange(float min, float max) {
            if (!property.hasMultipleDifferentValues) {
                property.floatValue = Mathf.Clamp(property.floatValue, min, max);
            }
        }
    }
    
    public Dictionary<string, SerializedProperty> properties = new Dictionary<string, SerializedProperty>();

    private Type classType;
    private SerializedObject serializedObject;

    public EditorHelper(Type classType) {
        this.classType = classType;
    }

    public void SetUpObject(SerializedObject serializedObject) {
        this.serializedObject = serializedObject;
        properties.Clear();
        foreach (string property in GetAllFieldNames()) {
            properties[property] = serializedObject.FindProperty(property);
        }
    }

    public SerializedProperty GetProperty(string propertyName) {
        return properties[propertyName];
    }

    public void AddLabel(string label, float labelWidth = 0) {
        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUILayout.LabelField(label);
        EditorGUIUtility.labelWidth = 0;
    }

    public Field AddField(string propertyName) {
        EditorGUILayout.PropertyField(properties[propertyName]);
        return new Field(properties[propertyName]);
    }

    public Field AddField(string propertyName, string label, float labelWidth = 0) {
        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUILayout.PropertyField(properties[propertyName], new GUIContent(label));
        EditorGUIUtility.labelWidth = 0;
        return new Field(properties[propertyName]);
    }

    public Field AddFieldCompact(string propertyName, string label) {
        EditorGUIUtility.labelWidth = 8 + label.Length * 6;
        EditorGUILayout.PropertyField(properties[propertyName], new GUIContent(label), GUILayout.MinWidth(40));
        EditorGUIUtility.labelWidth = 0;
        return new Field(properties[propertyName]);
    }

    public void BeginVector(string name = "") {
        EditorGUILayout.BeginHorizontal();
        if (name.Length > 0) {
            EditorGUILayout.LabelField(name, GUILayout.Width(Mathf.Max(0.45f * EditorGUIUtility.currentViewWidth - 45, 115)));
        }
    }

    public void EndVector() {
        EditorGUILayout.EndHorizontal();
    }

    private List<string> GetAllFieldNames() {
        return classType.GetFields().Where(f => f.IsPublic).Select(f => f.Name).ToList();
    }
}
