using UnityEditor;
using UnityEngine;

namespace DH.Save.Editor
{
    [CustomEditor(typeof(SaveSystemSettings))]
    public class SaveSystemSettingsEditor : UnityEditor.Editor
    {
#if UNITY_EDITOR
        public static SaveSystemSettings GetOrCreateSettings()
        {
            var settings = SaveSystemSettings.LoadSettings();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<SaveSystemSettings>();
                string properPath = System.IO.Path.Combine("Assets/Plugins/DH Save System/Resources", "SaveSystemSettings.asset");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(properPath));
                AssetDatabase.CreateAsset(settings, properPath);
                AssetDatabase.SaveAssets();
                Debug.Log("DH Save System Settings asset created at: " + properPath);
            }
            if (settings.NotSaveIcon == null)
            {
                settings.NotSaveIcon = Resources.Load<Texture2D>("unsaved_icon");
            }
            return settings;
        }

        /// <summary>
        /// Updates the path where SaveSystemSettings are stored.
        /// </summary>
        /// <param name="newPath">The new path for storing settings.</param>
        public static void SetNewSettingPath(string newPath)
        {
            EditorPrefs.SetString("SETTING_PATH_DH_SAVE_SYSTEM", newPath);
        }

        /// <summary>
        /// Updates the path where SaveSystemSettings are stored.
        /// </summary>
        /// <param name="newPath">The new path for storing settings.</param>
        public static SerializedObject GetSerializedObject()
        {
            return new SerializedObject(SaveSystemSettingsEditor.GetOrCreateSettings());
        }

        public static float currentWidth = 0f;
        public override void OnInspectorGUI()
        {
            DrawGUI(false);
        }

        public static void DrawGUI(bool value)
        {
            SerializedObject serializedObject = new SerializedObject(SaveSystemSettingsEditor.GetOrCreateSettings());
            serializedObject.Update();


            currentWidth = EditorGUILayout.BeginVertical().width;
            if (!value) currentWidth = 0;
            DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", "AutoSaveGlobalData", "AutoSaveSlotData", "AutoSaveSlotDataTime" });
            SaveSystemEditor.DrawButtonGenerateEncryptKey(30);
            SaveSystemEditor.DrawAutoSaveSection(serializedObject, currentWidth);

            EditorGUILayout.EndVertical();
            currentWidth = 0f;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

        }


#endif
    }

}