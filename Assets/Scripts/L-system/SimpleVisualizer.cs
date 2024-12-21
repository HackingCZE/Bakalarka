using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
public class SimpleVisualizer : MonoBehaviour
{
    public static SimpleVisualizer Instance { get; private set; }
    public LSystemGenerator lSystem;
    [SerializeField] List<Node> _positions = new();
    [SerializeField] Dictionary<Node, List<Node>> _candidats = new();
    [SerializeField] Dictionary<Node, List<Node>> _nodes = new();
    [SerializeField] Dictionary<RoadTileType, List<Node>> _roadTypes = new();
    List<List<BuildingPoint>> _groups = new();
    List<JoinNode> _joinNodes = new List<JoinNode>();
    List<BuildingPoint> _points = new();
    List<AlgoNode> _algoNodes = new List<AlgoNode>();
    [SerializeField] private int _length = 8;
    int _defaltLenght = 0;
    private float _angle = 90;

    public int Length { get { return _length > 0 ? _length : 1; } set { _length = value; } }

    [SerializeField] Transform _player, _target;


    [SerializeField] bool _createOnAwake = false;
    [SerializeField] Transform _tilesParent;
    Transform _currentTilesParent;
    [SerializeField] int _minLenght;
    [SerializeField] float _distance;
    [SerializeField, Range(0, 100)] float _chanceToJoin = 50;
    public bool randomAngle = true;
    public bool changeLength = true;
    public bool distanceCheck = true;
    public bool gizmosCandidats = false;

    [SerializeField] GameObject _roadLine, _roadCurve, _junction3Dirs, _junction4Dirs, _roadEnd;

    public enum RoadTileType
    {
        Junction3Dirs, Junction4Dirs, RoadCurve, RoadLine, RoadEnd, None
    }

    private void Awake()
    {
        _defaltLenght = _length;
        Instance = this;
    }

    private void Start()
    {
        if (_createOnAwake) Create();
        //StartCoroutine(DynamicallyChange());
    }

    [Button]
    public void Create()
    {
        if (_currentTilesParent!= null) Destroy(_currentTilesParent.gameObject);
        PlacePoints();
        Length = _defaltLenght;
        _positions.Clear();
        _candidats.Clear();
        _nodes.Clear();
        _roadTypes.Clear();
        _groups.Clear();
        _joinNodes.Clear();
        _algoNodes.Clear();
        _points.Clear();

        _chanceToJoin = Random.Range(30, 80);



        var gm = new GameObject();
        gm.transform.SetParent(_tilesParent);
        _currentTilesParent = gm.transform;

        var sequance = lSystem.GenerateSentence();
        VisualizeSequance(sequance);


    }

    void PlacePoints()
    {
        _player.transform.position = new Vector3(Random.Range(-50f, 100f), 0, Random.Range(-50f, 50f));

        do
        {
            _target.transform.position = new Vector3(Random.Range(-50f, 100f), 0, Random.Range(-50f, 100f));
        } while (Vector3.Distance(_player.transform.position, _target.transform.position) < 20);
    }

