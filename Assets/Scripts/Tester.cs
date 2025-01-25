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

    public GameObject objectToSpawn;

    [ContextMenu("spawn")]
    public void Spawn()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                // Spoèítáme pozici pro spawn
                Vector3 spawnPosition = new Vector3(x * cellSize, 0, z * cellSize);

                // Spawnuji objekt na dané pozici
                Instantiate(objectToSpawn, transform.position + spawnPosition, Quaternion.identity, transform);
            }
        }
    }

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

    public List<Vector3> points; // Seznam bodù cesty
    public float radius = 0.1f; // Polomìr trubice
    public int segments = 8; // Poèet segmentù pro kruhovou základnu


    [ContextMenu("GenrateTubeMesh")]
    void GenerateTube()
    {
        Mesh tubeMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Procházení všech bodù
        for (int i = 0; i < points.Count; i++)
        {
            // Smìr mezi body
            Vector3 forward = Vector3.forward;
            if (i < points.Count - 1)
            {
                forward = (points[i + 1] - points[i]).normalized;
            }

            // Rotace pro kruh kolem osy
            Quaternion rotation = Quaternion.LookRotation(forward);

            // Pøidání vertexù pro aktuální bod
            for (int j = 0; j < segments; j++)
            {
                float angle = 2 * Mathf.PI * j / segments;
                Vector3 offset = rotation * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                vertices.Add(points[i] + offset);

                uvs.Add(new Vector2(j / (float)segments, i / (float)points.Count));
            }
        }

        // Vytvoøení trojúhelníkù mezi kruhy
        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                int current = i * segments + j;
                int next = i * segments + (j + 1) % segments;
                int nextRow = (i + 1) * segments + j;
                int nextRowNext = (i + 1) * segments + (j + 1) % segments;

                // První trojúhelník
                triangles.Add(current);
                triangles.Add(nextRow);
                triangles.Add(next);

                // Druhý trojúhelník
                triangles.Add(next);
                triangles.Add(nextRow);
                triangles.Add(nextRowNext);
            }
        }

        // Nastavení meshe
        tubeMesh.vertices = vertices.ToArray();
        tubeMesh.triangles = triangles.ToArray();
        tubeMesh.uv = uvs.ToArray();
        tubeMesh.RecalculateNormals();
        tubeMesh.RecalculateBounds();

        // Pøipojení mesh k MeshFilter a MeshCollider
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = tubeMesh;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = tubeMesh;
        meshCollider.convex = false; // Pokud je potøeba, nastav convex na true
    }
}
