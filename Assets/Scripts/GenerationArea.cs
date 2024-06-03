using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationArea : MonoBehaviour
{
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;
    [SerializeField] float scale;
    Vector3 leftUp1, rightUp1, leftDown1, rightDown1;
    Vector3 leftUp2, rightUp2, leftDown2, rightDown2;
    List<Vector3> points;
    private void Awake()
    {
        Vector3 p1 = startPoint.position;

        leftDown1 = new Vector3(p1.x, p1.y - scale, p1.z - scale);
        rightDown1 = new Vector3(p1.x, p1.y + scale, p1.z - scale);
        rightUp1 = new Vector3(p1.x, p1.y + scale, p1.z + scale);
        leftUp1 = new Vector3(p1.x, p1.y - scale, p1.z + scale);

        Vector3 p2 = endPoint.position;

        leftDown2 = new Vector3(p2.x, p2.y - scale, p2.z - scale);
        rightDown2 = new Vector3(p2.x, p2.y + scale, p2.z - scale);
        rightUp2 = new Vector3(p2.x, p2.y + scale, p2.z + scale);
        leftUp2 = new Vector3(p2.x, p2.y - scale, p2.z + scale);


        points = new List<Vector3>
        {
            leftDown1,
            rightDown1,
            rightUp1,
            leftUp1,

            leftDown2,
            rightDown2,
            rightUp2,
            leftUp2,
        };

    }

    public float GetMinX()
    {
        float min = points[0].x;
        foreach (var item in points)
        {
            min = Mathf.Min(min, item.x); 
        }
        return min;
    }

    public float GetMaxX()
    {
        float max = points[0].x;
        foreach (var item in points)
        {
            max = Mathf.Max(max, item.x);
        }
        return max;
    }

    public float GetMinY()
    {
        float min = points[0].y;
        foreach (var item in points)
        {
            min = Mathf.Min(min, item.y);
        }
        return min;
    }

    public float GetMaxY()
    {
        float max = points[0].y;
        foreach (var item in points)
        {
            max = Mathf.Max(max, item.y);
        }
        return max;
    }

    public float GetMinZ()
    {
        float min = points[0].z;
        foreach (var item in points)
        {
            min = Mathf.Min(min, item.z);
        }
        return min;
    }

    public float GetMaxZ()
    {
        float max = points[0].z;
        foreach (var item in points)
        {
            max = Mathf.Max(max, item.z);
        }
        return max;
    }

    private void OnDrawGizmos()
    {
        Vector3 p1 = startPoint.position;

        leftDown1 = new Vector3(p1.x, p1.y - scale, p1.z - scale);
        rightDown1 = new Vector3(p1.x, p1.y + scale, p1.z - scale);
        rightUp1 = new Vector3(p1.x, p1.y + scale, p1.z + scale);
        leftUp1 = new Vector3(p1.x, p1.y - scale, p1.z + scale);

        Vector3 p2 = endPoint.position;

        leftDown2 = new Vector3(p2.x, p2.y - scale, p2.z - scale);
        rightDown2 = new Vector3(p2.x, p2.y + scale, p2.z - scale);
        rightUp2 = new Vector3(p2.x, p2.y + scale, p2.z + scale);
        leftUp2 = new Vector3(p2.x, p2.y - scale, p2.z + scale);
        Gizmos.color = Color.black; 

        Gizmos.DrawLine(leftDown1, rightDown1);
        Gizmos.DrawLine(rightDown1, rightUp1);
        Gizmos.DrawLine(rightUp1, leftUp1);
        Gizmos.DrawLine(leftUp1, leftDown1);

        Gizmos.DrawLine(leftDown2, rightDown2);
        Gizmos.DrawLine(rightDown2, rightUp2);
        Gizmos.DrawLine(rightUp2, leftUp2);
        Gizmos.DrawLine(leftUp2, leftDown2);

        Gizmos.DrawLine(leftDown1, leftDown2);
        Gizmos.DrawLine(rightDown1, rightDown2);
        Gizmos.DrawLine(rightUp1, rightUp2);
        Gizmos.DrawLine(leftUp1, leftUp2);
    }
}
