using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class BFS : AlgoBase
{
    Queue<AlgoNode> _queue;

    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        Stopwatch.Start();
        int c = 0;
        _queue = new Queue<AlgoNode>();

        _queue.Enqueue(startNode);
        startNode.Visited = true;
        VisitedNodes++;

        while (_queue.Count > 0)
        {
            await Task.Yield();
            AlgoNode currentNode = _queue.Dequeue();
            drawingNode.DrawNode(currentNode);

            if (currentNode == endNode)
            {
                return await GetResultPath(startNode, currentNode);
            }

            foreach (var neighbor in currentNode.Neighbours)
            {
                if (!neighbor.Visited)
                {
                    _queue.Enqueue(neighbor);
                    MemoryUsage = Mathf.Max(MemoryUsage, _queue.Count);

                    neighbor.Visited = true;
                    VisitedNodes++;
                    neighbor.Parent = currentNode;
                }
            }
        }

        throw new System.Exception("Path not found");
    }

}
