using UnityEngine;
using UnityEngine.UI;

namespace DH.Save.Demo
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] GameObject createSlot;
        [SerializeField] GameObject removeSlot;
        [SerializeField] Image parent;

        public void ShowCreateSlot()
        {
            parent.enabled = true;
            createSlot.SetActive(true);
        }
        public void ShowRemoveSlot()
        {
            parent.enabled = true;
            removeSlot.SetActive(true);
        }
        public void Close()
        {
            parent.enabled = false;
            removeSlot.SetActive(false);
            createSlot.SetActive(false);
        }
    }
}
