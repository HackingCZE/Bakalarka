using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public interface IRRTAlgorithm
{
    public async Task<TreeCollection> Interation(float3 start, float3 end, int maxIterations, float maxStepLength, GenerationArea area, float threshold, LayerMask barrierLayer, IDrawingNode drawingNode) { return null; }
    public Task<bool> Next(float3 end, float maxStepLength, int count, float threshold);

    public static bool NotInCollision(TreeCollectionItem newNode, TreeCollectionItem parentNode, LayerMask barrierLayer)
    {
        if(LineIntersectsObstacle(newNode.Position, parentNode.Position, barrierLayer))
        {
            return false;
        }
        return true;
    }
    public static bool LineIntersectsObstacle(float3 position1, float3 position2, LayerMask barrierLayer)
    {
        Vector3 direction = ((Vector3)(position2 - position1)).normalized;
        float distance = Vector3.Distance(position1, position2);
        if(Physics.Raycast(position1, direction, out RaycastHit hit, distance, barrierLayer))
        {
            if(hit.collider)
            {
                return true;
            }
        }
        return false;
    }

    public static float3 SampleRandomPoint(TreeCollectionItem node)
    {
        return new float3(UnityEngine.Random.Range(node.Position.x - 10, node.Position.x + 10), UnityEngine.Random.Range(node.Position.y - 10, node.Position.y + 10), UnityEngine.Random.Range(node.Position.z - 10, node.Position.z + 10));
    }
    public static float3 SampleRandomPoint(GenerationArea area)
    {
        float x = UnityEngine.Random.Range(area.GetMinX(), area.GetMaxX());
        float y = UnityEngine.Random.Range(area.GetMinY(), area.GetMaxY());
        float z = UnityEngine.Random.Range(area.GetMinZ(), area.GetMaxZ());
        return new float3(x, y, z);
    }


    public static TreeCollectionItem Steer(TreeCollectionItem fromNode, float3 toPoint, float maxStepLength)
    {
        Vector3 newPoint = Vector3.zero;
        var direction = toPoint - (float3)fromNode.Position;
        var distance = Length(direction);

        if(distance > maxStepLength)
        {
            direction = ((Vector3)direction).normalized;
            newPoint = (float3)fromNode.Position + direction * maxStepLength;
        }
        else
        {
            newPoint = toPoint;
        }


        return new TreeCollectionItem(newPoint);
    }



    public static float Length(float3 vector)
    {
        return Mathf.Sqrt(Mathf.Pow(vector.x, 2) + Mathf.Pow(vector.y, 2) + Mathf.Pow(vector.z, 2));
    }
}

public interface IDrawingNode
{
    public void DrawNode() { }
    public void DrawNode(AlgoNode node) { }
}