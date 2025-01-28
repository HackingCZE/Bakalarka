using System;
using System.Collections.Generic;
using UnityEngine;

public class TubeMouseDetector : MonoBehaviour
{
    public string tubeTag = "Tube";
    List<SpreadAlgorithms.Spread> _spreads = new();
    float _width;
    public float nwidth = 2.5f;
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

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag(tubeTag))
        {
            foreach (var item in _spreads)
            {
                if (hit.collider != null && item.Collider == hit.collider) newWidth = _width * nwidth;
                else continue;

                item.TrailRenderer.startWidth = newWidth;
                item.TrailRenderer.endWidth = newWidth;

                var newVal = newWidth / 2;
                if (item.GetLastRadius != newVal) item.GenerateTube(newVal);
            }
        }

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
}
