using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class RRTStar : IRRTAlgorithm 
{
    public LayerMask barrierLayer;
    public IDrawingNode drawingNode;

    public TreeCollection treeCollection = new TreeCollection();
    GenerationArea area;
    public async Task<TreeCollection> Interation(Vector3 start, Vector3 end, int maxIterations, float maxStepLength, GenerationArea area, float threshold, LayerMask barrierLayer, IDrawingNode drawingNode)
    {
        this.barrierLayer = barrierLayer;
        this.drawingNode = drawingNode;

        this.area = area;
        treeCollection.Init(start);
        for (int i = 0; i < maxIterations; i++)
        {
            if (false) await Task.Delay(5);
            if (!(await Next(end, maxStepLength, 1, threshold)))
            {
                break;
            }
        }
        return treeCollection;
    }

    public async Task<bool> Next(float3 end, float maxStepLength, int count, float threshold)
    {
        for (int i = 0; i < count; i++)
        {
            if (true) await Task.Delay(1);
            float3 randomPoint = IRRTAlgorithm.SampleRandomPoint(area);
            randomPoint.y = treeCollection.root.Position.y;
            TreeCollectionItem neareastNode = treeCollection.KDTree.NearestNeighbor(randomPoint).Item.treeCollectionItem;
            var newNode = IRRTAlgorithm.Steer(neareastNode, randomPoint, maxStepLength);
            if (IRRTAlgorithm.NotInCollision(neareastNode, newNode, barrierLayer))
            {
                var neighborNodes = treeCollection.KDTree.FindNeighborsWithinRadius(newNode.Position);
                var bestNode = neareastNode;
                var bestCost = CalculateCost(neareastNode) + Vector3.Distance(neareastNode.Position, newNode.Position);
                foreach (var neighbor in neighborNodes)
                {
                    if (IRRTAlgorithm.NotInCollision(neighbor.treeCollectionItem, newNode, barrierLayer))
                    {
                        if (CalculateCost(neighbor.treeCollectionItem) + Vector3.Distance(neighbor.treeCollectionItem.Position, newNode.Position) < bestCost)
                        {
                            bestNode = neighbor.treeCollectionItem;
                            bestCost = CalculateCost(neighbor.treeCollectionItem) + Vector3.Distance(neighbor.treeCollectionItem.Position, newNode.Position);
                        }
                    }
                }
                var lastNode = treeCollection.AddNode(newNode, bestNode);
                drawingNode.DrawNode();
                foreach (var neighbor in neighborNodes)
                {
                    if (IRRTAlgorithm.NotInCollision(neighbor.treeCollectionItem, newNode, barrierLayer))
                    {
                        if (CalculateCost(newNode) + Vector3.Distance(newNode.Position, neighbor.treeCollectionItem.Position) < CalculateCost(neighbor.treeCollectionItem))
                        {
                            neighbor.treeCollectionItem.Parent = newNode;
                        }
                    }
                }
                if (Vector3.Distance(lastNode.Position, end) < (threshold + 5))
                {
                    treeCollection.ReconstructPath(lastNode);
                    //GameManager.Instance.DrawPath(treeCollection.ReconstructPath(lastNode));
                    return false;
                }
            }
            else
            {
                i--;
            }
        }
        return true;
    }

    private float CalculateCost(TreeCollectionItem node)
    {
        float cost = 0;
        while (node.Parent != null)
        {
            cost += Vector3.Distance(node.Position, node.Parent.Position);
            node = node.Parent;
        }
        return cost;
    }

}

