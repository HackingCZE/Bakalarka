using DH.Save;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class RRT : IRRTAlgorithm
{
    public TreeCollection treeCollection = new TreeCollection();
    GenerationArea area;
    public async Task Interation(Vector3 start, Vector3 end, int maxIterations, int maxStepLength, GenerationArea area, float threshold)
    {
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
    }

    public async Task<bool> Next(float3 end, int maxStepLength, int count, float threshold)
    {
        for (int i = 0; i < count; i++)
        {
            if (false) await Task.Delay(5);
            float3 randomPoint = IRRTAlgorithm.SampleRandomPoint(area);
            TreeCollectionItem neareastNode = treeCollection.KDTree.NearestNeighbor(randomPoint).Item.treeCollectionItem;
            var newNode = IRRTAlgorithm.Steer(neareastNode, randomPoint, maxStepLength);
            if (IRRTAlgorithm.NotInCollision(neareastNode, newNode))
            {
                var lastNode = treeCollection.AddNode(newNode, neareastNode);
                GameManager.Instance.DrawNode();                
                if (Vector3.Distance(lastNode.Position, end) < (threshold + 5))
                {
                    GameManager.Instance.DrawPath(treeCollection.ReconstructPath(lastNode));
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


}
