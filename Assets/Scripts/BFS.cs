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
        _queue = new Queue<AlgoNode>();

        _queue.Enqueue(startNode);

        while(_queue.Count > 0)
        {
            await Task.Yield();
            AlgoNode currentNode = _queue.Dequeue();
            drawingNode.DrawNode(currentNode);

            if(currentNode == endNode)
            {
                return await GetResultPath(startNode, currentNode);
            }

            currentNode.Visited = true;
            Visited.Add(currentNode.Position);

            foreach(var neighbor in currentNode.Neighbours)
            {
                if(!neighbor.Visited)
                {
                    neighbor.Parent = currentNode;

                    if(!_queue.Contains(neighbor))
                    {
                        _queue.Enqueue(neighbor);
                    }
                }
            }
            MemoryUsage = Mathf.Max(MemoryUsage, _queue.Count);
        }

        throw new System.Exception("Path not found");
    }

}
