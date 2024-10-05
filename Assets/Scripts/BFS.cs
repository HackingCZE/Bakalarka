using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class BFS
{
    Queue<AlgoNode> queue;
    public async Task<List<AlgoNode>> StartBFS(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        int c = 0;

        queue = new Queue<AlgoNode>();

        queue.Enqueue(startNode);
        startNode.Visited = true;

        while (queue.Count > 0)
        {
            await Task.Delay(5);

            AlgoNode currentNode = queue.Dequeue();
            drawingNode.DrawNode(currentNode);

            if (currentNode == endNode)
            {
                Debug.Log("Getting path");
                var path = NodeUtility.ReconstructPath(startNode, currentNode);
                return await Task.FromResult(path);
            }

            foreach (var neighbor in currentNode.Neighbors)
            {
                if (!neighbor.Visited)
                {
                    queue.Enqueue(neighbor);
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
