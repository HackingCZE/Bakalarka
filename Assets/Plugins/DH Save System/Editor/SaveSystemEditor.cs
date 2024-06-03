using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DH.Save.Exceptions;
using static DH.Save.EditorUtils;
using System;
using DH.Save.Editor.Exceptions;

namespace DH.Save.Editor
{

    [CustomEditor(typeof(SaveSystemManager))]
    public class SaveSystemEditor : UnityEditor.Editor
    {
#if UNITY_EDITOR
        private Dictionary<string, bool> _foldoutsGlobal = new Dictionary<string, bool>();
        private Dictionary<string, bool> _foldoutsSlot = new Dictionary<string, bool>();
        private Vector2 _scrollPositionSlot;
        private Vector2 _scrollPositionGlobal;

        SaveSystemManager _saveSystemManager;

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            SerializedObject serializedObjectSetting = SaveSystemSettingsEditor.GetSerializedObject();

            SaveSystemSettingsEditor.DrawGUI(false);
            DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", "AutoSaveGlobalData", "AutoSaveSlotData", "AutoSaveSlotDataTime", "Search" });

            _saveSystemManager = (SaveSystemManager)target;

            var search = serializedObject.FindProperty("Search");

            EditorGUILayout.PropertyField(search, new GUIContent("Search"));

            serializedObject.ApplyModifiedProperties();


            DrawTabLabel("GLOBAL DATA", true, true);
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxHeight(250));
            _scrollPositionGlobal = EditorGUILayout.BeginScrollView(_scrollPositionGlobal);
            DrawRectData(SaveSystem.GetGlobalData, _foldoutsGlobal, _saveSystemManager.Search, true);


