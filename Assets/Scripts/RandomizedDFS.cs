using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RandomizedDFS : AlgoBase
{
    Stack<AlgoNode> _stack;
    private System.Random _random;

    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        _random = new System.Random();
        Stopwatch.Start();

        int c = 0;
        _stack = new Stack<AlgoNode>();

        _stack.Push(startNode);

        while(_stack.Count > 0)
        {
            await Task.Yield();
            AlgoNode currentNode = _stack.Pop();

            if(currentNode == endNode)
            {
                return await GetResultPath(startNode, currentNode);
            }

            if(!currentNode.Visited)
            {
                currentNode.Visited = true;
                VisitedNodes++;

                drawingNode.DrawNode(currentNode);

                // shuffle
                var shuffledNeighbors = currentNode.Neighbours.OrderBy(_ => _random.Next()).ToList();

                foreach(var neighbor in shuffledNeighbors)
                {
                    if(!neighbor.Visited)
                    {
                        _stack.Push(neighbor);
                        MemoryUsage = Mathf.Max(MemoryUsage, _stack.Count);
                        neighbor.Parent = currentNode;
                    }
                }
            }

            if(c > graph.Count + 150) break;
            c++;
        }

        throw new System.Exception("Path not found");
    }

}