    private void RecognizeTypeOfNode(KeyValuePair<Node, List<Node>> keyValue)
    {
        RoadTileType type = RoadTileType.None;

        List<Vector3> vector3s = new List<Vector3>();

        foreach (var item in keyValue.Value)
        {
            vector3s.Add(item.position);
        }


        keyValue.Key.list = vector3s;

        switch (keyValue.Value.Count)
        {
            case 1:
                type = RoadTileType.RoadEnd;
                break;
            case 2: // is curve or line
                Vector3 dir1 = keyValue.Value[0].position - keyValue.Key.position;
                Vector3 dir2 = keyValue.Value[1].position - keyValue.Key.position;
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

        _roadTypes[type].Add(keyValue.Key);
    }

    private List<Node> TryAddNextNeighbour(List<Node> neighbours, Node newNeighbour)
    {
        if (!neighbours.Contains(newNeighbour)) neighbours.Add(newNeighbour);
        return neighbours;
    }

    [Button]
    private void SpawnTiles()
    {
        foreach (var roadType in _roadTypes)
        {
            foreach (var node in roadType.Value)
            {

                float rotation = 0f;
                Vector3 dir1, dir2, dir3;
                switch (roadType.Key)
                {
                    case RoadTileType.Junction4Dirs:
                        AddBuildinPoints(Instantiate(_junction4Dirs, node.position, Quaternion.identity, _currentTilesParent));
                        break;
                    case RoadTileType.Junction3Dirs:
                        dir1 = (node.list[0] - node.position).normalized;
                        dir2 = (node.list[1] - node.position).normalized;
                        dir3 = (node.list[2] - node.position).normalized;

                        rotation = 0f;
                        bool up = (dir1.z > 0.5f) || (dir2.z > 0.5f) || (dir3.z > 0.5f);
                        bool down = (dir1.z < -0.5f) || (dir2.z < -0.5f) || (dir3.z < -0.5f);
                        bool right = (dir1.x > 0.5f) || (dir2.x > 0.5f) || (dir3.x > 0.5f);
                        bool left = (dir1.x < -0.5f) || (dir2.x < -0.5f) || (dir3.x < -0.5f);


                        if (up && down && right) rotation = 0;
                        else if (up && down && left) rotation = 180f;
                        else if (up && left && right) rotation = -90f;
                        else if (down && left && right) rotation = 90f;

                        //Debug.Log(rotation + " - " + Quaternion.Euler(0, rotation, 0).eulerAngles.ToString());

                        AddBuildinPoints(Instantiate(_junction3Dirs, node.position, Quaternion.Euler(0, rotation, 0), _currentTilesParent));
                        break;
                    case RoadTileType.RoadCurve:

                        dir1 = (node.list[0] - node.position).normalized;
                        dir2 = (node.list[1] - node.position).normalized;

                        rotation = 0f;

                        if (Mathf.Abs(dir1.x) > Mathf.Abs(dir1.z)) rotation = dir1.x > 0 ? (dir2.z > 0 ? -90f : 0f) : (dir2.z > 0 ? 180f : 90f); // Soused1 je na ose X (vodorovnì
                        else rotation = dir1.z > 0 ? (dir2.x > 0 ? -90f : 180f) : (dir2.x > 0 ? 0f : 90f); // Soused1 je na ose Z (svisle)

                        AddBuildinPoints(Instantiate(_roadCurve, node.position, Quaternion.Euler(0, rotation, 0), _currentTilesParent));
                        break;
                    case RoadTileType.RoadLine:
                        dir1 = (node.list[0] - node.position).normalized;
                        dir2 = (node.list[1] - node.position).normalized;

                        rotation = 0f;

                        if (Mathf.Abs(dir1.x) > 0.1f && Mathf.Abs(dir2.x) > 0.1f) rotation = 90;


                        AddBuildinPoints(Instantiate(_roadLine, node.position, Quaternion.Euler(0, rotation, 0), _currentTilesParent));
                        break;
                    case RoadTileType.RoadEnd:
                        dir1 = (node.list[0] - node.position).normalized;

                        float angle = Mathf.Atan2(dir1.x, dir1.z) * Mathf.Rad2Deg;
                        angle = Mathf.Round(angle / 90) * 90;
                        AddBuildinPoints(Instantiate(_roadEnd, node.position, Quaternion.Euler(0, angle, 0), _currentTilesParent));
                        break;
                    case RoadTileType.None:
                        break;
                    default:
                        break;
                }
            }
        }


    }

    [Button("GetGroups")]
    public void GetGroups()
    {
        GroupingPoints groupingPoints = new GroupingPoints();
        _points = RemoveDuplicates(_points);
        _groups = groupingPoints.GroupPoints(_points);
    }

    public List<BuildingPoint> RemoveDuplicates(List<BuildingPoint> allPoints)
    {
        List<BuildingPoint> toRemove = new();
        foreach (var item in allPoints)
        {
            foreach (var item1 in allPoints)
            {
                if (!toRemove.Contains(item1) && Vector3.Distance(item.Position, item1.Position) < .15f) toRemove.Add(item1);
            }
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            allPoints.Remove(toRemove[i]);
        }

        return allPoints;
    }

    public void AddBuildinPoints(GameObject obj)
    {
        TileStats tileStats = obj.GetComponent<TileStats>();

        _points.AddRange(tileStats.GetBuildingPoints());
        Destroy(tileStats);
    }

    public List<AlgoNode> GetNodes()
    {
        // Vytvoøíme mapu, abychom rychle našli a pøiøadili AlgoNode objekt k pozici
        Dictionary<Vector3, AlgoNode> algoNodeMap = new Dictionary<Vector3, AlgoNode>();

        // Nejprve vytvoøíme všechny AlgoNode objekty bez sousedù
        foreach (var entry in _nodes)
        {
            Node currentNode = entry.Key;
            AlgoNode algoNode = new AlgoNode(currentNode.position, null);  // Parent zatím nastaven na null
            algoNodeMap[currentNode.position] = algoNode;
        }

        // Nyní pøidáme sousedy a pøiøadíme Parent
        foreach (var entry in _nodes)
        {
            Node currentNode = entry.Key;
            List<Node> neighbors = entry.Value;

            AlgoNode algoNode = algoNodeMap[currentNode.position];

            foreach (Node neighbor in neighbors)
            {
                if (algoNodeMap.TryGetValue(neighbor.position, out AlgoNode neighborAlgoNode))
                {
                    algoNode.Neighbours.Add(neighborAlgoNode);
                }
            }
        }

        var lsit = algoNodeMap.Values.ToList();
        Debug.Log(lsit.Count);

        // Vrátíme všechny AlgoNode jako List
        return lsit;
    }

    IEnumerator DynamicallyChange()
    {
        yield return new WaitForSeconds(2f);
        Create();
        StartCoroutine(DynamicallyChange());
    }

    private void VisualizeSequance(string sequance)
    {
        Stack<AgentParameters> savePoints = new();

        if (randomAngle) _angle = UnityEngine.Random.Range(_angle > 180 ? 180 : 0, _angle > 180 ? 360 : 180);
        else _angle = 90;

        Node currentPosition = new Node(Vector3.zero);

        Vector3 direction = Vector3.forward;
        Node tempPosition = null;

        _positions.Add(currentPosition);

        foreach (var letter in sequance)
        {
            EncodingLetters encoding = (EncodingLetters)letter;
            switch (encoding)
            {
                case EncodingLetters.save:
                    savePoints.Push(new AgentParameters
                    {
                        position = currentPosition,
                        direction = direction,
                        length = Length
                    });
                    break;
                case EncodingLetters.load:
                    if (savePoints.Count > 0)
                    {
                        var agentParameter = savePoints.Pop();
                        currentPosition = agentParameter.position;
                        direction = agentParameter.direction;
                        Length = agentParameter.length;
                    }
                    else
                    {
                        throw new Exception("Dont have saved point in stack");
                    }
                    break;
                case EncodingLetters.draw:
                    tempPosition = currentPosition;
                    currentPosition = new Node(currentPosition.position + direction * _length);
                    currentPosition.direction = direction;
                    //currentPosition.position2 = currentPosition.position;

                    Node findedNode = null;
                    if (!_positions.Any(e =>
                    {
                        if (distanceCheck)
                        {
                            if (Vector3.Distance(e.position, currentPosition.position) <= _distance) findedNode = e;
                        }
                        else
                        {
                            if (e.position == currentPosition.position) findedNode = e;
                        }
                        return findedNode != null;
                    }))
                    {
                        if (!_nodes.ContainsKey(tempPosition)) _nodes[tempPosition] = new List<Node>();
                        if (!_nodes.ContainsKey(currentPosition)) _nodes[currentPosition] = new List<Node>();
                        _nodes[tempPosition] = TryAddNextNeighbour(_nodes[tempPosition], currentPosition);
                        _nodes[currentPosition] = TryAddNextNeighbour(_nodes[currentPosition], tempPosition);


                        //DrawLine(tempPosition, currentPosition, Color.red);
                        //PlaceStreetPos(tempPosition.position, Vector3Int.RoundToInt(direction), Length);
                        if (changeLength) Length -= 2;
                        if (Length < _minLenght) Length = _minLenght;

                        _distance = Length - .1f;
                        //while (_positions.Any(e => e.position == currentPosition.position2))
                        //{
                        //    currentPosition.position2 += new Vector3(0, 1, 0);
                        //}
                        _positions.Add(currentPosition);
                    }
                    else
                    {
                        currentPosition = findedNode;
                    }
                    break;
                case EncodingLetters.turnRight:
                    direction = Quaternion.AngleAxis(_angle, Vector3.up) * direction;
                    break;
                case EncodingLetters.turnLeft:
                    direction = Quaternion.AngleAxis(-_angle, Vector3.up) * direction;
                    break;
            }
        }
        RecognizeTypeOfNodes();
    }

    private Node FindNodeByPos(Vector3 pos)
    {
        foreach (var node in _nodes)
        {
            if (Vector3.Distance(node.Key.position, pos) < .2f) return node.Key;

        }
        return null;
    }


    private void RecognizeTypeOfNodes()
    {

        _joinNodes = new();
        foreach (var node in _nodes)
        {
            if (node.Value.Count == 1)
            {
                if (UnityEngine.Random.Range(0, 100f) <= _chanceToJoin)
                {
                    var currentJoin = new JoinNode();
                    var dir = (node.Key.position - node.Value[0].position).normalized * 3;
                    currentJoin.one = node.Key;
                    currentJoin.pos = node.Key.position + dir;

                    var foundNode = FindNodeByPos(currentJoin.pos);
                    if (foundNode != null)
                    {
                        currentJoin.two = foundNode;
                        _joinNodes.Add(currentJoin);
                    }

                }
            }
        }

        foreach (var item in _joinNodes)
        {
            if (!_nodes[item.one].Contains(item.two)) _nodes[item.one].Add(item.two);
            if (!_nodes[item.two].Contains(item.one)) _nodes[item.two].Add(item.one);
        }

        foreach (var node in _nodes)
        {
            RecognizeTypeOfNode(node);
        }

       // SpawnTiles();
    }


    private void OnDrawGizmos()
    {
        for (int i = 0; i < _points.Count; i++)
        {
            Gizmos.color = Color.white;
            if (_points[i].RotationY == 315) Gizmos.color = Color.black;
            Gizmos.DrawSphere(_points[i].Position + new Vector3(0, .5f, 0), .1f);
            Handles.ArrowHandleCap(0, _points[i].Position, Quaternion.LookRotation(_points[i].Direction), 1f, EventType.Repaint);
        }
        Gizmos.color = Color.red;
        foreach (var group in _groups)
        {
            for (int i = 0; i < group.Count; i++)
            {
                Gizmos.color = Color.red;

                if (i < group.Count - 1) Gizmos.DrawLine(group[i].Position + new Vector3(0, .5f, 0), group[i + 1].Position + new Vector3(0, .5f, 0));
                Handles.Label(group[i].Position + new Vector3(0, .8f, 0), i.ToString());
                Handles.Label(group[i].Position + new Vector3(0, 1f, 0), group[i].Direction.ToString());
                Handles.Label(group[i].Position + new Vector3(0, 1.2f, 0), group[i].RotationY.ToString());
                if (i < group.Count - 1) Handles.Label(group[i].Position + new Vector3(0, 1.5f, 0), ((group[i + 1].Position - group[i].Position).normalized).ToString());

                //Gizmos.color = Color.yellow;

                //Gizmos.DrawWireSphere(group[i].Position, 1.95f);

                //Gizmos.color = Color.green;
                //Gizmos.DrawWireSphere(group[i].Position, 1.2f);

            }
        }

        return;
        foreach (var item in _joinNodes)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(item.one.position + new Vector3(0, .5f, 0), .1f);
            Gizmos.color = Color.red;

            Gizmos.DrawLine(item.one.position + new Vector3(0, .5f, 0), item.pos + new Vector3(0, .5f, 0));
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(item.two.position + new Vector3(0, .8f, 0), .1f);
        }

        foreach (var item in _nodes)
        {
            Gizmos.color = Color.white;
            Handles.Label(item.Key.position + new Vector3(0, 2, 0), item.Key.direction.ToString());

            foreach (var neighbour in item.Value)
            {
                Gizmos.DrawLine(item.Key.position, neighbour.position + new Vector3(0, .5f, 0));
            }
        }

        foreach (var roadType in _roadTypes)
        {
            switch (roadType.Key)
            {
                case RoadTileType.Junction3Dirs:
                    Gizmos.color = Color.cyan;
                    break;
                case RoadTileType.Junction4Dirs:
                    Gizmos.color = Color.yellow;
                    break;
                case RoadTileType.RoadCurve:
                    Gizmos.color = Color.green;
                    break;
                case RoadTileType.RoadLine:
                    Gizmos.color = Color.red;
                    break;
                case RoadTileType.None:
                    Gizmos.color = Color.black;
                    break;
            }

            Color currentC = Gizmos.color;
            foreach (var node in roadType.Value)
            {
                Gizmos.color = currentC;
                Gizmos.DrawWireCube(node.position, new Vector3(.5f, .5f, .5f));
                if (roadType.Key != RoadTileType.RoadCurve) continue;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(node.list[0] + new Vector3(0, 1f, 0), .1f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(node.list[1] + new Vector3(0, 1f, 0), .1f);
            }
        }

        //Gizmos.color = Color.green;
        //foreach (var item in _positions)
        //{

        //    Gizmos.DrawWireCube(item.position, new Vector3(.5f, .5f, .5f));
        //    Handles.Label(item.position + new Vector3(0, 2, 0), item.direction.ToString());

        //}

        //foreach (var item in _joins)
        //{
        //    Gizmos.DrawLine(item.start.position, item.end.position);
        //}

        //if (!gizmosCandidats) return;
        //foreach (var item in _candidats)
        //{
        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawWireCube(item.Key.position, new Vector3(.5f, .5f, .5f));
        //    Handles.Label(item.Key.position + new Vector3(0, 2, 0), item.Value.Count.ToString());
        //    foreach (var node in item.Value)
        //    {
        //        Gizmos.color = Color.black;
        //        Vector3 nesPos = node.position + new Vector3(0, 1, 0);
        //        Gizmos.DrawLine(item.Key.position, nesPos);
        //        Gizmos.DrawSphere(nesPos, .5f);
        //    }
        //}
    }


    public enum EncodingLetters
    {
        unknown = '1',
        save = '[',
        load = ']',
        draw = 'F',
        turnRight = '+',
        turnLeft = '-'
    }

    [Serializable]
    public class Node
    {
        public Vector3 position;
        public Vector3 position2;
        public Vector3 direction;
        public List<Vector3> list;

        public Node(Vector3 position)
        {
            this.position = position;
            this.position2 = position;
        }
    }

    [Serializable]
    public class JoinNode
    {
        public Node one;
        public Node two;
        public Vector3 pos;
    }

    public class BuildingPoint
    {
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public float RotationY { get; set; }
        public bool Visited { get; set; }

        public BuildingPoint(Vector3 position, Vector3 direction, float rotationY)
        {
            this.Position = position;
            this.Direction = direction;
            this.RotationY = rotationY;
        }

        // Pøepsání Equals pro porovnání pozic
        public override bool Equals(object obj)
        {
            if (obj is BuildingPoint other)
            {
                return Position == other.Position;
            }
            return false;
        }

        // Pøepsání GetHashCode, abychom mohli použít Distinct
        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }

}
