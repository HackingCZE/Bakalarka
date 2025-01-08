using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static NavigationManager;
using static SpreadAlgorithms;

public class SpreadAlgorithms : MonoBehaviour
{
    // Prefab pro algoritmy, které chceme rozmístit
    public List<Spread> spreads = new List<Spread>();
    public Material material;
    private MaterialPropertyBlock materialBlock;

    private List<Coroutine> _coroutines;

    public List<ColorAlgorithm> _algorithms;
    public float speed = 2f;

    // Mìøítko, které urèuje vzdálenost mezi algoritmy
    public float scale = 1f;
    public float width = .2f;

    // Metoda pro rozmístìní algoritmù podle seznamu hodnot
    public void SpreadOnAxis(List<AlgorithmStats> values)
    {
        _coroutines = new();
        materialBlock = new MaterialPropertyBlock();
        // Zrušíme pøedchozí instancování algoritmù (pokud existují)
        foreach (var stat in values)
        {
            var parent = new GameObject(stat.Algorithm.ToString());
            parent.transform.position = new Vector3(stat.Path[0].x, stat.Path[0].y + 1, stat.Path[0].z);
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
        }
        foreach (var item in spreads)
        {
            _coroutines.Add(StartCoroutine(MoveSpread(item)));
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
            Destroy(item.TrailRenderer.transform.parent.gameObject);
        }

        spreads.Clear();
    }

    public IEnumerator MoveSpread(Spread item)
    {
        foreach (var targetPosition in item.AlgorithmStats.Path)
        {
            yield return StartCoroutine(MoveToPosition(item.TrailRenderer.transform.parent.gameObject, new Vector3(targetPosition.x, targetPosition.y + 1, targetPosition.z)));
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


    public class Spread
    {
        public TrailRenderer TrailRenderer { get; set; }
        public AlgorithmStats AlgorithmStats { get; set; }

        public Spread(TrailRenderer trailRenderer, AlgorithmStats algorithmStats)
        {
            TrailRenderer = trailRenderer;
            AlgorithmStats = algorithmStats;
        }
    }

    [Serializable]
    public class ColorAlgorithm
    {
        public NavigationAlgorithm algorithm;
        public Color color;
    }
}
