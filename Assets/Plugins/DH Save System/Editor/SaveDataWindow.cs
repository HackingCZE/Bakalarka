using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static DH.Save.SaveSystem;

namespace DH.Save.Editor
{
#if UNITY_EDITOR
    public class SaveDataWindow : EditorWindow, IHasCustomMenu
    {
        private Dictionary<string, bool> _foldoutsGlobal = new Dictionary<string, bool>();
        private Dictionary<string, bool> _foldoutsSlot = new Dictionary<string, bool>();
        private Vector2 _scrollPositionSlot;
        private Vector2 _scrollPositionGlobal;
        public static SaveDataWindow Window;
        public static bool IsGlobal = true;
        public string Search;

        private bool _slotCurrent;
        private bool _lastSlotCurrent;
        private int _selectedIndex = 0;
        private int _lastSelectedIndex = -1;
        private List<string> _dynamicOptions = new List<string>();

        public void LoadData()
        {
            SaveSystemManager.Instance.Load();
        }

        public void SaveData()
        {
           SaveSystem.SaveData();
        }
        [MenuItem("Tools/Save Data")]
        public static void OpenWindow()
        {
            CreateSaveSystemManagerObject();
            SaveDataWindow.ShowWindow(true);
        }
        public static void ShowWindow(bool isGlobal)
        {
            SaveDataWindow.IsGlobal = isGlobal;
            Window = (SaveDataWindow)EditorWindow.GetWindow(typeof(SaveDataWindow), false, "Save Data");
            Window.minSize = new Vector2(400, 600);

            Window.Search = "";

            Window.Show();
        }

        private static void CreateSaveSystemManagerObject()
        {
            SaveSystemManager manager = UnityEngine.Object.FindObjectOfType<SaveSystemManager>();

            if (manager == null)
            {
                GameObject managerObject = new GameObject("SaveSystemManager");
                manager = managerObject.AddComponent<SaveSystemManager>();
                try
                {
                    GameObject.DontDestroyOnLoad(managerObject);
                }
                catch { }
                manager.UpdateSettings(SaveSystemSettingsEditor.GetOrCreateSettings());
                EditorUtility.SetDirty(managerObject);

                Debug.Log("SaveSystemManager was not found and has been created.");
            }
            if (SaveSystemSettingsEditor.GetOrCreateSettings().Key == "")
            {
                Undo.RecordObject(SaveSystemSettingsEditor.GetOrCreateSettings(), "Modify SaveSystemSettings");
                SaveSystemSettingsEditor.GetOrCreateSettings().GenerateEncryptionKey();
                EditorUtility.SetDirty(SaveSystemSettingsEditor.GetOrCreateSettings());
            }
                EditorUtility.SetDirty(SaveSystemSettingsEditor.GetOrCreateSettings());
            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            if (SaveSystemManager.Instance == null) CreateSaveSystemManagerObject();
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(IsGlobal, "GLOBAL", EditorStyles.toolbarButton))
            {
                IsGlobal = true;
            }

            if (GUILayout.Toggle(!IsGlobal, "SLOT", EditorStyles.toolbarButton))
            {
                IsGlobal = false;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUILayout.Label("Search");
            Search = GUILayout.TextField(Search);

            if (IsGlobal)
            {
                SaveSystemEditor.DrawTabLabel("GLOBAL DATA", false, true);
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
                _scrollPositionGlobal = EditorGUILayout.BeginScrollView(_scrollPositionGlobal, GUILayout.ExpandHeight(true));
                SaveSystemEditor.DrawRectData(SaveSystem.GetGlobalData, _foldoutsGlobal, Search, true);
            }
            else
            {
                Dictionary<string, SaveDataEntry> slotData = SaveSystem.GetSlotData;

                SaveSystemEditor.DrawTabLabel("SLOT DATA", false, false);

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();

                GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
                centeredStyle.alignment = TextAnchor.MiddleCenter;
                centeredStyle.wordWrap = true;

                _slotCurrent = EditorGUILayout.Toggle("Current Slot", _slotCurrent);
                if (!_slotCurrent)
                {
                    if (SaveSystem.GetSaveDataSlots.Count > 0)
                    {
                        _selectedIndex = EditorGUILayout.Popup("Select Slot", _selectedIndex, _dynamicOptions.ToArray());
                        if (_selectedIndex == SaveSystem.GetSaveDataSlots.Count)
                        {
                            _selectedIndex = 0;
                        }

                        if (_dynamicOptions.Count != SaveSystem.GetSaveDataSlots.Count || SaveSystem.IsEditedSlots)
                        {
                            SaveSystem.IsEditedSlots = false;
                            _dynamicOptions.Clear();
                            foreach (var item in SaveSystem.GetSaveDataSlots)
                            {
                                _dynamicOptions.Add(item.SlotName);
                            }
                        }

                        if (SaveSystem.GetSaveDataSlots[_selectedIndex] == SaveSystem.GetCurrentSaveSlot)
                        {
                            slotData = SaveSystem.GetSlotData;
                        }
                        else
                        {
                            slotData = LoadSlot(slotData, SaveSystem.GetSaveDataSlots[_selectedIndex].SavePath);
                        }


                        if (_selectedIndex != _lastSelectedIndex)
                        {
                            _foldoutsSlot = new Dictionary<string, bool>();
                        }

                        _lastSelectedIndex = _selectedIndex;
                    }
                    else
                    {
                        EditorGUILayout.LabelField("You need to load some data (try loading the data again).", centeredStyle, GUILayout.ExpandWidth(true));
                    }
                }
                else
                {
                    if (SaveSystem.GetCurrentSaveSlot != null)
                    {
                        EditorGUILayout.LabelField(SaveSystem.GetCurrentSaveSlot.SlotName, centeredStyle, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("You must be in game mode and have a slot loaded.", centeredStyle, GUILayout.ExpandWidth(true));
                    }
                    _selectedIndex = 0;
                    _lastSelectedIndex = -1;
                }

                if (_lastSlotCurrent != _slotCurrent && _slotCurrent)
                {
                    _foldoutsSlot = new Dictionary<string, bool>();
                }

                _lastSlotCurrent = _slotCurrent;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
                _scrollPositionSlot = EditorGUILayout.BeginScrollView(_scrollPositionSlot, GUILayout.ExpandHeight(true));
                SaveSystemEditor.DrawRectData(slotData, _foldoutsSlot, Search == null ? "" : Search);
            }
        }


        public Dictionary<string, SaveDataEntry> LoadSlot(Dictionary<string, SaveDataEntry> slotData, string savePath)
        {
            slotData = new Dictionary<string, SaveDataEntry>();

            string jsonData = File.ReadAllText(Application.persistentDataPath + savePath);
            SerializableSaveDataList deserializedList = SaveSystem.Deserialize<SerializableSaveDataList>(jsonData);

            foreach (var item in deserializedList.items)
            {
                slotData.Add(item.Id, item);
            }
            return slotData;
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            GUIContent loadData = new GUIContent("Load Data");
            menu.AddItem(loadData, false, LoadData);
            GUIContent saveData = new GUIContent("Save Data");
            menu.AddItem(saveData, false, SaveData);
        }
    }
#endif
}