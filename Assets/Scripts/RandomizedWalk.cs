using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RandomizedWalk : AlgoBase
{
    private System.Random _random;

    public async override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        _random = new System.Random();
        Stopwatch.Start();

        var currentNode = startNode;

        currentNode.Visited = true;
        drawingNode.DrawNode(currentNode);

        while (currentNode != endNode)
        {
            await Task.Yield();
           
            if (currentNode.Neighbours.Count == 0)
            {
                throw new System.Exception("Path not found");
            }

            var nextNode = currentNode.Neighbours[_random.Next(currentNode.Neighbours.Count)];

            if (!nextNode.Visited)
            {
                nextNode.Visited = true;
                VisitedNodes++;
                nextNode.Parent = currentNode;
                drawingNode.DrawNode(nextNode);
            }


            currentNode = nextNode; 
        }

        if (currentNode == endNode)
        {
            return await GetResultPath(startNode, currentNode);
        }

        throw new System.Exception("Path not found");
    }
}
