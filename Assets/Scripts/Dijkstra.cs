using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Dijkstra : AlgoBase
{ 
    PriorityQueue<AlgoNode> _pq;

    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        Stopwatch.Start();

        int c = 0;
        _pq = new PriorityQueue<AlgoNode>();

        startNode.Parent = null;
        startNode.Value = 0; // Distance

        _pq.Enqueue(startNode, 0);

        while (_pq.Count > 0)
        {
            await Task.Yield();
            AlgoNode currentNode = _pq.Dequeue();

            if (currentNode.Visited) continue;
            drawingNode.DrawNode(currentNode);

            if (currentNode == endNode)
            {
                return await GetResultPath(startNode, currentNode);
            }

            currentNode.Visited = true;
            VisitedNodes++;

            foreach (var neighbor in currentNode.Neighbours)
            {
                var edgeWeight = Vector3.Distance(currentNode.Position, neighbor.Position);

                var newDistance = currentNode.Value + edgeWeight;

                if (newDistance < neighbor.Value)
                {
                    neighbor.Value = newDistance;
                    neighbor.Parent = currentNode;

                    _pq.Enqueue(neighbor, newDistance);
                    MemoryUsage = Mathf.Max(MemoryUsage, _pq.Count);
                }
            }
        }

        throw new System.Exception("Path not found");
    }

}