            DrawTabLabel("SLOT DATA", true, false);
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxHeight(250));
            _scrollPositionSlot = EditorGUILayout.BeginScrollView(_scrollPositionSlot);
            DrawRectData(SaveSystem.GetSlotData, _foldoutsSlot, _saveSystemManager.Search);
        }


        public static void DrawAutoSaveSection(SerializedObject serializedObject, float currentWidth = 0)
        {
            var autoSaveGlobalData = serializedObject.FindProperty("AutoSaveGlobalData");
            var autoSaveSlotData = serializedObject.FindProperty("AutoSaveSlotData");
            var autoSaveSlotDataTime = serializedObject.FindProperty("AutoSaveSlotDataTime");
            var settings = SaveSystemSettingsEditor.GetSerializedObject();

            EditorGUILayout.BeginVertical();
            if (currentWidth == 0) currentWidth = EditorGUIUtility.currentViewWidth;
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(currentWidth));
            EditorGUILayout.PropertyField(autoSaveGlobalData, new GUIContent("Auto Save Global Data"));

            EditorGUILayout.PropertyField(autoSaveSlotData, new GUIContent("Auto Save Slot Data"));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(20);

            GUI.enabled = autoSaveSlotData.boolValue;
            EditorGUILayout.PropertyField(autoSaveSlotDataTime, new GUIContent("Auto Save Slot Data Delay Time"));
            GUI.enabled = true;

            EditorGUILayout.Space(5);
        }

        public static void DrawButtonGenerateEncryptKey(float maxHeight)
        {
            EditorGUILayout.BeginHorizontal();

            SaveSystemSettings settings = (SaveSystemSettings)SaveSystemSettingsEditor.GetSerializedObject().targetObject;

            GUI.enabled = settings.LockKey;

            if (GUILayout.Button("Generate Encrypt KEY", GUILayout.MaxHeight(maxHeight), GUILayout.ExpandWidth(true)))
            {
                Undo.RecordObject(settings, "Generate a new Encrypt KEY");
                settings.GenerateEncryptionKey();
                EditorUtility.SetDirty(settings);
            }

            GUI.enabled = true;

            GUIContent lockedIconContent = EditorGUIUtility.IconContent("LockIcon-On");
            GUIContent unlockedIconContent = EditorGUIUtility.IconContent("LockIcon");

            if (GUILayout.Button(!settings.LockKey ? lockedIconContent : unlockedIconContent, GUILayout.MaxWidth(maxHeight), GUILayout.MaxHeight(maxHeight)))
            {
                if (!settings.LockKey) Undo.RecordObject(settings, "Unlock Generate Encrypt KEY BTN");
                else Undo.RecordObject(settings, "Lock Generate Encrypt KEY BTN");
                settings.LockKey = !settings.LockKey;
                EditorUtility.SetDirty(settings);
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawRectData(Dictionary<string, SaveDataEntry> data, Dictionary<string, bool> foldouts, string search, bool isGlobal = false)
        {
            if (data.Count == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
                centeredStyle.alignment = TextAnchor.MiddleCenter;
                centeredStyle.wordWrap = true;
                EditorGUILayout.LabelField("No data found \n(try to add some data or reload the data)", centeredStyle, GUILayout.ExpandWidth(true));

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Reload", GUILayout.Height(30), GUILayout.Width(150)))
                {
                    SaveSystemManager.Instance.Load();
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            if (isGlobal && SaveSystem.GetSaveDataSlots.Count > 0)
            {
                string iD_SLOTS = "SLOTS";

                if (CanDrawProperties(iD_SLOTS, foldouts, out FoldoutsDictionary foldoutsDictionary))
                {
                    DrawProperties(iD_SLOTS, SaveSystem.GetSaveDataSlots, foldoutsDictionary);
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }


            foreach (KeyValuePair<string, SaveDataEntry> entry in data)
            {
                if (entry.Key.Contains(search) || search == "")
                {
                    if (CanDrawProperties(entry.Key, foldouts, out FoldoutsDictionary foldoutsDictionary))
                    {
                        DrawProperties(entry.Key, SaveSystem.CallGenericMethod(entry.Value.SaveType, entry.Value.JsonData, entry.Value.SaveType != entry.Value.LoadType), foldoutsDictionary);
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }

            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private static bool CanDrawProperties(string key, Dictionary<string, bool> foldouts, out EditorUtils.FoldoutsDictionary foldoutsDictionary)
        {
            if (!foldouts.ContainsKey(key))
            {
                foldouts[key] = false;
            }

            foldouts[key] = EditorGUILayout.BeginFoldoutHeaderGroup(foldouts[key], key);
            foldoutsDictionary = new EditorUtils.FoldoutsDictionary(key, -100, new string[] { "-100" });
            if (EditorUtils.CheckOptionMenu(foldoutsDictionary))
            {
                foldouts[key] = false;
            }
            return foldouts[key];
        }

        private static void DrawProperties(string key, object deserializedObject, EditorUtils.FoldoutsDictionary foldoutsDictionary)
        {
            EditorGUI.indentLevel++;

            try
            {
                EditorUtils.DrawProperties(deserializedObject, key, foldoutsDictionary.GetName());
            }
            catch (InvalidSerializableTransformException exception)
            {
                EditorGUI.indentLevel--;
                EditorGUILayout.EndFoldoutHeaderGroup();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                throw exception;
            }
            catch (Exception e)
            {
                throw e;
            }

            EditorGUI.indentLevel--;
        }

        public static void DrawTabLabel(string text, bool withIcon, bool isGlobal)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            Rect rect = EditorGUILayout.GetControlRect();
            float height = 30;
            rect.height = height;
            rect.width = withIcon ? EditorGUIUtility.currentViewWidth - (height + 4) : EditorGUIUtility.currentViewWidth - 8;

            EditorGUI.DrawRect(rect, new Color32(88, 88, 88, 100));

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUI.LabelField(rect, text, centeredStyle);

            bool dataIsUnsaved = isGlobal ? SaveSystem.AreThereUnsavedGlobalData : SaveSystem.AreThereUnsavedSlotData;

            Texture2D saveIcon = Resources.Load<SaveSystemSettings>("SaveSystemSettings").NotSaveIcon;
            GUIContent saveIconContent = new GUIContent(saveIcon);

            Rect saveIconRect = new Rect(rect.x + 4, rect.y + (rect.height - height) / 2, height, height);
            var lastColor = GUI.color;
            GUI.color = new Color32(199, 52, 57, 255);
            if (dataIsUnsaved) EditorGUI.LabelField(saveIconRect, saveIconContent);
            GUI.color = lastColor;

            if (withIcon)
            {
                Rect buttonRect = new Rect(rect.xMax - 20, rect.y, height, height);
                GUIContent zoomIcon = EditorGUIUtility.IconContent("d_ViewToolZoom");

                if (GUI.Button(buttonRect, zoomIcon))
                {
                    SaveDataWindow.ShowWindow(isGlobal);
                }

            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

#endif
    }

}



