using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class DLS : AlgoBase
{
    private Stack<AlgoNode> _stack;
    private int _depthLimit = 25;


    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        Stopwatch.Start();

        int c = 0;
        _stack = new Stack<AlgoNode>();
        startNode.Cost = 0; 
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

                if (currentNode.Cost < _depthLimit)
                {
                    foreach (var neighbor in currentNode.Neighbours)
                    {
                        if (!neighbor.Visited)
                        {
                            neighbor.Cost = currentNode.Cost + 1; // add depth
                            _stack.Push(neighbor);
                            MemoryUsage = Mathf.Max(MemoryUsage, _stack.Count);
                            neighbor.Parent = currentNode;
                        }
                    }
                }
            }

            if (c > graph.Count + 150) break;
            c++;
        }

        throw new System.Exception("Path not found");
    }
}
