using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class RRT : IRRTAlgorithm
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
        for(int i = 0; i < maxIterations; i++)
        {
            if(false) await Task.Delay(5);
            if(!(await Next(end, maxStepLength, 1, threshold)))
            {
                break;
            }
        }
        return treeCollection;
    }

    public async Task<bool> Next(float3 end, float maxStepLength, int count, float threshold)
    {
        for(int i = 0; i < count; i++)
        {
            if(true) await Task.Delay(5);
            float3 randomPoint = IRRTAlgorithm.SampleRandomPoint(area);
            randomPoint.y = treeCollection.root.Position.y;
            TreeCollectionItem neareastNode = treeCollection.KDTree.NearestNeighbor(randomPoint).Item.treeCollectionItem;
            var newNode = IRRTAlgorithm.Steer(neareastNode, randomPoint, maxStepLength);
            if(IRRTAlgorithm.NotInCollision(neareastNode, newNode, barrierLayer))
            {
                var lastNode = treeCollection.AddNode(newNode, neareastNode);
                drawingNode.DrawNode();
                //GameManager.Instance.DrawNode();                
                if(Vector3.Distance(lastNode.Position, end) < (threshold + 5))
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


}

