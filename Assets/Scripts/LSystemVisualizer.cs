using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static SimpleVisualizer;

public class LSystemVisualizer : MonoBehaviour
{
    public static LSystemVisualizer Instance { get; private set; }

    public LSystemGenerator lSystemGenerator;

    public float _chanceToJoin = 80;

    // list
    private List<Node> _nodes;
    private List<Edge> _edges;
    private Dictionary<RoadTileType, List<Node>> _roadTypes = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        DontDestroyOnLoad(Instance);
    }

    public void InitValues()
    {
        _nodes = new();
        _edges = new();
    }

    public void VisualizeMap()
    {
        this.InitValues();
        var lSystemSentence = lSystemGenerator.GenerateSentence();

        List<Node> _positions = new List<Node>();

        Stack<Node> savePoints = new();
        Node currentPosition = new Node(Vector3.zero);
        Vector3 direction = Vector3.forward;
        Node tempPosition = null;

        int lenght = 3;
        float distance = lenght - .1f;
        float angle = 90;

        _positions.Add(currentPosition);


        foreach (var letter in lSystemSentence) // each char in sentence
        {
            EncodingLetters encoding = (EncodingLetters)letter;

            switch (encoding)
            {
                case EncodingLetters.unknown:
                    break;
                case EncodingLetters.save:
                    currentPosition.Direction = direction;
                    savePoints.Push(currentPosition);
                    break;
                case EncodingLetters.load:
                    if (savePoints.Count > 0)
                    {
                        var currentNode = savePoints.Pop();
                        currentPosition = currentNode;
                        direction = currentNode.Direction;
                    }
                    else
                    {
                        throw new Exception("Dont have saved point in stack");
                    }
                    break;
                case EncodingLetters.draw:
                    //create new node
                    tempPosition = currentPosition;
                    currentPosition = new Node(currentPosition.Position + direction * lenght);
                    currentPosition.Direction = direction;


                    // check node that is not crossing another
                    Node foundNode = null;
                    if (!_positions.Any(e =>
                    {
                        if (true) //distance check
                        {
                            if (Vector3.Distance(e.Position, currentPosition.Position) <= distance) foundNode = e;
                        }
                        else
                        {
                            if (e.Position == currentPosition.Position) foundNode = e;
                        }
                        return foundNode != null;
                    }))
                    {
                        tempPosition.Neighbours = TryAddNextNeighbour(tempPosition.Neighbours, currentPosition);
                        currentPosition.Neighbours = TryAddNextNeighbour(currentPosition.Neighbours, tempPosition);


                        _positions.Add(currentPosition);
                    }
                    else
                    {
                        currentPosition = foundNode;
                    }

                    break;
                case EncodingLetters.turnRight:
                    direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                    break;
                case EncodingLetters.turnLeft:
                    direction = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
                    break;
            }
        }
    }

    private void RecognizeTypeOfNodes()
    {
        // finding edges
        foreach (var node in _nodes)
        {
            if(node.Neighbours.Count == 1)
            {
                if (UnityEngine.Random.Range(0, 100f) <= _chanceToJoin)
                {
                    var currentEdge = new Edge();
                    var dir = (node.Position - node.Neighbours[0].Position).normalized * 3;
                    currentEdge.FirstNode = node;
                    currentEdge.PossibleNextPosNode = node.Position + dir;

                    var foundNode = FindNodeByPos(currentEdge.PossibleNextPosNode);
                    if (foundNode != null)
                    {
                        currentEdge.SecondNode = foundNode;
                        _edges.Add(currentEdge);
                    }

                }
            }
        }

        // fix neighbors
        foreach (var item in _edges)
        {
            if (!item.FirstNode.Neighbours.Contains(item.SecondNode)) item.FirstNode.Neighbours.Add(item.SecondNode);
            if (!item.SecondNode.Neighbours.Contains(item.FirstNode)) item.SecondNode.Neighbours.Add(item.FirstNode);
        }

        foreach (var node in _nodes)
        {
            RecognizeTypeOfNode(node);
        }
    }

    private void RecognizeTypeOfNode(Node node)
    {
        RoadTileType type = RoadTileType.None;

        switch (node.Neighbours.Count)
        {
            case 1:
                type = RoadTileType.RoadEnd;
                break;
            case 2: // is curve or line
                Vector3 dir1 = node.Neighbours[0].Position - node.Position;
                Vector3 dir2 = node.Neighbours[1].Position - node.Position;
                float angle = Mathf.Round(Vector3.Angle(dir1, dir2));

                if (angle == 180) type = RoadTileType.RoadLine;
                else type = RoadTileType.RoadCurve;
                break;
            case 3:
                type = RoadTileType.Junction3Dirs;
                break;
            case 4:
                type = RoadTileType.Junction4Dirs;
                break;
        }
        if (type == RoadTileType.None) return;
        if (!_roadTypes.ContainsKey(type)) _roadTypes[type] = new();

        _roadTypes[type].Add(node);
    }

    private Node FindNodeByPos(Vector3 pos)
    {
        foreach (var node in _nodes)
        {
            if (Vector3.Distance(node.Position, pos) < .2f) return node;
        }
        return null;
    }

    private List<Node> TryAddNextNeighbour(List<Node> neighbours, Node newNeighbour)
    {
        if (!neighbours.Contains(newNeighbour)) neighbours.Add(newNeighbour);
        return neighbours;
    }

    [Serializable]
    public class Node
    {
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public List<Node> Neighbours { get; set; } = new();

        public Node(Vector3 position)
        {
            this.Position = position;
            this.Direction = Vector3.zero;
        }
    }
    [Serializable]
    public class Edge
    {
        public Node FirstNode { get; set; }
        public Node SecondNode { get; set; }
        public Vector3 PossibleNextPosNode { get; set; }
    }
}
