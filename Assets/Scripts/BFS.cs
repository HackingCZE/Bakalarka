using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class BFS : AlgoBase
{
    Queue<AlgoNode> _queue;

    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        int c = 0;
        _queue = new Queue<AlgoNode>();

        _queue.Enqueue(startNode);
        startNode.Visited = true;

        while (_queue.Count > 0)
        {
            await Task.Delay(1);
            AlgoNode currentNode = _queue.Dequeue();
            drawingNode.DrawNode(currentNode);

            if (currentNode == endNode)
            {
                return await GetResultPath(startNode, currentNode);
            }

            foreach (var neighbor in currentNode.Neighbors)
            {
                if (!neighbor.Visited)
                {
                    _queue.Enqueue(neighbor);
                    neighbor.Visited = true;
                    neighbor.Parent = currentNode;
                }
            }
            if (c > graph.Count + 150) break;
            c++;
        }

        throw new System.Exception("Path not found");
    }

}
