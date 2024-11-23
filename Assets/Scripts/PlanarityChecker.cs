using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanarityChecker : MonoBehaviour
{
    public void StartPlanar(List<AlgoNode> graph)
    {
        bool isPlanar = CheckPlanarity(graph);

        // Výpis výsledku do konzole
        Debug.Log("Is the graph planar? " + isPlanar);
    }
    // Kontrola rovinnosti
    bool CheckPlanarity(List<AlgoNode> graph)
    {
        int nodeCount = graph.Count;
        int edgeCount = CountEdges(graph);

        // Eulerova podmínka
        if (edgeCount > 3 * nodeCount - 6)
        {
            return false;
        }

        // Geometrická kontrola pøekrývání hran
        if (CheckEdgeIntersections(graph))
        {
            return false;
        }

        return true;
    }

    // Spoèítání hran v grafu
    int CountEdges(List<AlgoNode> graph)
    {
        int edgeCount = 0;
        foreach (AlgoNode node in graph)
        {
            edgeCount += node.Neighbors.Count;
        }

        // Každá hrana je poèítána dvakrát (od obou uzlù)
        return edgeCount / 2;
    }

    // Kontrola, zda se nìjaké dvì hrany pøekrývají
    bool CheckEdgeIntersections(List<AlgoNode> graph)
    {
        List<(Vector2, Vector2)> edges = new List<(Vector2, Vector2)>();

        // Získání všech hran jako dvojic bodù
        foreach (AlgoNode node in graph)
        {
            foreach (AlgoNode neighbor in node.Neighbors)
            {
                // Zamezte duplicitnímu pøidání hran (každá hrana je oboustranná)
                if (!edges.Contains((neighbor.Position, node.Position)))
                {
                    edges.Add((node.Position, neighbor.Position));
                }
            }
        }

        // Kontrola prùseèíkù
        for (int i = 0; i < edges.Count; i++)
        {
            for (int j = i + 1; j < edges.Count; j++)
            {
                if (DoSegmentsIntersect(edges[i].Item1, edges[i].Item2, edges[j].Item1, edges[j].Item2))
                {
                    return true; // Našli jsme prùseèík
                }
            }
        }

        return false;
    }

    // Kontrola, zda se dva segmenty pøekrývají
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

    // Smìr k urèení relativní polohy bodù
    float Direction(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p3.x - p1.x) * (p2.y - p1.y) - (p2.x - p1.x) * (p3.y - p1.y);
    }
}
