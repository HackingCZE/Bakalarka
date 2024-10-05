using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Dijkstra
{
    PriorityQueue<AlgoNode> pq;
    public async Task<List<AlgoNode>> StartDijkstra(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        int c = 0;
        pq = new PriorityQueue<AlgoNode>();

        startNode.Parent = null;
        startNode.Value = 0; // Distance

        pq.Enqueue(startNode, 0);

        while (pq.Count > 0)
        {
            await Task.Delay(1);
            AlgoNode currentNode = pq.Dequeue();

            if (currentNode.Visited) continue;
            drawingNode.DrawNode(currentNode);

            if (currentNode == endNode)
            {
                Debug.Log("Getting path");
                var path = NodeUtility.ReconstructPath(startNode, currentNode);
                return await Task.FromResult(path);
            }

            currentNode.Visited = true;

            foreach (var neighbor in currentNode.Neighbors)
            {
                var edgeWeight = Vector3.Distance(currentNode.Position, neighbor.Position);

                var newDistance = currentNode.Value + edgeWeight;

                if (newDistance < neighbor.Value)
                {
                    neighbor.Value = newDistance;
                    neighbor.Parent = currentNode;
                    pq.Enqueue(neighbor, newDistance);
                }
            }
            if (c > graph.Count + 150) break;
            c++;
        }

        throw new System.Exception("Path not found");
    }
}

