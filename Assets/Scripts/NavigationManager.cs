using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static GameManager;
using static NavigationManager;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

public class NavigationManager : MonoBehaviour, IDrawingNode
{
    public static NavigationManager Instance { get; private set; }
    public NavigationAlgorithm navigationAlgorithm;
    NavigationAlgorithm _lastAlgo;
    public Transform player;
    public Transform target;
    public int maxInteractions;
    public float threshold, maxStepLenght;
    public LayerMask barrierLayer;
    [SerializeField] GenerationArea area;
    [SerializeField] LSystemVisualizer visualizer; 
    [SerializeField] SimpleVisualizer visualizer1; 

    public List<string> times = new List<string>();

    public float DistanceBetweenPoints;

    private List<TreeCollectionItem> _nodes;
    private List<TreeCollectionItem> _path;
    private List<AlgoNode> _result, _oldResult;
    private List<Line> _lines;

    RRTStar _rRT;
    AlgoBase _algoBase;
    AlgoNode _startNode = null, _endNode = null;
    List<AlgoNode> _mapNodes = new List<AlgoNode>();

    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        _rRT = new RRTStar();
        _nodes = new List<TreeCollectionItem>();
        _lines = new List<Line>();
        _path = new List<TreeCollectionItem>();
    }

    [Serializable]
    public enum NavigationAlgorithm
    {
        [AlgorithmType(typeof(DFS))]
        DFS,

        [AlgorithmType(typeof(BFS))]
        BFS,

        [AlgorithmType(typeof(Dijkstra))]
        DIJKSTRA,

        [AlgorithmType(typeof(AStar))]
        AStar,

        [AlgorithmType(typeof(RandomizedDFS))]
        RandomizedDFS,

        [AlgorithmType(typeof(DLS))]
        DLS,

        [AlgorithmType(typeof(RandomizedWalk))]
        RandomizedWalk,
            
        [AlgorithmType(typeof(BidirectionalBFS))]
        BidirectionalBFS,

        [AlgorithmType(typeof(BidirectionalDFS))]
        BidirectionalDFS
    }


    [Button]
    public void GetN()
    {
        if(visualizer != null)_newNodes = visualizer.GetNodes();
        if(visualizer1 != null)_newNodes = visualizer1.GetNodes();
    }

    private void InitFinding()
    {
        _lastAlgo = navigationAlgorithm;
        _oldResult = _result;
        _result = new();
        _nodes = new List<TreeCollectionItem>();
        _lines = new List<Line>();
        _path = new List<TreeCollectionItem>();
        _startNode = null;
        _endNode = null;
        _mapNodes = new List<AlgoNode>();
    }

    [Button]
    public async void StartAlgorithm()
    {
        this._result = (await RunAlgoInThread(navigationAlgorithm)).Result;

        // after get result path
    }

    public void PlacePoints()
    {
        float val = 50;
        if (visualizer != null) val = visualizer.GetNodes().Count;
        if (visualizer1 != null) val = visualizer1.GetNodes().Count;
        player.transform.position = new Vector3(Random.Range(-50f - val /2, 50f + val / 2), 0, Random.Range(-50f - val / 2, 50f + val / 2));

        do
        {
            target.transform.position = new Vector3(Random.Range(-50f - val / 2, 50f + val / 2), 0, Random.Range(-50f - val / 2, 50f + val / 2));
        } while (Vector3.Distance(player.transform.position, target.transform.position) <= 40 ||
           player.transform.position == target.transform.position);

        DistanceBetweenPoints = Vector3.Distance(player.transform.position, target.transform.position);
    }


    public async Task<List<AlgorithmStats>> GetOrderOfAlgorithms()
    {

        List<Task<AlgoBase>> tasks = new List<Task<AlgoBase>>()
        {
             RunAlgoInThread(NavigationAlgorithm.BFS),
             RunAlgoInThread(NavigationAlgorithm.AStar),
             RunAlgoInThread(NavigationAlgorithm.DIJKSTRA),
             RunAlgoInThread(NavigationAlgorithm.RandomizedWalk),
             RunAlgoInThread(NavigationAlgorithm.RandomizedDFS),
             RunAlgoInThread(NavigationAlgorithm.BidirectionalBFS),
             RunAlgoInThread(NavigationAlgorithm.BidirectionalDFS),
             RunAlgoInThread(NavigationAlgorithm.DFS)        
        };

        await Task.WhenAll(tasks);

        var results = tasks.ConvertAll(task =>
        {
            var algoResult = task.Result;

            return new AlgorithmStats(algoResult.Algorithm, algoResult.Stopwatch.Elapsed, algoResult.VisitedNodes, algoResult.MemoryUsage, algoResult.Result.Count, algoResult.Result, algoResult.NodesCount);
        });

        return results.OrderBy(a => a.GetEfficiencyScore()).ToList();
    }

    public List<AlgorithmStats> SortAlgorithmsByEfficiency(List<AlgorithmStats> algorithms)
    {
        return algorithms
            .OrderBy(a => a.VisitedNodes)          // Pak podle navštívených uzlù (èím ménì, tím lepší)
            .ThenBy(a => a.MemoryUsage)           // Poté podle pamìti (èím ménì, tím lepší)
            .ThenBy(a => a.ResultPathLength)      // Nakonec podle délky cesty (èím kratší, tím lepší)
            .ToList();
    }

    private async Task<AlgoBase> RunAlgoInThread(NavigationAlgorithm navigationAlgorithm)
    {
        InitFinding();

        var algorithmType = navigationAlgorithm.GetAlgorithmType();
        _algoBase = (AlgoBase)Activator.CreateInstance(algorithmType);
        _algoBase.Algorithm = navigationAlgorithm;

        GetNodes(out _mapNodes, out _startNode, out _endNode);

        return await(_algoBase).RunStartAlgoInThread(_startNode, _endNode, _mapNodes, this);
    }

    [Button]
    public bool CheckPlanar()
    {
        GetNodes(out _mapNodes, out _startNode, out _endNode);

        return GetComponent<PlanarityChecker>().StartPlanar(_mapNodes);
    }


    private void GetNodes(out List<AlgoNode> nodes, out AlgoNode startNode, out AlgoNode endNode)
    {
        _newNodes = new List<AlgoNode>();
        nodes = new();
        if (visualizer != null) nodes = visualizer.GetNodes();
        if (visualizer1 != null) nodes = visualizer1.GetNodes();
        startNode = NodeUtility.FindClosestNode(nodes, player.position);
        player.position = startNode.Position;
        endNode = NodeUtility.FindClosestNode(nodes, target.position);
        target.position = endNode.Position;

    }

    List<AlgoNode> _newNodes = new List<AlgoNode>();
    public void DrawNode(AlgoNode node)
    {
        return;
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
            for (int j = 0; j < _newNodes[i].Neighbours.Count; j++)
            {
                Gizmos.color = Color.blue;

                Gizmos.DrawSphere(_newNodes[i].Neighbours[j].Position, .5f);
            }
        }


        if (_result == null) return;
        Gizmos.color = Color.black;

        int c = 0;
        foreach (var item in _result)
        {
            Handles.Label(item.Position + new Vector3(0, 1, 0), c.ToString());

            Gizmos.DrawSphere(item.Position, .5f);
            c++;
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

    public class AlgorithmStats
    {
        public NavigationAlgorithm Algorithm { get; set; }
        public TimeSpan Time { get; set; }
        public int VisitedNodes { get; set; }
        public int MemoryUsage { get; set; }
        public int ResultPathLength { get; set; }
        public int NodesCount { get; set; }
        public List<Vector3> Path { get; set; }

        public float GetEfficiencyScore()
        {
            return 1;
        }

        public AlgorithmStats(NavigationAlgorithm algorithm, TimeSpan time, int visitedNodes, int memoryUsage, int resultPathLength, List<AlgoNode> path, int nodesCount)
        {
            Algorithm = algorithm;
            NodesCount = nodesCount;
            Time = time;
            VisitedNodes = visitedNodes;
            MemoryUsage = memoryUsage;
            ResultPathLength = resultPathLength;

            List<Vector3> newPath = new();

            for (int i = 0; i < path.Count; i++)
            {
                var item = path[i];
                if (item.Type == SimpleVisualizer.RoadTileType.RoadCurve && newPath.Count > 0 && i != path.Count - 1)
                {
                    var dir1 = (item.Neighbours[0].Position - item.Position).normalized;
                    var dir2 = (item.Neighbours[1].Position - item.Position).normalized;

                    var rotation = 0f;// Reset rotation

                    // Determine rotation based on the curve's neighbor alignment
                    if (Mathf.Abs(dir1.x) > Mathf.Abs(dir1.z))
                        rotation = dir1.x > 0 ? (dir2.z > 0 ? -90f : 0f) : (dir2.z > 0 ? 180f : 90f); // Soused1 je na ose X (vodorovnì)
                    else
                        rotation = dir1.z > 0 ? (dir2.x > 0 ? -90f : 180f) : (dir2.x > 0 ? 0f : 90f); // Soused1 je na ose Z (svisle)

                    float value1 = 1;
                    float value2 = .35f;

                    Vector3 point1 = Vector3.zero, point2 = Vector3.zero, point3 = Vector3.zero;
                    if (rotation == 0)
                    {
                        point1 = item.Position + new Vector3(value1, 0, 0);
                        point2 = item.Position + new Vector3(0, 0, -value1);
                        point3 = item.Position + new Vector3(value2, 0, -value2);
                    }
                    else if (rotation == 90)
                    {
                        point1 = item.Position + new Vector3(-value1, 0, 0);
                        point2 = item.Position + new Vector3(0, 0, -value1);
                        point3 = item.Position + new Vector3(-value2, 0, -value2);
                    }
                    else if (rotation == -90 || rotation == 270)
                    {
                        point1 = item.Position + new Vector3(value1, 0, 0);
                        point2 = item.Position + new Vector3(0, 0, value1);
                        point3 = item.Position + new Vector3(value2, 0, value2);
                    }
                    else if (rotation == 180)
                    {
                        point1 = item.Position + new Vector3(-value1, 0, 0);
                        point2 = item.Position + new Vector3(0, 0, value1);
                        point3 = item.Position + new Vector3(-value2, 0, value2);
                    }

                    var previousItem = newPath.Count > 0 ? newPath[newPath.Count - 1] : Vector3.zero;

                    if (previousItem != null)
                    {
                        // Vzdálenosti mezi pøedchozím prvkem a body
                        float distanceToPoint1 = Vector3.Distance(previousItem, point1);
                        float distanceToPoint2 = Vector3.Distance(previousItem, point2);

                        // Pøidání bodù na základì vzdálenosti
                        if (distanceToPoint1 < distanceToPoint2)
                        {
                            newPath.Add(point1);
                            newPath.Add(point3);
                            newPath.Add(point2);
                        }
                        else
                        {
                            newPath.Add(point2);
                            newPath.Add(point3);
                            newPath.Add(point1);
                        }
                    }
                    else
                    {
                        // Pokud neexistuje pøedchozí prvek, pøidáme body bez ohledu na poøadí
                        newPath.Add(point1);
                        newPath.Add(point3);
                        newPath.Add(point2);
                    }
                }
                else
                {
                    // Pokud není RoadCurve, pøidáme pøímo položku
                    newPath.Add(item.Position);
                }
            }

            Path = newPath;
        }
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class AlgorithmTypeAttribute : Attribute
{
    public Type AlgorithmType { get; }

    public AlgorithmTypeAttribute(Type algorithmType)
    {
        AlgorithmType = algorithmType;
    }
}

public static class NavigationAlgorithmExtensions
{
    public static Type GetAlgorithmType(this NavigationAlgorithm algorithm)
    {
        var memberInfo = typeof(NavigationAlgorithm).GetMember(algorithm.ToString()).FirstOrDefault();
        var attribute = memberInfo?.GetCustomAttribute<AlgorithmTypeAttribute>();
        return attribute?.AlgorithmType ?? throw new ArgumentException($"No AlgorithmType defined for {algorithm}");
    }
}
