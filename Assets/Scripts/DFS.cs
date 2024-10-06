using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DFS : AlgoBase
{
    Stack<AlgoNode> _stack;

    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        int c = 0;
        _stack = new Stack<AlgoNode>();

        _stack.Push(startNode);

        while (_stack.Count > 0)
        {
            await Task.Delay(1);
            AlgoNode currentNode = _stack.Pop();

            if (currentNode == endNode)
            {
                return await GetResultPath(startNode, currentNode);
            }

            if (!currentNode.Visited)
            {
                currentNode.Visited = true;
                drawingNode.DrawNode(currentNode);

                foreach (var neighbor in currentNode.Neighbors)
                {
                    if (!neighbor.Visited)
                    {
                        _stack.Push(neighbor);
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


