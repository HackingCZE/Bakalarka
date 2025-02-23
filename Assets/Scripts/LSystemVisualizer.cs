using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using static SimpleVisualizer;
using UnityEditor;
using NaughtyAttributes;

public class LSystemVisualizer : MonoBehaviour
{
    public static LSystemVisualizer Instance { get; private set; }

    public LSystemGenerator lSystemGenerator;

    [SerializeField, Range(0, 100)] private float _chanceToJoin = 80;
    Transform _currentTilesParent, _tilesParent;

    [SerializeField] private bool _withTiles = true;

    // list
    private List<Node> _nodes;
    private List<Edge> _edges;
    private Dictionary<RoadTileType, List<Node>> _roadTypes = new();
    List<BuildingPoint> _points = new();

    [Header("Prefabs")]
    [SerializeField] private GameObject _junction4Dirs;
    [SerializeField] private GameObject _junction3Dirs;
    [SerializeField] private GameObject _roadLine;
    [SerializeField] private GameObject _roadEnd;
    [SerializeField] private GameObject _roadCurve;



    private void Awake()
    {
        Instance = this;
    }

    public float GetCurrentChanceToJoin => _chanceToJoin;

    public void InitValues()
    {
        if (_currentTilesParent != null) Destroy(_currentTilesParent.gameObject);
        var gm = new GameObject();
        gm.transform.SetParent(_tilesParent);
        _currentTilesParent = gm.transform;

        // TODO reset cam

        _chanceToJoin = Random.Range(Mathf.Min(30f + (MainGameManager.Instance.GetLevel() / 7) * 5, 75f), 90f);

        _nodes = new();
        _edges = new();
        _roadTypes = new();
        _points = new();
    }

    public List<Vector3> GetNodesAsVector()
    {
        List<Vector3> result = new();
        foreach (var currentNode in _nodes)
        {
            result.Add(currentNode.Position);
        }
        return result;
    }

    public List<AlgoNode> GetNodes()
    {
        Dictionary<Vector3, AlgoNode> algoNodeMap = new Dictionary<Vector3, AlgoNode>();

        foreach (var currentNode in _nodes)
        {
            AlgoNode algoNode = new AlgoNode(currentNode.Position, null, currentNode.Type);  
            algoNodeMap[currentNode.Position] = algoNode;
        }

        foreach (var currentNode in _nodes)
        {
            AlgoNode algoNode = algoNodeMap[currentNode.Position];

            foreach (Node neighbor in currentNode.Neighbours)
            {
                if (algoNodeMap.TryGetValue(neighbor.Position, out AlgoNode neighborAlgoNode))
                {
                    algoNode.Neighbours.Add(neighborAlgoNode);
                }
            }
        }

        return algoNodeMap.Values.ToList();
    }

