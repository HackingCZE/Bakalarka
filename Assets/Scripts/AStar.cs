using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AStar : AlgoBase
{
    PriorityQueue<AlgoNode> _queue;

    public override async Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        int c = 0;
        _queue = new PriorityQueue<AlgoNode>();

        startNode.GScore = 0;
        startNode.FScore = Vector3.Distance(startNode.Position, endNode.Position);

        _queue.Enqueue(startNode, startNode.FScore);

        while (_queue.Count > 0)
        {
            await Task.Delay(1);

            AlgoNode currentNode = _queue.Dequeue();
            drawingNode.DrawNode(currentNode);

            if (currentNode == endNode)
            {
                return await GetResultPath(startNode, currentNode);
            }

            currentNode.Visited = true;

            foreach (var neighbor in currentNode.Neighbors)
            {
                if (neighbor.Visited) continue;

                var newGScore = currentNode.GScore + Vector3.Distance(currentNode.Position, neighbor.Position);

                if (newGScore < neighbor.GScore)
                {
                    neighbor.GScore = newGScore;
                    neighbor.FScore = neighbor.GScore + Vector3.Distance(neighbor.Position, endNode.Position);
                    neighbor.Parent = currentNode;

                    if (!_queue.Contains(neighbor))
                    {
                        _queue.Enqueue(neighbor, neighbor.FScore);
                    }
                }
            }

            if (c > graph.Count + 150) break;
            c++;
        }

        throw new System.Exception("Path not found");
    }
}
