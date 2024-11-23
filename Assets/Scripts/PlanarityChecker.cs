using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarityChecker : MonoBehaviour
{
    public void StartPlanar(List<AlgoNode> graph)
    {
        bool isPlanar = CheckPlanarity(graph);

        // V�pis v�sledku do konzole
        Debug.Log("Is the graph planar? " + isPlanar);
    }
    // Kontrola rovinnosti
    bool CheckPlanarity(List<AlgoNode> graph)
    {
        int nodeCount = graph.Count;
        int edgeCount = CountEdges(graph);

        // Eulerova podm�nka
        if (edgeCount > 3 * nodeCount - 6)
        {
            return false;
        }

        // Geometrick� kontrola p�ekr�v�n� hran
        if (CheckEdgeIntersections(graph))
        {
            return false;
        }

        return true;
    }

    // Spo��t�n� hran v grafu
    int CountEdges(List<AlgoNode> graph)
    {
        int edgeCount = 0;
        foreach (AlgoNode node in graph)
        {
            edgeCount += node.Neighbors.Count;
        }

        // Ka�d� hrana je po��t�na dvakr�t (od obou uzl�)
        return edgeCount / 2;
    }

    // Kontrola, zda se n�jak� dv� hrany p�ekr�vaj�
    bool CheckEdgeIntersections(List<AlgoNode> graph)
    {
        List<(Vector2, Vector2)> edges = new List<(Vector2, Vector2)>();

        // Z�sk�n� v�ech hran jako dvojic bod�
        foreach (AlgoNode node in graph)
        {
            foreach (AlgoNode neighbor in node.Neighbors)
            {
                // Zamezte duplicitn�mu p�id�n� hran (ka�d� hrana je oboustrann�)
                if (!edges.Contains((neighbor.Position, node.Position)))
                {
                    edges.Add((node.Position, neighbor.Position));
                }
            }
        }

        // Kontrola pr�se��k�
        for (int i = 0; i < edges.Count; i++)
        {
            for (int j = i + 1; j < edges.Count; j++)
            {
                if (DoSegmentsIntersect(edges[i].Item1, edges[i].Item2, edges[j].Item1, edges[j].Item2))
                {
                    return true; // Na�li jsme pr�se��k
                }
            }
        }

        return false;
    }

    // Kontrola, zda se dva segmenty p�ekr�vaj�
    bool DoSegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float d1 = Direction(c, d, a);
        float d2 = Direction(c, d, b);
        float d3 = Direction(a, b, c);
        float d4 = Direction(a, b, d);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
        {
            return true;
        }

        return false;
    }

    // Sm�r k ur�en� relativn� polohy bod�
    float Direction(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p3.x - p1.x) * (p2.y - p1.y) - (p2.x - p1.x) * (p3.y - p1.y);
    }
}
