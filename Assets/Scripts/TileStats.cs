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
                // Vytvoøení nové skupiny a zahájení vyhledávání
                List<BuildingPoint> group = new List<BuildingPoint>();
                Search(point, allPoints,  group);
                groups.Add(group);
            }
        }

        return groups;
    }

    public void Search(BuildingPoint startPoint, List<BuildingPoint> allPoints,  List<BuildingPoint> currentGroup)
    {
        // Vytvoøení fronty pro BFS (šíøkové prohledávání)
        Queue<BuildingPoint> queue = new Queue<BuildingPoint>();
        queue.Enqueue(startPoint);
        startPoint.Visited = true; // Oznaèíme bod jako navštívený
        currentGroup.Add(startPoint);

        while (queue.Count > 0)
        {
            BuildingPoint currentPoint = queue.Dequeue();

            foreach (BuildingPoint point in allPoints)
            {
                if (!point.Visited)
                {
                    // Podmínky na RotationY a Direction 
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
                                // Kontrola, zda RotationY odpovídá požadovaným hodnotám
                               // isRotationMatching = (point.RotationY != 0 && point.RotationY != 180 && point.RotationY != 90 && point.RotationY != 270);

                            }
                            //if (!isRotationMatching)
                            //{
                            //    if (Vector3.Distance(point.Position, currentPoint.Position) <= maxDistanceMore)
                            //    {
                            //        Vector3 directionToCheck = (point.Position - currentPoint.Position).normalized;
                            //        // Podmínky pro porovnání smìru
                            //        if (currentPoint.Direction == new Vector3(1, 0, 0) || currentPoint.Direction == new Vector3(-1, 0, 0)) // currentPoint smìrem doprava (1, 0, 0)
                            //        {
                            //            // Hledáme body s direction (0, 0, -1) nebo (0, 0, 1)
                            //            isDirectionMatching = directionToCheck == new Vector3(0, 0, -1) || directionToCheck == new Vector3(0, 0, 1);
                            //        }
                            //        else if (currentPoint.Direction == new Vector3(0, 0, 1) || currentPoint.Direction == new Vector3(0, 0, -1)) // currentPoint smìrem dolù (0, 0, -1)
                            //        {
                            //            // Hledáme body s direction (1, 0, 0) nebo (-1, 0, 0)
                            //            isDirectionMatching = directionToCheck == new Vector3(1, 0, 0) || directionToCheck == new Vector3(-1, 0, 0);
                            //        }
                            //    }
                            //}
                            break;
                        default: // Obrácená kontrola pro opaèné rotace
                                 // Kontrola vzdálenosti
                            if (Vector3.Distance(point.Position, currentPoint.Position) <= maxDistance)
                            {
                                isRotationMatching = point.RotationY == 0 ||
                                                 point.RotationY == 180 ||
                                                 point.RotationY == 270 ||
                                                 point.RotationY == 90;
                            }

                            break;
                    }

                    // Pokud RotationY odpovídá a smìr je správný, pøidej bod do skupiny
                    if (isRotationMatching)
                    {
                        Debug.Log("X" + currentPoint.RotationY + " Y" + point.RotationY);

                        point.Visited = true; // Oznaè bod jako navštívený
                        currentGroup.Add(point);
                        queue.Enqueue(point); // Pøidej bod do fronty pro další zpracování
                    }

                }
            }
        }

    }
}