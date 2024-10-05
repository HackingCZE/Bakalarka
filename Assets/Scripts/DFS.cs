using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DFS : MonoBehaviour
{
    Stack<AlgoNode> stack;
    public async Task<List<AlgoNode>> StartDFS(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        int c = 0;
        stack = new Stack<AlgoNode>();

        stack.Push(startNode);

        while (stack.Count > 0)
        {
            await Task.Delay(1);
            AlgoNode currentNode = stack.Pop();

            if (currentNode == endNode){
                Debug.Log("Getting path");
                var path = NodeUtility.ReconstructPath(startNode, currentNode);
                return await Task.FromResult(path);
            }

            if (!currentNode.Visited)
            {
                currentNode.Visited = true;
                drawingNode.DrawNode(currentNode);

                for (int i = 0; i < currentNode.Neighbors.Count; i++)
                {
                    var neighbor = currentNode.Neighbors[i];

                    if (!neighbor.Visited)
                    {
                        stack.Push(neighbor);
                        neighbor.Parent = currentNode;
                    }
                }

            }
            if (c > graph.Count + 150) break;
            c++;
        }

        throw new System.Exception("Path not found");
    }
}