    [Button]
    public void VisualizeMap()
    {
        this.InitValues();
        NavigationManager.Instance.PlacePoints();
        var lSystemSentence = lSystemGenerator.GenerateSentence();

        Stack<Node> savePoints = new();
        Node currentPosition = new Node(Vector3.zero);
        Vector3 direction = Vector3.forward;
        Node tempPosition = null;

        int lenght = 3;
        float distance = lenght - .1f;
        float angle = 90;

        _nodes.Add(currentPosition);

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
                    if (!_nodes.Any(e =>
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


                        _nodes.Add(currentPosition);
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

        RecognizeTypeOfNodes();
    }

    private void RecognizeTypeOfNodes()
    {
        // finding edges
        foreach (var node in _nodes)
        {
            if (node.Neighbours.Count == 1)
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

        if(_withTiles) SpawnTiles();
    }

    public void SpawnTiles()
    {
        foreach (var roadType in _roadTypes)
        {
            foreach (var node in roadType.Value)
            {

                float rotation = 0f;
                Vector3 dir1, dir2, dir3; // Direction vectors for neighbor calculations

                // Determine the road tile type and handle each case
                switch (roadType.Key)
                {
                    case RoadTileType.Junction4Dirs:
                        // Spawn a 4-way junction at the node's position
                        Instantiate(_junction4Dirs, node.Position, Quaternion.identity, _currentTilesParent);
                        break;
                    case RoadTileType.Junction3Dirs:
                        // Calculate normalized directions to neighbors
                        dir1 = (node.Neighbours[0].Position - node.Position).normalized;
                        dir2 = (node.Neighbours[1].Position - node.Position).normalized;
                        dir3 = (node.Neighbours[2].Position - node.Position).normalized;

                        rotation = 0f;// Reset rotation

                        // Check which directions (up, down, left, right) have neighbors
                        bool up = (dir1.z > 0.5f) || (dir2.z > 0.5f) || (dir3.z > 0.5f);
                        bool down = (dir1.z < -0.5f) || (dir2.z < -0.5f) || (dir3.z < -0.5f);
                        bool right = (dir1.x > 0.5f) || (dir2.x > 0.5f) || (dir3.x > 0.5f);
                        bool left = (dir1.x < -0.5f) || (dir2.x < -0.5f) || (dir3.x < -0.5f);

                        // Determine the rotation based on neighbor configuration
                        if (up && down && right) rotation = 0;
                        else if (up && down && left) rotation = 180f;
                        else if (up && left && right) rotation = -90f;
                        else if (down && left && right) rotation = 90f;

                        // Spawn a 3-way junction with the calculated rotation
                        Instantiate(_junction3Dirs, node.Position, Quaternion.Euler(0, rotation, 0), _currentTilesParent);
                        break;
                    case RoadTileType.RoadCurve:
                        // Get directions to two neighbors
                        dir1 = (node.Neighbours[0].Position - node.Position).normalized;
                        dir2 = (node.Neighbours[1].Position - node.Position).normalized;

                        rotation = 0f;// Reset rotation

                        // Determine rotation based on the curve's neighbor alignment
                        if (Mathf.Abs(dir1.x) > Mathf.Abs(dir1.z))
                            rotation = dir1.x > 0 ? (dir2.z > 0 ? -90f : 0f) : (dir2.z > 0 ? 180f : 90f); // Neighbour1 is on X (horizontal)
                        else
                            rotation = dir1.z > 0 ? (dir2.x > 0 ? -90f : 180f) : (dir2.x > 0 ? 0f : 90f); // Neighbour1 is on Z (vertical)

                        // Spawn a road curve tile
                        Instantiate(_roadCurve, node.Position, Quaternion.Euler(0, rotation, 0), _currentTilesParent);
                        break;
                    case RoadTileType.RoadLine:
                        // Get directions to two neighbors
                        dir1 = (node.Neighbours[0].Position - node.Position).normalized;
                        dir2 = (node.Neighbours[1].Position - node.Position).normalized;

                        rotation = 0f; // Reset rotation

                        // Determine if the road is horizontal (along the X-axis)
                        if (Mathf.Abs(dir1.x) > 0.1f && Mathf.Abs(dir2.x) > 0.1f) rotation = 90;

                        // Spawn a straight road tile
                        Instantiate(_roadLine, node.Position, Quaternion.Euler(0, rotation, 0), _currentTilesParent);
                        break;
                    case RoadTileType.RoadEnd:
                        // Calculate the angle based on the single neighbors direction
                        dir1 = (node.Neighbours[0].Position - node.Position).normalized;

                        float angle = Mathf.Atan2(dir1.x, dir1.z) * Mathf.Rad2Deg;
                        angle = Mathf.Round(angle / 90) * 90; // Round to the nearest 90 degrees

                        // Spawn a road end tile
                        Instantiate(_roadEnd, node.Position, Quaternion.Euler(0, angle, 0), _currentTilesParent);
                        break;
                    case RoadTileType.None:
                        break;
                    default:
                        break;
                }
            }
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

        node.Type = type;
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

    private void OnDrawGizmosSelected()
    {
        foreach (var item in _nodes)
        {
            Gizmos.color = Color.white;
            Handles.Label(item.Position + new Vector3(0, 2, 0), item.Direction.ToString());

            foreach (var neighbour in item.Neighbours)
            {
                Gizmos.DrawLine(item.Position, neighbour.Position + new Vector3(0, .5f, 0));
            }
        }
    }


    [Serializable]
    public class Node
    {
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public List<Node> Neighbours { get; set; } = new();
        public RoadTileType Type { get; set; }

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
