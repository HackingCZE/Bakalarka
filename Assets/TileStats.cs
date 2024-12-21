using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering.Universal.Internal;
using static SimpleVisualizer;

public class TileStats : MonoBehaviour
{
    public List<Transform> points = new();

    public List<BuildingPoint> GetBuildingPoints()
    {
        var result = new List<BuildingPoint>();
        foreach (Transform t in points)
        {
            result.Add(new BuildingPoint(t.position, t.transform.right, t.transform.localEulerAngles.y));
            //Debug.Log(t.transform.localEulerAngles.y);
        }
        return result;
    }
}

public class GroupingPoints
{
    public float maxDistance = 1f;
    public float maxDistanceMore = 1.7f;


    public List<List<BuildingPoint>> GroupPoints(List<BuildingPoint> allPoints)
    {
        List<List<BuildingPoint>> groups = new List<List<BuildingPoint>>();

        foreach (BuildingPoint point in allPoints)
        {
            if (!point.Visited)
            {
                // Vytvo�en� nov� skupiny a zah�jen� vyhled�v�n�
                List<BuildingPoint> group = new List<BuildingPoint>();
                Search(point, allPoints,  group);
                groups.Add(group);
            }
        }

        return groups;
    }

    public void Search(BuildingPoint startPoint, List<BuildingPoint> allPoints,  List<BuildingPoint> currentGroup)
    {
        // Vytvo�en� fronty pro BFS (���kov� prohled�v�n�)
        Queue<BuildingPoint> queue = new Queue<BuildingPoint>();
        queue.Enqueue(startPoint);
        startPoint.Visited = true; // Ozna��me bod jako nav�t�ven�
        currentGroup.Add(startPoint);

        while (queue.Count > 0)
        {
            BuildingPoint currentPoint = queue.Dequeue();

            foreach (BuildingPoint point in allPoints)
            {
                if (!point.Visited)
                {
                    // Podm�nky na RotationY a Direction 
                    bool isRotationMatching = false;
                    bool isDirectionMatching = false;

                    if(currentGroup.Count > 0 && currentGroup[0].RotationY == 225 && point.RotationY == 180)
                    {
                        Debug.Log("now");
                    }

                    switch (currentPoint.RotationY)
                    {
                        case 0:
                        case 180:
                        case 90:
                        case 270:
                            isRotationMatching = false;
                            if (Vector3.Distance(point.Position, currentPoint.Position) <= maxDistance)
                            {
                                // Kontrola, zda RotationY odpov�d� po�adovan�m hodnot�m
                               // isRotationMatching = (point.RotationY != 0 && point.RotationY != 180 && point.RotationY != 90 && point.RotationY != 270);

                            }
                            //if (!isRotationMatching)
                            //{
                            //    if (Vector3.Distance(point.Position, currentPoint.Position) <= maxDistanceMore)
                            //    {
                            //        Vector3 directionToCheck = (point.Position - currentPoint.Position).normalized;
                            //        // Podm�nky pro porovn�n� sm�ru
                            //        if (currentPoint.Direction == new Vector3(1, 0, 0) || currentPoint.Direction == new Vector3(-1, 0, 0)) // currentPoint sm�rem doprava (1, 0, 0)
                            //        {
                            //            // Hled�me body s direction (0, 0, -1) nebo (0, 0, 1)
                            //            isDirectionMatching = directionToCheck == new Vector3(0, 0, -1) || directionToCheck == new Vector3(0, 0, 1);
                            //        }
                            //        else if (currentPoint.Direction == new Vector3(0, 0, 1) || currentPoint.Direction == new Vector3(0, 0, -1)) // currentPoint sm�rem dol� (0, 0, -1)
                            //        {
                            //            // Hled�me body s direction (1, 0, 0) nebo (-1, 0, 0)
                            //            isDirectionMatching = directionToCheck == new Vector3(1, 0, 0) || directionToCheck == new Vector3(-1, 0, 0);
                            //        }
                            //    }
                            //}
                            break;
                        default: // Obr�cen� kontrola pro opa�n� rotace
                                 // Kontrola vzd�lenosti
                            if (Vector3.Distance(point.Position, currentPoint.Position) <= maxDistance)
                            {
                                isRotationMatching = point.RotationY == 0 ||
                                                 point.RotationY == 180 ||
                                                 point.RotationY == 270 ||
                                                 point.RotationY == 90;
                            }

                            break;
                    }

                    // Pokud RotationY odpov�d� a sm�r je spr�vn�, p�idej bod do skupiny
                    if (isRotationMatching)
                    {
                        Debug.Log("X" + currentPoint.RotationY + " Y" + point.RotationY);

                        point.Visited = true; // Ozna� bod jako nav�t�ven�
                        currentGroup.Add(point);
                        queue.Enqueue(point); // P�idej bod do fronty pro dal�� zpracov�n�
                    }

                }
            }
        }

    }
}