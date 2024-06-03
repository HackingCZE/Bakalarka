using UnityEngine;
using UnityEditor;
using DH.Save.SerializableTypes;


namespace DH.Save.Editor.Drawers
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializableVector3))]
    public class SerializableVector3Drawer : PropertyDrawer
    {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty xProp = property.FindPropertyRelative("x");
        SerializedProperty yProp = property.FindPropertyRelative("y");
        SerializedProperty zProp = property.FindPropertyRelative("z");

        Vector3 vector3 = new Vector3(xProp.floatValue, yProp.floatValue, zProp.floatValue);

        EditorGUI.BeginChangeCheck();
        vector3 = EditorGUI.Vector3Field(position, label, vector3);
        if (EditorGUI.EndChangeCheck())
        {
            xProp.floatValue = vector3.x;
            yProp.floatValue = vector3.y;
            zProp.floatValue = vector3.z;
        }
    }
}
#endif
}
