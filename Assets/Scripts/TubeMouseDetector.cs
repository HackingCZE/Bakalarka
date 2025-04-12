using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NavigationManager;

public class TubeMouseDetector : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public string tubeTag = "Tube";
    List<SpreadAlgorithms.Spread> _spreads = new();
    float _width;
    public float nwidth = 2.5f;

    public List<AlgorithmStats> algorithmStats = new();
    public List<AlgorithmStats> newAlgorithmStats = new();
    private List<Vector3> points = new();

    private string _highlightName = "";
    private bool _shouldHighlight;
    void FixedUpdate()
    {
        if(_spreads.Count > 0) DetectMouseOverTube();
    }

    private void DetectMouseOverTube()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float newWidth = _width;

        newAlgorithmStats.Clear();

        foreach(var item in _spreads)
        {
            item.TrailRenderer.startWidth = newWidth;
            item.TrailRenderer.endWidth = newWidth;

            var newVal = newWidth / 2;
            if(item.GetLastRadius != newVal) item.GenerateTube(newVal);
        }

        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag(tubeTag) || _shouldHighlight)
        {
            foreach(var item in _spreads)
            {
                if((hit.collider != null && (item.Collider == hit.collider || item.AlgorithmStats.Algorithm.ToString() == hit.transform.name.ToString())) || _shouldHighlight && item.AlgorithmStats.Algorithm.ToString() == _highlightName) newWidth = _width * nwidth;
                else continue;

                newAlgorithmStats.Add(item.AlgorithmStats);

                item.TrailRenderer.startWidth = newWidth;
                item.TrailRenderer.endWidth = newWidth;

                var newVal = newWidth / 2;
                if(item.GetLastRadius != newVal) item.GenerateTube(newVal);
            }

            if(algorithmStats.Count != newAlgorithmStats.Count || algorithmStats.Count > 0 && newAlgorithmStats.Count > 0 && newAlgorithmStats[0].Algorithm != algorithmStats[0].Algorithm)
            {
                algorithmStats = new(newAlgorithmStats);
                points.Clear();

                foreach(var stat in algorithmStats)
                {
                    foreach(var item in stat.VisitedNodes)
                    {
                        if(!stat.IsPointNerby(item) && !algorithmStats.Any(e => (e != stat && e.IsPointNerby(item))))
                        {
                            points.Add(item);
                        }
                    }
                }
            }
        }
        else
        {
            algorithmStats.Clear();
            points.Clear();
        }


    }



    private void Update()
    {
        foreach(var item in points)
        {
            Matrix4x4 transform = Matrix4x4.TRS(item + new Vector3(0, .6f, 0), Quaternion.Euler(-90f, 0f, 90f), new Vector3(.5f, .5f, .5f));

            Graphics.DrawMesh(mesh, transform, material, 1);
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
}

