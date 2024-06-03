using DH.Save;
using DH.Save.Attributes;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace DH.Save.Editor.Drawers
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LockAttribute))]
    public class LockPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();
            float _height = 30;
            position.width = position.width - _height;

            LockAttribute lockAttr = (LockAttribute)attribute;
            GUI.enabled = lockAttr.locked;
            EditorGUI.PropertyField(position, property, new GUIContent(label));
            GUI.enabled = true;


            GUIContent lockedIconContent = EditorGUIUtility.IconContent("LockIcon-On");
            GUIContent unlockedIconContent = EditorGUIUtility.IconContent("LockIcon");

            var buttonRect = new Rect(position.xMax, position.y, _height, position.height);
            if (GUI.Button(buttonRect, !lockAttr.locked ? lockedIconContent : unlockedIconContent))
            {
                if (!lockAttr.locked) Undo.RecordObject(property.serializedObject.targetObject, "Unlock Field: " + label);
                else Undo.RecordObject(property.serializedObject.targetObject, "Lock Field: " + label);
                lockAttr.locked = !lockAttr.locked;
            }
            EditorGUILayout.EndHorizontal();
        }

    }
#endif
}

