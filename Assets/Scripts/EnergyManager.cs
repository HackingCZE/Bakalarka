using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyManager : MonoBehaviour
{
    [SerializeField] List<Image> energySlot = new();

    private void Start()
    {
        MainGameManager.Instance.OnEnergyChange += UpdateEnergy;
    }

    private void OnDisable()
    {
        MainGameManager.Instance.OnEnergyChange -= UpdateEnergy;
    }

    private void UpdateEnergy(int currentEnergy)
    {
        for(int i = 0; i < energySlot.Count; i++)
        {
            if(i < currentEnergy) energySlot[i].color = new Color32(255, 255, 255, 255);
            else energySlot[i].color = new Color32(100, 100, 100, 255);
        }
    }
}
