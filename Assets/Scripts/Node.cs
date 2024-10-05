using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoNode
{
    public Vector3 Position { get; set; }
    public List<AlgoNode> Neighbors { get; set; }
    public AlgoNode Parent { get; set; }

    public float Value { get; set; }
    public float Cost { get; set; }
    public bool Visited { get; set; }

    public AlgoNode()
    {
        Visited = false;
    }

    public AlgoNode(Vector3 position)
    {
        Position = position;

        Visited = false;
    }

    public AlgoNode(Vector3 position, AlgoNode parent)
    {
        Visited = false;

        Position = position;
        Parent = parent;
        Neighbors = new List<AlgoNode>();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        AlgoNode other = (AlgoNode)obj;
        return Position.Equals(other.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}

public static class NodeUtility
{
    public static List<AlgoNode> ReconstructPath(AlgoNode startNode, AlgoNode endNode)
    {
        var recontructedPath = new List<AlgoNode>();
        AlgoNode currentNode = endNode;
        while (currentNode != null)
        {
            recontructedPath.Insert(0, currentNode);
            if (currentNode == startNode) break;
            currentNode = currentNode.Parent;
        }

        return recontructedPath;
    }

    public static AlgoNode FindClosestNode(List<AlgoNode> nodes, Vector3 targetPosition)
    {
        AlgoNode closestNode = null;
        float closestDistance = float.MaxValue;

        // Iterace pøes všechny uzly
        foreach (AlgoNode node in nodes)
        {
            // Vypoèítáme vzdálenost mezi cílovou pozicí a aktuálním uzlem
            float distance = Vector3.Distance(node.Position, targetPosition);

            // Pokud je vzdálenost menší než dosud nalezená, uložíme tento uzel jako nejbližší
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

}