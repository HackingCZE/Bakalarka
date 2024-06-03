using DH.Save.Attributes;
using UnityEditor;
using UnityEngine;


namespace DH.Save.Editor.Drawers
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ToggleAttribute))]
    public class TogglePropertyDrawer : PropertyDrawer
    {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ToggleAttribute toggleAttr = (ToggleAttribute)attribute;
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical();

        float height = toggleAttr.boxHeight;
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        buttonStyle.fixedHeight = height;

        GUIContent buttonContent = new GUIContent(label.text, property.tooltip);

        Rect rect = EditorUtils.CalculateBoxRect(toggleAttr.totalBoxes, toggleAttr.boxIndex, toggleAttr.boxHeight, position.y, position.x);

        if (GUI.Button(rect, buttonContent, buttonStyle))
        {
            property.boolValue = !property.boolValue;
        }       

        rect.x += 4;
        rect.y += (height - EditorGUIUtility.singleLineHeight) / 2;
        rect.height = EditorGUIUtility.singleLineHeight;        
        
        property.boolValue = EditorGUI.Toggle(rect, property.boolValue);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
}
#endif
}

