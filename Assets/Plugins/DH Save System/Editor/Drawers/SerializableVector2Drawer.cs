using DH.Save.SerializableTypes;
using UnityEditor;
using UnityEngine;


namespace DH.Save.Editor.Drawers
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializableVector2))]
    public class SerializableVector2Drawer : PropertyDrawer
    {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty xProp = property.FindPropertyRelative("x");
        SerializedProperty yProp = property.FindPropertyRelative("y");

        Vector2 vector2 = new Vector2(xProp.floatValue, yProp.floatValue);

        EditorGUI.BeginChangeCheck();
        vector2 = EditorGUI.Vector2Field(position, label, vector2);
        if (EditorGUI.EndChangeCheck())
        {
            xProp.floatValue = vector2.x;
            yProp.floatValue = vector2.y;
        }       
    }
}
#endif
}