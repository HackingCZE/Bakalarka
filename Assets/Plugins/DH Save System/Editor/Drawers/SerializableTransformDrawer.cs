using UnityEngine;
using UnityEditor;
using DH.Save.SerializableTypes;

namespace DH.Save.Editor.Drawers
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializableTransform))]
    public class SerializableTransformDrawer : PropertyDrawer
    {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var height = EditorGUIUtility.singleLineHeight;
        position.height = height;

        var positionProperty = property.FindPropertyRelative("position");
        var rotationProperty = property.FindPropertyRelative("rotation");
        var scaleProperty = property.FindPropertyRelative("localScale");

        var rotationArray = new float[] { rotationProperty.GetArrayElementAtIndex(0).floatValue, rotationProperty.GetArrayElementAtIndex(1).floatValue, rotationProperty.GetArrayElementAtIndex(2).floatValue, rotationProperty.GetArrayElementAtIndex(3).floatValue };
        var positionArray = new float[] { positionProperty.GetArrayElementAtIndex(0).floatValue, positionProperty.GetArrayElementAtIndex(1).floatValue, positionProperty.GetArrayElementAtIndex(2).floatValue };
        var scaleArray = new float[] { scaleProperty.GetArrayElementAtIndex(0).floatValue, scaleProperty.GetArrayElementAtIndex(1).floatValue, scaleProperty.GetArrayElementAtIndex(2).floatValue };

        GUILayout.BeginVertical(EditorStyles.helpBox);


        EditorGUI.PrefixLabel(position, new GUIContent("Transform"));
        position.x += 5;
        position.width -= 10;
        position.y += 2.5f;
        position.y += height + EditorGUIUtility.standardVerticalSpacing;
        Vector3 newPosition = EditorGUI.Vector3Field(position, new GUIContent("Position"), ToVector3(positionArray));
        position.y += height + EditorGUIUtility.standardVerticalSpacing;

        Quaternion rotation = ToQuaternion(rotationArray);
        var euler = rotation.eulerAngles;
        euler = EditorGUI.Vector3Field(position, new GUIContent("Rotation"), euler);
        position.y += height + EditorGUIUtility.standardVerticalSpacing;

        Vector3 newScale = EditorGUI.Vector3Field(position, new GUIContent("Scale"), ToVector3(scaleArray));

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();

        var positionF = FromVector3(newPosition);
        positionProperty.GetArrayElementAtIndex(0).floatValue = positionF[0];
        positionProperty.GetArrayElementAtIndex(1).floatValue = positionF[1];
        positionProperty.GetArrayElementAtIndex(2).floatValue = positionF[2];

        var updatedQuaternion = Quaternion.Euler(euler);
        rotationProperty.GetArrayElementAtIndex(0).floatValue = updatedQuaternion.x;
        rotationProperty.GetArrayElementAtIndex(1).floatValue = updatedQuaternion.y;
        rotationProperty.GetArrayElementAtIndex(2).floatValue = updatedQuaternion.z;
        rotationProperty.GetArrayElementAtIndex(3).floatValue = updatedQuaternion.w;

        var scaleF = FromVector3(newScale);
        scaleProperty.GetArrayElementAtIndex(0).floatValue = scaleF[0];
        scaleProperty.GetArrayElementAtIndex(1).floatValue = scaleF[1];
        scaleProperty.GetArrayElementAtIndex(2).floatValue = scaleF[2];

        GUILayout.Space((height * 3) + 5);
        GUILayout.EndVertical();
    }

    private static Vector3 ToVector3(float[] array) => new Vector3(array[0], array[1], array[2]);
    private static Quaternion ToQuaternion(float[] array) => new Quaternion(array[0], array[1], array[2], array[3]);
    private static float[] FromVector3(Vector3 vec) => new float[] { vec.x, vec.y, vec.z };

}
#endif
}
