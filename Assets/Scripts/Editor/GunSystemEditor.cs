using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WeaponData))]
public class WeaponDataDrawer : PropertyDrawer
{
    private bool expanded = false;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (expanded && property.objectReferenceValue != null)
        {
            SerializedObject so = new SerializedObject(property.objectReferenceValue);
            SerializedProperty prop = so.GetIterator();
            prop.NextVisible(true); // skip m_Script
            while (prop.NextVisible(false))
            {
                height += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            height += EditorGUIUtility.standardVerticalSpacing * 2;
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Strzalka rozwijajaca (tylko jesli przypisany asset)
        if (property.objectReferenceValue != null)
        {
            Rect arrowRect = new Rect(lineRect.x, lineRect.y, 14f, lineRect.height);
            expanded = EditorGUI.Foldout(arrowRect, expanded, GUIContent.none);
            lineRect.xMin += 14f;
        }

        // Standardowe pole Unity z przyciskami Select / None
        EditorGUI.ObjectField(lineRect, property, typeof(WeaponData), label);

        // Rozwijany edytor inline
        if (expanded && property.objectReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            SerializedObject so = new SerializedObject(property.objectReferenceValue);
            so.Update();

            SerializedProperty prop = so.GetIterator();
            prop.NextVisible(true); // pomin m_Script

            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            while (prop.NextVisible(false))
            {
                float propHeight = EditorGUI.GetPropertyHeight(prop, true);
                Rect propRect = new Rect(position.x, y, position.width, propHeight);
                EditorGUI.PropertyField(propRect, prop, true);
                y += propHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            if (so.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(property.objectReferenceValue);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }
}
