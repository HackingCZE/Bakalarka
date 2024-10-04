using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static GameManager;

public class NavigationManager : MonoBehaviour, IDrawingNode
{
    public NavigationAlgorithm navigationAlgorithm;
    public Transform player;
    public Transform target;
    public int maxInteractions;
    public float threshold, maxStepLenght;
    public LayerMask barrierLayer;
    [SerializeField] GenerationArea area;



    private List<TreeCollectionItem> _nodes;
    private List<TreeCollectionItem> _path;
    private List<Line> _lines;

    [SerializeField] private float _elapsedTime = 0f;
    bool _timerIsRunning;
    RRTStar _rRT;

    private void Start()
    {
        _rRT = new RRTStar();
        _nodes = new List<TreeCollectionItem>();
        _lines = new List<Line>();
        _path = new List<TreeCollectionItem>();
    }

    public enum NavigationAlgorithm
    {
        RRT,
        AStar
    }

    private void Update()
    {
        if (_timerIsRunning)
        {
            _elapsedTime += Time.deltaTime;
        }
    }

    [Button]
    public async void StartAlgorithm()
    {
        _nodes = new List<TreeCollectionItem>();
        _lines = new List<Line>();
        _path = new List<TreeCollectionItem>();

        switch (navigationAlgorithm)
        {
            case NavigationAlgorithm.RRT:
                var val = await ((RRTStar)_rRT).Interation(player.position, target.position, maxInteractions, maxStepLenght, area, threshold, barrierLayer, this);

                this._path = val.recontructedPath;
                _timerIsRunning = false;

                //TryAddNewNode(((RRT)_rRT).treeCollection.root);
                break;
            case NavigationAlgorithm.AStar:
                break;
        }


    }

    public void DrawNode()
    {
        DrawNode(((RRTStar)_rRT).treeCollection.root);
    }

    private void DrawNode(TreeCollectionItem node)
    {
        if (node == null) return;
        foreach (var child in node.Children)
        {
            _lines.Add(new Line(node.Position, child.Position));
            TryAddNewNode(child);
            DrawNode(child);
        }
    }

    private void TryAddNewNode(TreeCollectionItem newNode)
    {
        foreach (var item in _nodes)
        {
            if (item == newNode)
            {
                Debug.LogError("Stejny node");
            }
        }
        _nodes.Add(newNode);
    }

    private void OnDrawGizmos()
    {
        if (_path == null || _path.Count == 0)
        {
            Gizmos.DrawSphere(player.position, 1f);
            Gizmos.DrawSphere(target.position, 1f);
        }

        for (int i = 0; i < _nodes.Count; i++)
        {
            var node = _nodes[i];
            Gizmos.color = Color.red;
            if (i == _nodes.Count - 1) Gizmos.color = Color.blue;
            Gizmos.DrawSphere(node.Position, .5f);
        }

        foreach (var item in _lines)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(item.start, item.end);
        }

        for (int i = 0; i < _path.Count; i++)
        {
            var node = _path[i];
            Gizmos.color = Color.black;
            if (i == _nodes.Count - 1) Gizmos.color = Color.blue;
            if (i == 0) Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(node.Position, .5f);
        }
    }
}
