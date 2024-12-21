using System;
using System.Collections;
using System.Collections.Generic;
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

    public List<ColorAlgorithm> _algorithms;
    public float speed = 2f;

    // M���tko, kter� ur�uje vzd�lenost mezi algoritmy
    public float scale = 1f;
    public float width = .2f;

    // Metoda pro rozm�st�n� algoritm� podle seznamu hodnot
    public void SpreadOnXAxis(List<AlgorithmStats> values)
    {
        materialBlock = new MaterialPropertyBlock();
        // Zru��me p�edchoz� instancov�n� algoritm� (pokud existuj�)
        foreach (var stat in values)
        {
            var parent = new GameObject(stat.Algorithm.ToString());
            parent.transform.position = stat.Path[0];
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
            spreads.Add(new Spread(trailRenderer, stat));
        }

        int count = spreads.Count;

        float centerX = (count - 1) * scale / 2f;

        // Rozm�st�me algoritmy podle hodnot v seznamu
        for (int i = 0; i < count; i++)
        {

            float xPos = (i * scale) - centerX + spreads[i].TrailRenderer.transform.position.x;
            float zPos = (i * scale) - centerX + spreads[i].TrailRenderer.transform.position.z;
            float yPos = (i * (scale/5)) + spreads[i].TrailRenderer.transform.position.y;

            // Nastaven� pozice objektu
            spreads[i].TrailRenderer.transform.localPosition = new Vector3(xPos, yPos, zPos);
        }
        foreach (var item in spreads)
        {
            StartCoroutine(MoveSpread(item));
        }
    }

    public IEnumerator MoveSpread(Spread item)
    {
        foreach (var targetPosition in item.AlgorithmStats.Path)
        {
            yield return StartCoroutine(MoveToPosition(item.TrailRenderer.transform.parent.gameObject, targetPosition));
        }
    }

    private IEnumerator MoveToPosition(GameObject obj, Vector3 targetPosition)
    {
        while (obj.transform.position != targetPosition)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition, speed * Time.deltaTime);

            // �ek�me na dal�� framex
            yield return null;
        }

        obj.transform.position = targetPosition;
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
