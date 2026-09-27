using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty sourceProp = property.serializedObject.FindProperty(showIf.ConditionalSourceField);

        bool enabled = true;
        if (sourceProp != null && sourceProp.propertyType == SerializedPropertyType.Boolean)
        {
            enabled = sourceProp.boolValue;
        }

        if (enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty sourceProp = property.serializedObject.FindProperty(showIf.ConditionalSourceField);

        if (sourceProp != null && sourceProp.propertyType == SerializedPropertyType.Boolean && !sourceProp.boolValue)
        {
            return 0f; // Ukrywa pole (brak wysokości)
        }

        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}