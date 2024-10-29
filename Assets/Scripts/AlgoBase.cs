using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static NavigationManager;
using Debug = UnityEngine.Debug;

public abstract class AlgoBase : IAlgo
{

    public Stopwatch Stopwatch { get; private set; }
    public int VisitedNodes { get; set; }
    public int MemoryUsage { get; set; }
    public List<AlgoNode> Result { get; set; }
    public NavigationAlgorithm Algorithm { get; set; }

    protected AlgoBase()
    {
        Stopwatch = new Stopwatch();
    }

    public async Task<List<AlgoNode>> GetResultPath(AlgoNode startNode, AlgoNode currentNode)
    {
        Debug.Log("Getting path");
        var path = NodeUtility.ReconstructPath(startNode, currentNode);

        Stopwatch.Stop();
        return await Task.FromResult(path);
    }
    public async Task<AlgoBase> RunStartAlgoInThread(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        Result = await Task.Run(() => StartAlgo(startNode, endNode, graph, drawingNode));
        return this;
    }

    public abstract Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode);
}
public interface IAlgo
{
    Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode);
}
