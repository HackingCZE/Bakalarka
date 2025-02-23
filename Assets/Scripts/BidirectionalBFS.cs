using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class BidirectionalBFS : BiderectionalAlgoBase
{
    private Queue<AlgoNode> _forwardQueue;
    private Queue<AlgoNode> _backwardQueue;

    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        Stopwatch.Start();
        _forwardQueue = new Queue<AlgoNode>();
        _backwardQueue = new Queue<AlgoNode>();

        _forwardParents = new Dictionary<AlgoNode, AlgoNode>();
        _backwardParents = new Dictionary<AlgoNode, AlgoNode>();

        _forwardQueue.Enqueue(startNode);
        _backwardQueue.Enqueue(endNode);

        _forwardParents[startNode] = null;
        _backwardParents[endNode] = null;

        VisitedNodes = 2;

        while(_forwardQueue.Count > 0 && _backwardQueue.Count > 0)
        {
            await Task.Yield();

            // Forward search step
            if(Step(_forwardQueue, _forwardParents, _backwardParents, drawingNode, true))
            {
                return await GetResultPath(startNode, endNode);
            }

            // Backward search step
            if(Step(_backwardQueue, _backwardParents, _forwardParents, drawingNode, false))
            {
                return await GetResultPath(startNode, endNode);
            }

            // Update memory usage
            MemoryUsage = Mathf.Max(MemoryUsage, _forwardQueue.Count + _backwardQueue.Count);
        }

        throw new System.Exception("Path not found");
    }

    public bool Step(Queue<AlgoNode> queue, Dictionary<AlgoNode, AlgoNode> currentParents, Dictionary<AlgoNode, AlgoNode> otherParents, IDrawingNode drawingNode, bool isForward)
    {
        if(queue.Count == 0) return false;

        AlgoNode currentNode = queue.Dequeue();
        drawingNode.DrawNode(currentNode);

        foreach(var neighbor in currentNode.Neighbours)
        {
            if(!currentParents.ContainsKey(neighbor))
            {
                queue.Enqueue(neighbor);
                currentParents[neighbor] = currentNode;
                VisitedNodes++;

                if(otherParents.ContainsKey(neighbor))
                {
                    return true; // Meeting point found
                }
            }
        }

        return false;
    }
}
