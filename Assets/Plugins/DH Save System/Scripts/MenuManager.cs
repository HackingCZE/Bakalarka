using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Save.Demo
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance { get; private set; }
        [SerializeField] PopupManager popupParent;
        [SerializeField] GameObject slotsParent;
        [SerializeField] GameObject savingObject;
        [SerializeField] GameObject saveBtn;
        [SerializeField] List<SaveSlotUI> slots;
        [SerializeField] InputField slotName;
        [SerializeField] GameObject playModeButtonsParent;
        int slotIndex = -1;
        SaveSlotUI saveSlotUI = null;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            SaveSystemManager.OnAutoSaveStateChange += HandleAutoSaveStateChange;
        }

        private void OnDisable()
        {
            SaveSystemManager.OnAutoSaveStateChange -= HandleAutoSaveStateChange;
        }

        private void HandleAutoSaveStateChange(bool isAutoSaving)
        {
            if (isAutoSaving)
            {
                savingObject.SetActive(true);
            }
            else
            {
                savingObject.SetActive(false);
            }
        }

        public void Initialize()
        {
            foreach (var item in slots)
            {
                item.Setup(null);
            }
            foreach (var item in SaveSystem.GetSaveDataSlots)
            {
                slots[item.IndexSlot].Setup(item);
            }
        }

        public void SaveButton()
        {
            SaveSystem.SaveData();
        }

        public void PlayButton()
        {
            slotsParent.SetActive(true);
        }

        public void CloseSlots()
        {
            slotsParent.SetActive(false);
        }

        public void ClosePopup()
        {
            popupParent.Close();
        }

        public void LoadSlot(string slotName)
        {
            SaveSystem.LoadSaveSlot(slotName);
            slotsParent.SetActive(false);
            saveBtn.SetActive(false);
            playModeButtonsParent.SetActive(true);
        }

        public void ShowPopupCreateSlot(int slotIndex)
        {
            this.slotIndex = slotIndex;
            popupParent.ShowCreateSlot();
        }

        public void ShowPopupRemoveSlot(int slotIndex, SaveSlotUI saveSlotUI)
        {
            this.slotIndex = slotIndex;
            this.saveSlotUI = saveSlotUI;
            popupParent.ShowRemoveSlot();
        }

        public void CreateNewSlot()
        {
            if (slotName.text != "")
            {
                SaveSystem.CreateNewSaveSlot(slotName.text, slotIndex);
                ClosePopup();
                Initialize();
            }
        }

        public void RemoveSlot()
        {
            if (SaveSystem.RemoveSaveSlot(this.saveSlotUI.saveDataSlot.SlotName, this.slotIndex))
            {
                ClosePopup();
                Initialize();
            }

        }
    }
}