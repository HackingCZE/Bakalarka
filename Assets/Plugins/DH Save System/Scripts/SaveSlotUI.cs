using UnityEngine;
using UnityEngine.UI;
using static DH.Save.SaveSystem;

namespace DH.Save.Demo
{
    public class SaveSlotUI : MonoBehaviour
    {
        [SerializeField] Text slotName;
        [SerializeField] Text lastSave;
        [SerializeField] Text progress;
        [SerializeField] GameObject SlotObjects;
        [SerializeField] GameObject PlusIcon;
        bool canLoad = false;
        public SaveDataSlot saveDataSlot { get; private set; }

        private void Start()
        {
            if (!canLoad) SlotObjects.SetActive(false);
        }

        public void Setup(SaveDataSlot saveDataSlot)
        {
            this.saveDataSlot = saveDataSlot;
            if (saveDataSlot == null)
            {
                canLoad = false;
                SlotObjects.SetActive(false);
                PlusIcon.SetActive(true);
                return;
            }
            canLoad = true;
            SlotObjects.SetActive(true);
            PlusIcon.SetActive(false);
            this.slotName.text = saveDataSlot.SlotName;
            this.lastSave.text = saveDataSlot.LastSave.ToString();
            this.progress.text = saveDataSlot.Progress + "%";
        }

        public void LoadCreateButton(int slotIndex)
        {
            if (canLoad) MenuManager.Instance.LoadSlot(slotName.text);
            else MenuManager.Instance.ShowPopupCreateSlot(slotIndex);
        }

        public void RemoveSlotButton(int slotIndex)
        {
            MenuManager.Instance.ShowPopupRemoveSlot(slotIndex, this);
        }
    }
}
