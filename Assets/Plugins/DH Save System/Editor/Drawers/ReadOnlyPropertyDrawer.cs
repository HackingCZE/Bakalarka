using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DH.Save.Attributes;
namespace DH.Save.Editor.Drawers
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}
#endif
}