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
            foreach (var item in _algorithmStats)
            {
                Debug.Log("--------------");
                Debug.Log("Algorithm: " + item.Algorithm.ToString());
                Debug.Log("Nodes: " + item.VisitedNodes.ToString());
                Debug.Log("Length: " + item.ResultPathLength.ToString());
                Debug.Log("Memory: " + item.MemoryUsage.ToString());
                Debug.Log("Time: " + item.Time.ToString());
                Debug.Log("--------------");

            }

        }
    }
}
