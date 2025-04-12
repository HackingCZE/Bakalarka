using UnityEngine;
using UnityEngine.UI;

public class InputMin : MonoBehaviour
{
    [SerializeField] Button btn;
    [SerializeField] int min;
    [SerializeField] int max;
    public void FilterMinValue(string stringValue)
    {
        btn.interactable = (stringValue.Length >= min && stringValue.Length <= max);
    }
}
