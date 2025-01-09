using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static NavigationManager;
using static SpreadAlgorithms;

public class SpreadAlgorithms : MonoBehaviour
{
    // Prefab pro algoritmy, kter� chceme rozm�stit
    public List<Spread> spreads = new List<Spread>();
    public Material material;
    private MaterialPropertyBlock materialBlock;

    private List<Coroutine> _coroutines;

    public List<ColorAlgorithm> _algorithms;
    public float speed = 2f;

    // M���tko, kter� ur�uje vzd�lenost mezi algoritmy
    public float scale = 1f;
    public float width = .2f;

    // Metoda pro rozm�st�n� algoritm� podle seznamu hodnot
    public void SpreadOnAxis(List<AlgorithmStats> values)
    {

        _coroutines = new();
        materialBlock = new MaterialPropertyBlock();
        // Zru��me p�edchoz� instancov�n� algoritm� (pokud existuj�)
        foreach (var stat in values)
        {
            var parentParent = new GameObject(stat.Algorithm.ToString());
            parentParent.transform.position = Vector3.zero;
            var parent = new GameObject("Parent");
            parent.transform.position = new Vector3(stat.Path[0].x, stat.Path[0].y + 1, stat.Path[0].z);
            parent.transform.SetParent(parentParent.transform);
            var gm = new GameObject(stat.Algorithm.ToString());
            gm.transform.SetParent(parent.transform);
            gm.AddComponent<TrailRenderer>();
            gm.transform.position = Vector3.zero;

            var trailRenderer = gm.GetComponent<TrailRenderer>();
            trailRenderer.material = material;
            trailRenderer.material.color = _algorithms.Find(e => e.algorithm == stat.Algorithm).color;
            trailRenderer.time = float.PositiveInfinity;
            trailRenderer.startWidth = width;
            trailRenderer.endWidth = width;
            trailRenderer.numCapVertices = 90;
            trailRenderer.shadowBias = 0;
            trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spreads.Add(new Spread(trailRenderer, stat));
        }

        int count = spreads.Count;

        float centerX = (count - 1) * scale / 2f;

        // Rozm�st�me algoritmy podle hodnot v seznamu
        for (int i = 0; i < count; i++)
        {

            float xPos = (i * scale) - centerX + spreads[i].TrailRenderer.transform.position.x;
            float zPos = (i * scale) - centerX + spreads[i].TrailRenderer.transform.position.z;
            float yPos = (i * (scale / 4.5f)) + spreads[i].TrailRenderer.transform.position.y;

            // Nastaven� pozice objektu
            spreads[i].TrailRenderer.transform.localPosition = new Vector3(xPos, yPos, zPos);
            spreads[i].AddPointTube(spreads[i].TrailRenderer.transform.position);
        }
        foreach (var item in spreads)
        {
            _coroutines.Add(StartCoroutine(MoveSpread(item)));
        }

        GetComponent<TubeMouseDetector>().SetTubes(spreads, width);
    }

    public void Clear()
    {
        foreach (var item in _coroutines)
        {
            if (item != null) StopCoroutine(item);
        }
        _coroutines.Clear();

        foreach (var item in spreads)
        {
            Destroy(item.TrailRenderer.transform.parent.parent.gameObject);
        }

        spreads.Clear();
    }

    public IEnumerator MoveSpread(Spread item)
    {
        foreach (var targetPosition in item.AlgorithmStats.Path)
        {
            yield return StartCoroutine(MoveToPosition(item.TrailRenderer.transform.parent.gameObject, new Vector3(targetPosition.x, targetPosition.y + 1, targetPosition.z)));
            item.AddPointTube(item.TrailRenderer.transform.position);
        }
    }

    private IEnumerator MoveToPosition(GameObject obj, Vector3 targetPosition)
    {
        while (obj != null && obj.transform.position != targetPosition)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition, speed * Time.deltaTime);

            // �ek�me na dal�� framex
            yield return null;
        }

        if (obj != null) obj.transform.position = targetPosition;
    }

    [Serializable]
    public class Spread
    {
        [field: SerializeField] public TrailRenderer TrailRenderer { get; set; }
        [field: SerializeField] public AlgorithmStats AlgorithmStats { get; set; }
        public MeshCollider Collider { get; set; } 


        private float radius = 0.08f;
        private float lastRadius;
        private int segments = 8;
        private List<Vector3> points = new List<Vector3>();

        private Mesh tubeMesh;
        private MeshFilter filter;
        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();

        public Spread(TrailRenderer trailRenderer, AlgorithmStats algorithmStats)
        {
            Collider = trailRenderer.transform.parent.parent.AddComponent<MeshCollider>();
            filter = trailRenderer.transform.parent.parent.AddComponent<MeshFilter>();
            tubeMesh = new Mesh();
            filter.mesh = tubeMesh;
            trailRenderer.transform.parent.parent.tag = "Tube";
            TrailRenderer = trailRenderer;
            AlgorithmStats = algorithmStats;
        }

        public void AddPointTube(Vector3 newPoint)
        {
            // P�id�n� nov�ho bodu do seznamu
            points.Add(newPoint);

            // Pokud m�me alespo� dva body, vytvo� trubici
            if (points.Count > 1)
            {
                GenerateTube(radius);
            }
        }

        public float GetLastRadius => lastRadius;

        public void GenerateTube(float radius)
        {
            lastRadius = radius;
            // Vy�i�t�n� p�edchoz�ch dat
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();

            // Proch�zen� v�ech bod�
            for (int i = 0; i < points.Count; i++)
            {
                // Sm�r mezi body
                Vector3 forward = Vector3.forward;
                if (i < points.Count - 1)
                {
                    forward = (points[i + 1] - points[i]).normalized;
                }

                // Rotace pro kruh kolem osy
                Quaternion rotation = forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward);

                // P�id�n� vertex� pro aktu�ln� bod
                for (int j = 0; j < segments; j++)
                {
                    float angle = 2 * Mathf.PI * j / segments;
                    Vector3 offset = rotation * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                    vertices.Add(points[i] + offset);

                    uvs.Add(new Vector2(j / (float)segments, i / (float)points.Count));
                }
            }

            // Vytvo�en� troj�heln�k� mezi kruhy
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int current = i * segments + j;
                    int next = i * segments + (j + 1) % segments;
                    int nextRow = (i + 1) * segments + j;
                    int nextRowNext = (i + 1) * segments + (j + 1) % segments;

                    // Prvn� troj�heln�k
                    triangles.Add(current);
                    triangles.Add(nextRow);
                    triangles.Add(next);

                    // Druh� troj�heln�k
                    triangles.Add(next);
                    triangles.Add(nextRow);
                    triangles.Add(nextRowNext);
                }
            }

            if (points.Count < 5)
            {
                return;
            }

            // Nastaven� meshe
           // tubeMesh.Clear();
            tubeMesh.vertices = vertices.ToArray();
            tubeMesh.triangles = triangles.ToArray();
            tubeMesh.uv = uvs.ToArray();
            tubeMesh.RecalculateNormals();
            tubeMesh.RecalculateBounds();

            filter.mesh = tubeMesh;

            // Aktualizace MeshCollideru
            MeshCollider meshCollider = Collider;
            meshCollider.sharedMesh = tubeMesh;
        }
    }

    [Serializable]
    public class ColorAlgorithm
    {
        public NavigationAlgorithm algorithm;
        public Color color;
    }
}
