using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DFS : AlgoBase
{
    Stack<AlgoNode> _stack;
    // standardní iterativní DFS
    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        Stopwatch.Start();

        int c = 0;
        _stack = new Stack<AlgoNode>();

        _stack.Push(startNode);

        while (_stack.Count > 0)
        {
            await Task.Yield();
            AlgoNode currentNode = _stack.Pop();

            if (currentNode == endNode)
            {
                return await GetResultPath(startNode, currentNode);
            }

            if (!currentNode.Visited)
            {
                currentNode.Visited = true;
                VisitedNodes++;

                drawingNode.DrawNode(currentNode);

                foreach (var neighbor in currentNode.Neighbours)
                {
                    if (!neighbor.Visited)
                    {
                        _stack.Push(neighbor);
                        MemoryUsage = Mathf.Max(MemoryUsage, _stack.Count);
                        neighbor.Parent = currentNode;
                    }
                }

            }
        }

        throw new System.Exception("Path not found");
    }
   
}


