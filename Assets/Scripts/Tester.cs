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
        CSVGenerator csv = new CSVGenerator("Assets/Output.csv");

        for (int i = 0; i < 250; i++)
        {

            LSystemVisualizer.Instance.VisualizeMap();
            var _algorithmStats = await NavigationManager.Instance.GetOrderOfAlgorithms();
            if(_algorithmStats[0].ResultPathLength == 1)
            {
                LSystemVisualizer.Instance.SpawnTiles();

                return;
            }
            algorithms.Add(_algorithmStats[0].Algorithm); 
            csv.AddRow(new string[] {
                    "Test("+i+")",
                    "NodesCount: "+_algorithmStats[0].NodesCount.ToString(),
                    "ChanceToJoin: "+Mathf.Round(LSystemVisualizer.Instance.GetCurrentChanceToJoin),
                    "IsPlanar: " + NavigationManager.Instance.CheckPlanar().ToString(),
                    "DistanceBetweenPoints: " + Mathf.Round(NavigationManager.Instance.DistanceBetweenPoints)
                });

            csv.AddRow("Algorithm", "Time", "VisitedNodes", "MemoryUsage", "ResultPathLength");
            foreach (var item in _algorithmStats)
            {
                csv.AddRow(new string[] {
                    item.Algorithm.ToString(),
                    item.Time.ToString(),
                    item.VisitedNodes.ToString(),
                    item.MemoryUsage.ToString(),
                    item.ResultPathLength.ToString()
                });
                //Debug.Log("--------------");
                //Debug.Log("Algorithm: " + item.Algorithm.ToString());
                //Debug.Log("Nodes: " + item.VisitedNodes.ToString());
                //Debug.Log("Length: " + item.ResultPathLength.ToString());
                //Debug.Log("Memory: " + item.MemoryUsage.ToString());
                //Debug.Log("Time: " + item.Time.ToString());
                //Debug.Log("--------------");

            }

            csv.AddRow(new string[] {
                    "",
                    "",
                    "",
                    "",
                    ""
                });

        }

        csv.SaveToFile();
    }

    [Header("Grid Settings")]
    public int gridSizeX = 10; // Poèet bunìk na ose X
    public int gridSizeZ = 10; // Poèet bunìk na ose Z
    public float cellSize = 1f; // Velikost jedné buòky

    [Header("Grid Appearance")]
    public Color gridColor = Color.green; // Barva møížky

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;

        // Vykreslení èar na ose X
        for (int x = 0; x <= gridSizeX; x++)
        {
            Vector3 start = new Vector3(x * cellSize, 0, 0);
            Vector3 end = new Vector3(x * cellSize, 0, gridSizeZ * cellSize);
            Gizmos.DrawLine(transform.position + start, transform.position + end);
        }

        // Vykreslení èar na ose Z
        for (int z = 0; z <= gridSizeZ; z++)
        {
            Vector3 start = new Vector3(0, 0, z * cellSize);
            Vector3 end = new Vector3(gridSizeX * cellSize, 0, z * cellSize);
            Gizmos.DrawLine(transform.position + start, transform.position + end);
        }
    }
}
