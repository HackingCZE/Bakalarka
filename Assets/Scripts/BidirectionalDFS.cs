using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class BidirectionalDFS : BiderectionalAlgoBase
{
    private Stack<AlgoNode> _forwardStack;
    private Stack<AlgoNode> _backwardStack;

    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        Stopwatch.Start();
        _forwardStack = new Stack<AlgoNode>();
        _backwardStack = new Stack<AlgoNode>();

        _forwardParents = new Dictionary<AlgoNode, AlgoNode>();
        _backwardParents = new Dictionary<AlgoNode, AlgoNode>();

        _forwardStack.Push(startNode);
        _backwardStack.Push(endNode);

        _forwardParents[startNode] = null;
        _backwardParents[endNode] = null;

        VisitedNodes = 2;

        while(_forwardStack.Count > 0 && _backwardStack.Count > 0)
        {
            await Task.Yield();

            // Forward search step
            if(Step(_forwardStack, _forwardParents, _backwardParents, drawingNode, true))
            {
                return await GetResultPath(startNode, endNode);
            }

            // Backward search step
            if(Step(_backwardStack, _backwardParents, _forwardParents, drawingNode, false))
            {
                return await GetResultPath(startNode, endNode);
            }

            // Update memory usage
            MemoryUsage = Mathf.Max(MemoryUsage, _forwardStack.Count + _backwardStack.Count);
        }

        throw new System.Exception("Path not found");
    }

    private bool Step(Stack<AlgoNode> stack, Dictionary<AlgoNode, AlgoNode> currentParents, Dictionary<AlgoNode, AlgoNode> otherParents, IDrawingNode drawingNode, bool isForward)
    {
        if(stack.Count == 0) return false;

        AlgoNode currentNode = stack.Pop();
        currentNode.Visited = true;
        Visited.Add(currentNode.Position);

        drawingNode.DrawNode(currentNode);

        foreach(var neighbor in currentNode.Neighbours)
        {
            if(!currentParents.ContainsKey(neighbor))
            {
                currentParents[neighbor] = currentNode;

                if(!stack.Contains(neighbor))
                {
                    stack.Push(neighbor);
                }

                if(otherParents.ContainsKey(neighbor))
                {
                    return true; // Meeting point found
                }
            }
        }

        return false;
    }

}
