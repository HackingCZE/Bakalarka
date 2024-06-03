using DH.Save.SerializableTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DH.Save.Editor.Drawers
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializableColor))]
    public class SerializableColorDrawer : PropertyDrawer
    {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty rProp = property.FindPropertyRelative("r");
        SerializedProperty gProp = property.FindPropertyRelative("g");
        SerializedProperty bProp = property.FindPropertyRelative("b");
        SerializedProperty aProp = property.FindPropertyRelative("a");

        Color color = new Color(rProp.floatValue, gProp.floatValue, bProp.floatValue, aProp.floatValue);

        color = EditorGUI.ColorField(position, label, color);

        rProp.floatValue = color.r;
        gProp.floatValue = color.g;
        bProp.floatValue = color.b;
        aProp.floatValue = color.a;
    }

}
#endif
}