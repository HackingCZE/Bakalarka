using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NavigationManager;

public class Tester : MonoBehaviour
{
    [SerializeField] List<NavigationAlgorithm> algorithms = new List<NavigationAlgorithm>();
    [Button]
    public async void StartTest()
    {
        for (int i = 0; i < 100; i++)
        {
            SimpleVisualizer.Instance.Create();
            var _algorithmStats = await NavigationManager.Instance.GetOrderOfAlgorithms();
            algorithms.Add(_algorithmStats[0].Algorithm);
        }
    }
}
