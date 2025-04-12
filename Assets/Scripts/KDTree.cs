using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KDTree
{
    public KDTreeItem root;
    public int k;
    public KDTree() { }
    public KDTree(Vector3 point, TreeCollectionItem rootNode, int k = 3)
    {
        this.k = k;
        this.root = new KDTreeItem(0 % k, rootNode);
    }

    public void Insert(TreeCollectionItem treeNode, KDTreeItem kdTreeNode = null, int depth = 0)
    {
        Vector3 point = treeNode.Position;
        if(kdTreeNode == null) kdTreeNode = root;

        if(kdTreeNode == null) root = new KDTreeItem(depth % k, treeNode);

        var splitDimension = depth % k;

        if(point[splitDimension] < kdTreeNode.treeCollectionItem.Position[splitDimension])
        {
            if(kdTreeNode.Left == null) kdTreeNode.Left = new KDTreeItem((depth + 1) % k, treeNode);
            else Insert(treeNode, kdTreeNode.Left, depth + 1);
        }
        else
        {
            if(kdTreeNode.Right == null) kdTreeNode.Right = new KDTreeItem((depth + 1) % k, treeNode);
            else Insert(treeNode, kdTreeNode.Right, depth + 1);
        }
    }

    public (KDTreeItem Item, float BestDistance) NearestNeighbor(Vector3 searchPoint, KDTreeItem best = null, KDTreeItem node = null, int depth = 0, float bestDistance = float.MaxValue)
    {
        if(best == null) best = root;
        if(node == null) node = root;

        var axis = depth % k;

        var currentDistance = Vector3.Distance(node.treeCollectionItem.Position, searchPoint);
        if(currentDistance < bestDistance)
        {
            best = node;
            bestDistance = currentDistance;
        }

        KDTreeItem nextNode = searchPoint[axis] < node.treeCollectionItem.Position[axis] ? node.Left : node.Right;
        KDTreeItem otherNode = searchPoint[axis] < node.treeCollectionItem.Position[axis] ? node.Right : node.Left;

        if(nextNode != null) (best, bestDistance) = NearestNeighbor(searchPoint, best, nextNode, depth + 1, bestDistance);

        if(Mathf.Abs(searchPoint[axis] - node.treeCollectionItem.Position[axis]) < bestDistance)
        {
            if(otherNode != null) (best, bestDistance) = NearestNeighbor(searchPoint, best, otherNode, depth + 1, bestDistance);
        }

        return (best, bestDistance);
    }

    public List<KDTreeItem> FindNeighborsWithinRadius(Vector3 searchPoint, float searchRadius = 10, KDTreeItem node = null, int depth = 0, float bestDistance = float.MaxValue)
    {
        if(node == null) node = root;
        List<KDTreeItem> neighborsWithinRadius = new List<KDTreeItem>();

        var axis = depth % k;

        var currentDistance = Vector3.Distance(node.treeCollectionItem.Position, searchPoint);
        if(currentDistance <= searchRadius)
        {
            neighborsWithinRadius.Add(node);
        }

        KDTreeItem nextNode = searchPoint[axis] < node.treeCollectionItem.Position[axis] ? node.Left : node.Right;
        KDTreeItem otherNode = searchPoint[axis] < node.treeCollectionItem.Position[axis] ? node.Right : node.Left;

        if(nextNode != null) neighborsWithinRadius.AddRange(FindNeighborsWithinRadius(searchPoint, searchRadius, nextNode, depth + 1));

        if(Mathf.Abs(searchPoint[axis] - node.treeCollectionItem.Position[axis]) <= searchRadius)
        {
            if(otherNode != null) neighborsWithinRadius.AddRange(FindNeighborsWithinRadius(searchPoint, searchRadius, otherNode, depth + 1));
        }

        return neighborsWithinRadius;
    }
}

[Serializable]
public class KDTreeItem
{
    public float SplitDimension;
    public KDTreeItem Left;
    public KDTreeItem Right;
    public TreeCollectionItem treeCollectionItem;
    public KDTreeItem() { }
    public KDTreeItem(float splitDimension, TreeCollectionItem treeCollectionItem, KDTreeItem left = null, KDTreeItem right = null)
    {
        SplitDimension = splitDimension;
        Left = left;
        Right = right;
        this.treeCollectionItem = treeCollectionItem;
    }
}
