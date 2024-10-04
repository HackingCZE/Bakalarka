using DH.Save.SerializableTypes;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class TreeCollection
{
    public TreeCollectionItem root;
    public TreeCollectionItem lastNode;
    public KDTree KDTree;

    public TreeCollection() { }
    public void Init(float3 startPoint)
    {
        root = new TreeCollectionItem(startPoint);
        KDTree = new KDTree(startPoint, root);
        lastNode = root;
    }
    public TreeCollectionItem AddNode(float3 point, TreeCollectionItem parent)
    {
        TreeCollectionItem newNode = new TreeCollectionItem(point, parent);
        parent.AddChild(newNode);
        lastNode = newNode;
        return newNode;
    }

    public TreeCollectionItem AddNode(TreeCollectionItem node, TreeCollectionItem parent)
    {
        node.Parent = parent;
        parent.AddChild(node);
        KDTree.Insert(node);
        lastNode = node;
        return node;
    }

    public TreeCollectionItem FindNearestNode(float3 endPoint)
    {
        TreeCollectionItem nearestItem = root;
        CheckNode(out nearestItem, nearestItem, root, endPoint);
        return nearestItem;
    }


    private void CheckNode(out TreeCollectionItem nearestItem, TreeCollectionItem nearestNode, TreeCollectionItem node, float3 endPoint)
    {
        nearestItem = nearestNode;
        if (node == null) return;
        foreach (var child in node.Children)
        {
            CheckNode(out nearestItem, nearestItem, child, endPoint);
            if (Vector3.Distance(child.Position, endPoint) < Vector3.Distance(nearestItem.Position, endPoint))
            {
                nearestItem = child;
            }
        }
    }

    public List<TreeCollectionItem> FindNeighborNodes(float3 newPoint, float searchRadius = 10)
    {
        List<TreeCollectionItem> nearestItems = new List<TreeCollectionItem>() { root };
        CheckNode(out nearestItems, nearestItems, root, newPoint, searchRadius);
        return nearestItems;
    }

    private void CheckNode(out List<TreeCollectionItem> nearestItems, List<TreeCollectionItem> nearestNodes, TreeCollectionItem node, float3 newPoint, float searchRadius)
    {
        if (nearestNodes == null) nearestNodes = new List<TreeCollectionItem>();
        nearestItems = nearestNodes;
        if (node == null) return;
        foreach (var child in node.Children)
        {
            CheckNode(out nearestItems, nearestNodes, child, newPoint, searchRadius);
            if (Vector3.Distance(child.Position, newPoint) < searchRadius)
            {
                if (!nearestItems.Contains(child)) nearestItems.Add(child);
            }
        }
    }

    public List<TreeCollectionItem> recontructedPath = new List<TreeCollectionItem>();
    public List<TreeCollectionItem> ReconstructPath(TreeCollectionItem endNode)
    {
        recontructedPath = new List<TreeCollectionItem>();
        TreeCollectionItem currentNode = endNode;
        while (currentNode != null)
        {
            recontructedPath.Insert(0, currentNode);
            currentNode = currentNode.Parent;
        }

        return recontructedPath;
    }

}
[Serializable]
public class TreeCollectionItem
{
    public Vector3 Position;
     public TreeCollectionItem Parent;
    [JsonIgnore]public List<TreeCollectionItem> Children;
    public TreeCollectionItem() { }
    public TreeCollectionItem(float3 position, TreeCollectionItem parent = null)
    {
        this.Position = (Vector3)position;
        this.Parent = parent;
        if (Children == null) Children = new List<TreeCollectionItem>();
    }

    public void AddChild(TreeCollectionItem child)
    {
        Children.Add(child);
        child.Parent = this;
    }


}