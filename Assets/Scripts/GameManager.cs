using DH.Save;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour, IDrawingNode
{
    public static GameManager Instance;
    [SerializeField] Transform startPostion;
    [SerializeField] Transform endPostion;
    [SerializeField] int maxIterations;
    [SerializeField] int maxStepLength;
    [SerializeField] int threshold;
    [SerializeField] GenerationArea area;
    [SerializeField] List<TreeCollectionItem> nodes = new List<TreeCollectionItem>();
    [SerializeField] List<Line> lines = new List<Line>();
    [SerializeField] List<TreeCollectionItem> path = new List<TreeCollectionItem>();
    [SerializeField] Algo algo;
    [Space(30),SerializeField] int nextCount;
    [SerializeField] private float elapsedTime = 0f; // Uloží uplynulý èas
    bool timerIsRunning;
    IRRTAlgorithm rRT;

    public enum Algo
    {
        RRT,
        RRTStart
    }

    private void Awake()
    {
        Instance = this;
    }
    [ContextMenu("StartTree")]
    public async void StartTree()
    {
        rRT = (algo == Algo.RRT ? new RRT() : new RRTStar());
        nodes = new List<TreeCollectionItem>();
        lines = new List<Line>();
        path = new List<TreeCollectionItem>();
        timerIsRunning = true;
        await (algo == Algo.RRT ? ((RRT)rRT).Interation(startPostion.position, endPostion.position, maxIterations, maxStepLength, area, threshold, LayerMask.NameToLayer("Barrier"), this) : ((RRTStar)rRT).Interation(startPostion.position, endPostion.position, maxIterations, maxStepLength, area, threshold, LayerMask.NameToLayer("Barrier"), this));

        TryAddNewNode(algo == Algo.RRT ? ((RRT)rRT).treeCollection.root : ((RRTStar)rRT).treeCollection.root);
    }
    private void Update()
    {
        if (timerIsRunning)
        {
            // Pøidává uplynulý èas
            elapsedTime += Time.deltaTime;
        }
    }

    public void DrawPath(List<TreeCollectionItem> path)
    {
        this.path = path;
        timerIsRunning = false;
        SaveSystem.UpdateOrSave<List<TreeCollectionItem>>("path", path, true);
        //Debug.Log("END");
    }
    [ContextMenu("NextNodes")]
    public async void Next()
    {
        await rRT.Next(endPostion.position, maxStepLength, nextCount, threshold);
    }

    [ContextMenu("DrawNode")]
    public void DrawNode()
    {
        nodes = new List<TreeCollectionItem>(); 
        lines = new List<Line>();
        DrawNode(algo == Algo.RRT ? ((RRT)rRT).treeCollection.root : ((RRTStar)rRT).treeCollection.root);
    }
    private void DrawNode(TreeCollectionItem node)
    {
        if (node == null) return;
        foreach (var child in node.Children)
        {
            lines.Add(new Line(node.Position, child.Position));
            TryAddNewNode(child);
            DrawNode(child);
        }
    }

    private void TryAddNewNode(TreeCollectionItem newNode)
    {
        foreach (var item in nodes)
        {
            if (item == newNode)
            {
                Debug.LogError("Stejny node");
            }
        }
        nodes.Add(newNode);
    }

    private void OnDrawGizmos()
    {
        if(path == null || path.Count == 0)
        {
            Gizmos.DrawSphere(endPostion.position, 5f);
            Gizmos.DrawSphere(startPostion.position, 5f);
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            Gizmos.color = Color.red;
            if (i == nodes.Count - 1) Gizmos.color = Color.blue;
            Gizmos.DrawSphere(node.Position, .5f);
        }

        foreach (var item in lines)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(item.start, item.end);
        }

        for (int i = 0; i < path.Count; i++)
        {
            var node = path[i];
            Gizmos.color = Color.black;
            if (i == nodes.Count - 1) Gizmos.color = Color.blue;
            if (i == 0) Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(node.Position, .5f);
        }
    }
    [Serializable]
    public class Line
    {
        public Vector3 start;
        public Vector3 end;

        public Line(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
        }
    }
}
