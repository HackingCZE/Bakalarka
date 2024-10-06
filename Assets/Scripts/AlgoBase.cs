using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class AlgoBase : IAlgo
{
    public async Task<List<AlgoNode>> GetResultPath(AlgoNode startNode, AlgoNode currentNode)
    {
        Debug.Log("Getting path");
        var path = NodeUtility.ReconstructPath(startNode, currentNode);
        return await Task.FromResult(path);
    }

    public abstract Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode);
}
public interface IAlgo
{
    Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode);
}
