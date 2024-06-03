using UnityEngine;
using UnityEditor;
using DH.Save.SerializableTypes;

namespace DH.Save.Editor.Drawers
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializableQuaternion))]
    public class SerializableQuaternionDrawer : PropertyDrawer
    {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty xProp = property.FindPropertyRelative("x");
        SerializedProperty yProp = property.FindPropertyRelative("y");
        SerializedProperty zProp = property.FindPropertyRelative("z");
        SerializedProperty wProp = property.FindPropertyRelative("w");

        Quaternion quaternion = new Quaternion(xProp.floatValue, yProp.floatValue, zProp.floatValue, wProp.floatValue);

        Vector3 euler = quaternion.eulerAngles;

        euler = EditorGUI.Vector3Field(position, label, euler);

        Quaternion updatedQuaternion = Quaternion.Euler(euler);

        xProp.floatValue = updatedQuaternion.x;
        yProp.floatValue = updatedQuaternion.y;
        zProp.floatValue = updatedQuaternion.z;
        wProp.floatValue = updatedQuaternion.w;
    }

}
#endif
}