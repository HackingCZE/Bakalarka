using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SimpleVisualizer;

public class AlgoNode
{
    public Vector3 Position { get; set; }
    public List<AlgoNode> Neighbours { get; set; } = new List<AlgoNode>();
    public AlgoNode Parent { get; set; }

    public float Value { get; set; } = float.PositiveInfinity;
    public float GScore { get; set; } = float.PositiveInfinity;
    public float FScore { get; set; } = float.PositiveInfinity;
    public float Cost { get; set; }
    public bool Visited { get; set; } = false;
    public RoadTileType Type { get; set; }

 

    public AlgoNode(Vector3 position)
    {
        Position = position;
    }

    public AlgoNode(Vector3 position, AlgoNode parent)
    {
        Position = position;
        Parent = parent;
    }

    public AlgoNode(Vector3 position, AlgoNode parent, RoadTileType type)
    {
        Position = position;
        Parent = parent;
        Type = type;
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

        foreach (AlgoNode node in nodes)
        {
            float distance = Vector3.Distance(node.Position, targetPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

}