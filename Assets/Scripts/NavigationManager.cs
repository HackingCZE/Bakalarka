using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static GameManager;

public class NavigationManager : MonoBehaviour, IDrawingNode
{
    public NavigationAlgorithm navigationAlgorithm;
    NavigationAlgorithm _lastAlgo;
    public Transform player;
    public Transform target;
    public int maxInteractions;
    public float threshold, maxStepLenght;
    public LayerMask barrierLayer;
    [SerializeField] GenerationArea area;
    [SerializeField] SimpleVisualizer visualizer;

    public List<string> times = new List<string>();



    private List<TreeCollectionItem> _nodes;
    private List<TreeCollectionItem> _path;
    private List<AlgoNode> _result, _oldResult;
    private List<Line> _lines;

    [SerializeField] private float _elapsedTime = 0f;
    bool _timerIsRunning;
    RRTStar _rRT;
    AlgoBase _algoBase;
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
        AStar,
        DFS,
        BFS,
        DIJKSTRA
    }

    private void Update()
    {
        if (_timerIsRunning)
        {
            _elapsedTime += Time.deltaTime;
        }
    }

    [Button]
    public void GetN()
    {
        _newNodes = visualizer.GetNodes();
    }

    [Button]
    public async void StartAlgorithm()
    {
        _timerIsRunning = true;
        _elapsedTime = 0f;
        _lastAlgo = navigationAlgorithm;
        _oldResult = _result;
        _result = new();
        _nodes = new List<TreeCollectionItem>();
        _lines = new List<Line>();
        _path = new List<TreeCollectionItem>();
        List<AlgoNode> nodes = new List<AlgoNode>();
        AlgoNode startNode = null, endNode = null;

        switch (navigationAlgorithm)
        {
            case NavigationAlgorithm.RRT:
                var val = await ((RRTStar)_rRT).Interation(player.position, target.position, maxInteractions, maxStepLenght, area, threshold, barrierLayer, this);

                this._path = val.recontructedPath;

                //TryAddNewNode(((RRT)_rRT).treeCollection.root);
                break;
            case NavigationAlgorithm.DFS:
                _algoBase = new DFS();
                GetNodes(out nodes, out startNode, out endNode);

                this._result = await ((DFS)_algoBase).StartAlgo(startNode, endNode, nodes, this);

                //Invoke(nameof(StartAlgorithm), 2);
                break;
            case NavigationAlgorithm.BFS:
                _algoBase = new BFS();
                GetNodes(out nodes, out startNode, out endNode);

                this._result = await ((BFS)_algoBase).StartAlgo(startNode, endNode, nodes, this);
                break;
            case NavigationAlgorithm.DIJKSTRA:
                _algoBase = new Dijkstra();
                GetNodes(out nodes, out startNode, out endNode);

                this._result = await ((Dijkstra)_algoBase).StartAlgo(startNode, endNode, nodes, this);
                break;
            case NavigationAlgorithm.AStar:
                _algoBase = new AStar();
                GetNodes(out nodes, out startNode, out endNode);

                this._result = await ((AStar)_algoBase).StartAlgo(startNode, endNode, nodes, this);
                break;
        }

        // after get result path
        _timerIsRunning = false;
        AddTime();
    }

    private void AddTime()
    {
        float minutes = Mathf.Floor(_elapsedTime / 60);             // Get the total minutes
        float seconds = Mathf.Floor(_elapsedTime % 60);             // Get the remaining seconds
        float milliseconds = (_elapsedTime * 1000) % 1000;          // Get the remaining milliseconds

        string timeFormatted = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        times.Add(_lastAlgo.ToString() + ":(" + timeFormatted + ")");
    }

    private void GetNodes(out List<AlgoNode> nodes, out AlgoNode startNode, out AlgoNode endNode)
    {
        _newNodes = new List<AlgoNode>();
        nodes = visualizer.GetNodes();
        startNode = NodeUtility.FindClosestNode(nodes, player.position);
        endNode = NodeUtility.FindClosestNode(nodes, target.position);
    }

    List<AlgoNode> _newNodes = new List<AlgoNode>();
    public void DrawNode(AlgoNode node)
    {
        foreach (var item in _newNodes)
        {
            if (item == node)
            {
                Debug.LogError("Stejny node");
            }
        }
        _newNodes.Add(node);
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
        if (_oldResult != null)
        {
            Gizmos.color = Color.yellow;

            foreach (var item in _oldResult)
            {
                Gizmos.DrawSphere(item.Position, .4f);

            }
        }
        if (_newNodes == null) return;
        for (int i = 0; i < _newNodes.Count; i++)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(_newNodes[i].Position, .5f);
            for (int j = 0; j < _newNodes[i].Neighbors.Count; j++)
            {
                Gizmos.color = Color.blue;

                Gizmos.DrawSphere(_newNodes[i].Neighbors[j].Position, .5f);
            }
        }
       

        if (_result == null) return;
        Gizmos.color = Color.black;

        foreach (var item in _result)
        {
            Gizmos.DrawSphere(item.Position, .5f);

        }



        return;
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
