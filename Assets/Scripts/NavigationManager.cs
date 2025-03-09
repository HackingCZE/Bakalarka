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
        BidirectionalDFS,

        None
    }


    [Button]
    public void GetN()
    {
        if(visualizer != null) _newNodes = visualizer.GetNodes();
        if(visualizer1 != null) _newNodes = visualizer1.GetNodes();
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


    public void PlacePoints()
    {
        float val = 50;
        if(visualizer != null) val = visualizer.GetNodes().Count;
        if(visualizer1 != null) val = visualizer1.GetNodes().Count;

        int currentDistance = 50; // Výchozí hodnota

        if(MainGameManager.Instance.GetLevel() > 80)
            currentDistance = Mathf.Max((MainGameManager.Instance.GetLevel() / 50) * 20 + 160, 1);
        else if(MainGameManager.Instance.GetLevel() > 50)
            currentDistance = 160;
        else if(MainGameManager.Instance.GetLevel() > 25)
            currentDistance = 120;
        else if(MainGameManager.Instance.GetLevel() > 15)
            currentDistance = 90;
        else if(MainGameManager.Instance.GetLevel() > 5)
            currentDistance = 70;


        Vector3 newPosition;
        int safetyCounter = 200;

        do
        {
            newPosition = GetRandomPosition(currentDistance, val);
            player.transform.position = GetRandomPosition(currentDistance, val);
            safetyCounter--;
        }
        while(Vector3.Distance(player.transform.position, newPosition) <= (currentDistance / 1.5f) && safetyCounter > 0);

        target.transform.position = newPosition;
        DistanceBetweenPoints = Vector3.Distance(player.transform.position, target.transform.position);
    }
    private Vector3 GetRandomPosition(int distance, float val)
    {
        return new Vector3(
            Random.Range(-distance - val / 2, distance + val / 2),
            0,
            Random.Range(-distance - val / 2, distance + val / 2)
        );
    }

    int _biderectionalDFSLastCounted = 0;

    public async Task<List<AlgorithmStats>> GetOrderOfAlgorithms()
    {
        if(_biderectionalDFSLastCounted < 0) _biderectionalDFSLastCounted = 0;

        List<Task<AlgoBase>> tasks = new List<Task<AlgoBase>>()
        {
             RunAlgoInThread(NavigationAlgorithm.AStar),
             RunAlgoInThread(NavigationAlgorithm.DFS),
             RunAlgoInThread(NavigationAlgorithm.BidirectionalBFS)
        };

        if(_biderectionalDFSLastCounted < 3)
        {
            try { FindFirstObjectByType<VoteNavigationAlgorithm>(FindObjectsInactive.Include).gameObject.SetActive(true); } catch { }
            tasks.Add(RunAlgoInThread(NavigationAlgorithm.BidirectionalDFS));
        }
        else
        {
            try { FindFirstObjectByType<VoteNavigationAlgorithm>(FindObjectsInactive.Include).gameObject.SetActive(false); } catch { }
        }

        await Task.WhenAll(tasks);

        List<AlgorithmStats> results = tasks.SelectMany(task =>
        {
            var algoResult = task.Result;

            List<AlgorithmStats> currentResult = new();

            if(algoResult.Algorithm.ToString().Contains("Bidirectional"))
            {
                currentResult.Add(new AlgorithmStats(algoResult.Algorithm, algoResult.Stopwatch.Elapsed, algoResult.Visited, algoResult.MemoryUsage, algoResult.Result.Count, ((BiderectionalAlgoBase)algoResult).forwardPath, algoResult.NodesCount));
                currentResult.Add(new AlgorithmStats(algoResult.Algorithm, algoResult.Stopwatch.Elapsed, algoResult.Visited, algoResult.MemoryUsage, algoResult.Result.Count, ((BiderectionalAlgoBase)algoResult).backwardPath, algoResult.NodesCount));
            }
            else
            {
                currentResult.Add(new AlgorithmStats(algoResult.Algorithm, algoResult.Stopwatch.Elapsed, algoResult.Visited, algoResult.MemoryUsage, algoResult.Result.Count, algoResult.Result, algoResult.NodesCount));
            }

            return currentResult;
        }).ToList();

        var res = results.OrderBy(a => a.GetEfficiencyScore()).ToList();
        if(res[0].Algorithm == NavigationAlgorithm.BidirectionalDFS) _biderectionalDFSLastCounted += Random.Range(1,2);
        else _biderectionalDFSLastCounted--;
        return res;
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

        return await (_algoBase).RunStartAlgoInThread(_startNode, _endNode, _mapNodes, this);
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
        if(visualizer != null) nodes = visualizer.GetNodes();
        if(visualizer1 != null) nodes = visualizer1.GetNodes();
        startNode = NodeUtility.FindClosestNode(nodes, player.position);
        player.position = startNode.Position;
        endNode = NodeUtility.FindClosestNode(nodes, target.position);
        target.position = endNode.Position;

    }

    List<AlgoNode> _newNodes = new List<AlgoNode>();
    public void DrawNode(AlgoNode node)
    {
        return;
        foreach(var item in _newNodes)
        {
            if(item == node)
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
        if(node == null) return;
        foreach(var child in node.Children)
        {
            _lines.Add(new Line(node.Position, child.Position));
            TryAddNewNode(child);
            DrawNode(child);
        }
    }

    private void TryAddNewNode(TreeCollectionItem newNode)
    {
        foreach(var item in _nodes)
        {
            if(item == newNode)
            {
                Debug.LogError("Stejny node");
            }
        }
        _nodes.Add(newNode);
    }

    private void OnDrawGizmos()
    {
        if(_path == null || _path.Count == 0)
        {
            Gizmos.DrawSphere(player.position, 1f);
            Gizmos.DrawSphere(target.position, 1f);
        }
        if(_oldResult != null)
        {
            Gizmos.color = Color.yellow;

            foreach(var item in _oldResult)
            {
                Gizmos.DrawSphere(item.Position, .4f);

            }
        }
        if(_newNodes == null) return;
        for(int i = 0; i < _newNodes.Count; i++)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(_newNodes[i].Position, .5f);
            for(int j = 0; j < _newNodes[i].Neighbours.Count; j++)
            {
                Gizmos.color = Color.blue;

                Gizmos.DrawSphere(_newNodes[i].Neighbours[j].Position, .5f);
            }
        }


        if(_result == null) return;
        Gizmos.color = Color.black;

        int c = 0;
        foreach(var item in _result)
        {
#if UNITY_EDITOR
            Handles.Label(item.Position + new Vector3(0, 1, 0), c.ToString());
#endif
            Gizmos.DrawSphere(item.Position, .5f);
            c++;
        }



        return;
        for(int i = 0; i < _nodes.Count; i++)
        {
            var node = _nodes[i];
            Gizmos.color = Color.red;
            if(i == _nodes.Count - 1) Gizmos.color = Color.blue;
            Gizmos.DrawSphere(node.Position, .5f);
        }

        foreach(var item in _lines)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(item.start, item.end);
        }

        for(int i = 0; i < _path.Count; i++)
        {
            var node = _path[i];
            Gizmos.color = Color.black;
            if(i == _nodes.Count - 1) Gizmos.color = Color.blue;
            if(i == 0) Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(node.Position, .5f);
        }
    }

    [Serializable]
    public class AlgorithmStats
    {
        [field: SerializeField] public NavigationAlgorithm Algorithm { get; set; }
        public TimeSpan Time { get; set; }
        // public int VisitedNodes { get; set; }
        public int MemoryUsage { get; set; }
        public int ResultPathLength { get; set; }
        public int NodesCount { get; set; }
        [field: SerializeField] public List<Vector3> Path { get; set; }
        public List<Vector3> VisitedNodes { get; set; }

        public bool IsPointNerby(Vector3 point)
        {
            return Path.Any(e => Vector3.Distance(point, e) < .8f);
        }

        public float GetEfficiencyScore()
        {
            return VisitedNodes.Count;
        }

        public AlgorithmStats(NavigationAlgorithm algorithm, TimeSpan time, List<Vector3> visitedNodes, int memoryUsage, int resultPathLength, List<AlgoNode> path, int nodesCount)
        {
            Algorithm = algorithm;
            NodesCount = nodesCount;
            Time = time;
            VisitedNodes = visitedNodes;
            MemoryUsage = memoryUsage;
            ResultPathLength = resultPathLength;

            Path = GetNewPath(path);
        }


        private List<Vector3> GetNewPath(List<AlgoNode> path)
        {
            List<Vector3> newPath = new();

            for(int i = 0; i < path.Count; i++)
            {
                var item = path[i];
                if(item.Type == SimpleVisualizer.RoadTileType.RoadCurve && newPath.Count > 0 && i != path.Count - 1)
                {
                    var dir1 = (item.Neighbours[0].Position - item.Position).normalized;
                    var dir2 = (item.Neighbours[1].Position - item.Position).normalized;

                    var rotation = 0f;// Reset rotation

                    // Determine rotation based on the curve's neighbor alignment
                    if(Mathf.Abs(dir1.x) > Mathf.Abs(dir1.z))
                        rotation = dir1.x > 0 ? (dir2.z > 0 ? -90f : 0f) : (dir2.z > 0 ? 180f : 90f); // Soused1 je na ose X (vodorovnì)
                    else
                        rotation = dir1.z > 0 ? (dir2.x > 0 ? -90f : 180f) : (dir2.x > 0 ? 0f : 90f); // Soused1 je na ose Z (svisle)

                    float value1 = 1;
                    float value2 = .35f;

                    Vector3 point1 = Vector3.zero, point2 = Vector3.zero, point3 = Vector3.zero;
                    if(rotation == 0)
                    {
                        point1 = item.Position + new Vector3(value1, 0, 0);
                        point2 = item.Position + new Vector3(0, 0, -value1);
                        point3 = item.Position + new Vector3(value2, 0, -value2);
                    }
                    else if(rotation == 90)
                    {
                        point1 = item.Position + new Vector3(-value1, 0, 0);
                        point2 = item.Position + new Vector3(0, 0, -value1);
                        point3 = item.Position + new Vector3(-value2, 0, -value2);
                    }
                    else if(rotation == -90 || rotation == 270)
                    {
                        point1 = item.Position + new Vector3(value1, 0, 0);
                        point2 = item.Position + new Vector3(0, 0, value1);
                        point3 = item.Position + new Vector3(value2, 0, value2);
                    }
                    else if(rotation == 180)
                    {
                        point1 = item.Position + new Vector3(-value1, 0, 0);
                        point2 = item.Position + new Vector3(0, 0, value1);
                        point3 = item.Position + new Vector3(-value2, 0, value2);
                    }

                    var previousItem = newPath.Count > 0 ? newPath[newPath.Count - 1] : Vector3.zero;

                    if(previousItem != null)
                    {
                        // Vzdálenosti mezi pøedchozím prvkem a body
                        float distanceToPoint1 = Vector3.Distance(previousItem, point1);
                        float distanceToPoint2 = Vector3.Distance(previousItem, point2);

                        // Pøidání bodù na základì vzdálenosti
                        if(distanceToPoint1 < distanceToPoint2)
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
            return newPath;
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
