using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static NavigationManager;
using static SpreadAlgorithms;

public class SpreadAlgorithms : MonoBehaviour
{
    // Prefab pro algoritmy, které chceme rozmístit
    public List<Spread> spreads = new List<Spread>();
    public Material material;
    private MaterialPropertyBlock materialBlock;

    private List<Coroutine> _coroutines = new();

    public List<ColorAlgorithm> _algorithms;
    public float speed = 2f;

    // Mìøítko, které urèuje vzdálenost mezi algoritmy
    public float scale = 1f;
    public float width = .2f;

    public void SpreadOnAxis(List<AlgorithmStats> values)
    {

        _coroutines.Clear();
        materialBlock = new MaterialPropertyBlock();
        // Zrušíme pøedchozí instancování algoritmù (pokud existují)
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

        // Rozmístíme algoritmy podle hodnot v seznamu
        for (int i = 0; i < count; i++)
        {

            float xPos = (i * scale) - centerX + spreads[i].TrailRenderer.transform.position.x;
            float zPos = (i * scale) - centerX + spreads[i].TrailRenderer.transform.position.z;
            float yPos = (i * (scale / 4.5f)) + spreads[i].TrailRenderer.transform.position.y;

            // Nastavení pozice objektu
            spreads[i].TrailRenderer.transform.localPosition = new Vector3(xPos, yPos, zPos);
            spreads[i].AddPointTube(spreads[i].TrailRenderer.transform.position);
        }
        foreach (var item in spreads)
        {
            _coroutines.Add(StartCoroutine(MoveSpread(item)));
        }

        GetComponent<TubeMouseDetector>().SetTubes(spreads, width);
    }

    public void MainMenuPaths()
    {
        spreads = MenuPathMove.Instance.paths;
        _coroutines.Clear();

        for (int i = 0; i < spreads.Count; i++)
        {
            spreads[i].TrailRenderer.emitting = false;
            spreads[i].TrailRenderer.time = spreads[i].AlgorithmStats.Path.Count / 8;
            spreads[i] = new Spread(MenuPathMove.Instance.trails[i], spreads[i].AlgorithmStats);
            spreads[i].TrailRenderer.transform.parent.position = new Vector3(spreads[i].AlgorithmStats.Path[0].x, spreads[i].AlgorithmStats.Path[0].y + 1 - 2, spreads[i].AlgorithmStats.Path[0].z);
            spreads[i].TrailRenderer.material = material;
            spreads[i].TrailRenderer.material.color = _algorithms.Find(e => e.algorithm == spreads[i].AlgorithmStats.Algorithm).color;
            spreads[i].TrailRenderer.startWidth = width * 2f;
            spreads[i].TrailRenderer.endWidth = width * 2f;

        }

        int count = spreads.Count;

        float scale = .08f;

        float centerX = (count - 1) * scale / 2f;

        // Rozmístíme algoritmy podle hodnot v seznamu
        for (int i = 0; i < count; i++)
        {

            float xPos = (i * scale) - centerX + spreads[i].TrailRenderer.transform.position.x;
            float zPos = (i * scale) - centerX + spreads[i].TrailRenderer.transform.position.z;
            float yPos = (i * (scale / 4.5f)) + spreads[i].TrailRenderer.transform.position.y;

            // Nastavení pozice objektu
            spreads[i].TrailRenderer.transform.position = new Vector3(xPos, yPos, zPos);
            spreads[i].StartPos = spreads[i].TrailRenderer.transform.position;
            spreads[i].CreateMesh = false;

        }
        foreach (var item in spreads)
        {
            _coroutines.Add(StartCoroutine(TrackCoroutine(MoveSpread(item), item)));
        }
    }

    IEnumerator TrackCoroutine(IEnumerator coroutine, Spread item)
    {
        item.TrailRenderer.emitting = false;
        yield return new WaitForEndOfFrame();
        item.TrailRenderer.transform.parent.position = new Vector3(item.AlgorithmStats.Path[0].x, item.TrailRenderer.transform.parent.position.y, item.AlgorithmStats.Path[0].z);
        item.TrailRenderer.transform.parent.position += new Vector3(0, 2, 0);
        yield return new WaitForEndOfFrame();
        item.TrailRenderer.emitting = true;

        yield return StartCoroutine(coroutine);
        item.TrailRenderer.emitting = false;
        item.TrailRenderer.transform.parent.position -= new Vector3(0, 2, 0);

        if (SceneManager.GetActiveScene().name == "Menu") StartCoroutine(TrackCoroutine(MoveSpread(item), item));
    }

    private void OnDrawGizmos()
    {
        foreach (var item in spreads)
        {
            foreach (var point in item.AlgorithmStats.Path)
            {
                Gizmos.DrawSphere(point, .5f);
            }
        }
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
            if(item.TrailRenderer != null) item.AddPointTube(item.TrailRenderer.transform.position);
        }

    }

    private IEnumerator MoveToPosition(GameObject obj, Vector3 targetPosition)
    {

        while (obj != null && obj.transform.position != targetPosition)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition, speed * Time.deltaTime);

            // Èekáme na další framex
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

        public Vector3 StartPos { get; set; }
        public bool CreateMesh { get; set; }

        private float radius = 0.05f;
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
            if (!CreateMesh) return;
            // Pøidání nového bodu do seznamu
            points.Add(newPoint);

            // Pokud máme alespoò dva body, vytvoø trubici
            if (points.Count > 1)
            {
                GenerateTube(radius);
            }
        }

        public override string ToString()
        {
            return base.ToString() + " " + TrailRenderer.ToString();
        }

        public float GetLastRadius => lastRadius;

        public void GenerateTube(float radius)
        {

            lastRadius = radius;
            // Vyèištìní pøedchozích dat
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();

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
                Quaternion rotation = forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward);

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

            if (points.Count < 5)
            {
                return;
            }

            // Nastavení meshe
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
