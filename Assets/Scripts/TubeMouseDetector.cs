using System;
using System.Collections.Generic;
using UnityEngine;

public class TubeMouseDetector : MonoBehaviour
{
    public string tubeTag = "Tube";
    List<SpreadAlgorithms.Spread> _spreads = new();
    float _width;
    public float nwidth = 2.5f;

    private List<Vector3> currentPoints = new();

    private string _highlightName = "";
    private bool _shouldHighlight;
    void FixedUpdate()
    {
        if (_spreads.Count > 0) DetectMouseOverTube();
    }

    private void DetectMouseOverTube()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float newWidth = _width;

        foreach (var item in _spreads)
        {
            item.TrailRenderer.startWidth = newWidth;
            item.TrailRenderer.endWidth = newWidth;

            var newVal = newWidth / 2;
            if (item.GetLastRadius != newVal) item.GenerateTube(newVal);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag(tubeTag) || _shouldHighlight)
        {
            foreach (var item in _spreads)
            {
                if ((hit.collider != null && (item.Collider == hit.collider || item.AlgorithmStats.Algorithm.ToString() == hit.transform.name.ToString())) || _shouldHighlight && item.AlgorithmStats.Algorithm.ToString() == _highlightName) newWidth = _width * nwidth;
                else continue;

                currentPoints = item.AlgorithmStats.VisitedNodes;

                item.TrailRenderer.startWidth = newWidth;
                item.TrailRenderer.endWidth = newWidth;

                var newVal = newWidth / 2;
                if (item.GetLastRadius != newVal) item.GenerateTube(newVal);
            }
        }

    }

    public void Highlight(string name, bool isOn)
    {
        _highlightName = name;
        _shouldHighlight = isOn;
    }

    public void SetTubes(List<SpreadAlgorithms.Spread> spreads, float width)
    {
        _spreads = spreads;
        _width = width;
    }
    public void Clear()
    {
        _spreads.Clear();
    }

    private void OnDrawGizmos()
    {
        foreach(var item in currentPoints)
        {
            Gizmos.DrawSphere(item, 1f);
        }
    }
}
