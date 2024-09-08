using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86;

public class SimpleVisualizer : MonoBehaviour
{
    public LSystemGenerator lSystem;
    [SerializeField] List<Node> _positions = new();
    [SerializeField] Dictionary<Node, List<Node>> _candidats = new();
    [SerializeField] Dictionary<Node, List<Node>> _nodes = new();
    [SerializeField] Dictionary<RoadTileType, List<Node>> _roadTypes = new();
    [SerializeField] List<Joins> _joins = new();
    [SerializeField] Transform _tilesParent;
    [SerializeField] int _minLenght;
    [SerializeField] float _distance;
    public bool randomAngle = true;
    public bool changeLength = true;
    public bool distanceCheck = true;
    public bool gizmosCandidats = false;

    [SerializeField] GameObject _roadLine, _roadCurve, _junction3Dirs, _junction4Dirs;

    public enum RoadTileType
    {
        Junction3Dirs, Junction4Dirs, RoadCurve, RoadLine, None
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
            case 2:
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
                        Instantiate(_junction4Dirs, node.position, Quaternion.identity, _tilesParent);
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

                        Debug.Log(rotation + " - " + Quaternion.Euler(0, rotation, 0).eulerAngles.ToString());

                        Instantiate(_junction3Dirs, node.position, Quaternion.Euler(0, rotation, 0), _tilesParent);
                        break;
                    case RoadTileType.RoadCurve:

                        dir1 = (node.list[0] - node.position).normalized;
                        dir2 = (node.list[1] - node.position).normalized;

                        rotation = 0f;

                        if (Mathf.Abs(dir1.x) > Mathf.Abs(dir1.z)) rotation = dir1.x > 0 ? (dir2.z > 0 ? -90f : 0f) : (dir2.z > 0 ? 180f : 90f); // Soused1 je na ose X (vodorovnì
                        else rotation = dir1.z > 0 ? (dir2.x > 0 ? -90f : 180f) : (dir2.x > 0 ? 0f : 90f); // Soused1 je na ose Z (svisle)

                        Instantiate(_roadCurve, node.position, Quaternion.Euler(0, rotation, 0), _tilesParent);
                        break;
                    case RoadTileType.RoadLine:
                        dir1 = (node.list[0] - node.position).normalized;
                        dir2 = (node.list[1] - node.position).normalized;

                        rotation = 0f;

                        if (Mathf.Abs(dir1.x) > 0.1f && Mathf.Abs(dir2.x) > 0.1f) rotation = 90;

                        Instantiate(_roadLine, node.position, Quaternion.Euler(0, rotation, 0), _tilesParent);
                        break;
                    case RoadTileType.None:
                        break;
                    default:
                        break;
                }
            }
        }
    }

    [Serializable]
    private class Joins
    {
        public Node start;
        public Node end;
        public Color color;
    }

    [SerializeField] private int _length = 8;
    int defaltLenght = 0;
    private float _angle = 90;

    public int Length { get { return _length > 0 ? _length : 1; } set { _length = value; } }

    private void Start()
    {
        defaltLenght = _length;
        Create();
        //StartCoroutine(DynamicallyChange());
    }

    [Button]
    void Create()
    {
        Length = defaltLenght;
        _positions.Clear();
        _joins.Clear();
        _candidats.Clear();

        var sequance = lSystem.GenerateSentence();
        VisualizeSequance(sequance);
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

    private void RecognizeTypeOfNodes()
    {
        foreach (var node in _nodes)
        {
            RecognizeTypeOfNode(node);
        }

        SpawnTiles();
    }

    HashSet<Vector3Int> fixRoadCandidates = new HashSet<Vector3Int>();
    Dictionary<Vector3Int, GameObject> roadDictionary = new Dictionary<Vector3Int, GameObject>();

    public void PlaceStreetPos(Vector3 startPos, Vector3Int direction, int length)
    {
        var rotation = Quaternion.identity;

        if (direction.x != 0)
        {
            rotation = Quaternion.Euler(0, 90, 0);
        }
        var pos = Vector3Int.RoundToInt(startPos + direction);
        if (roadDictionary.ContainsKey(pos)) return;
        var road = Instantiate(_roadLine, pos, rotation, _tilesParent);
        roadDictionary.Add(pos, road);
    }

    private void GetJunctions()
    {
        List<Joins> newJoins = new(_joins);
        while (newJoins.Count > 0)
        {
            var currentJoin = newJoins[0];
            newJoins.RemoveAt(0);

            Joins toAdd = null;
            if (newJoins.Any(e =>
            {
                if (e.start == currentJoin.start || e.start == currentJoin.end) toAdd = e;

                return toAdd != null;
            }))
            {
                if (!_candidats.ContainsKey(toAdd.start))
                {
                    _candidats.Add(toAdd.start, new());
                }
                _candidats[toAdd.start].Add(toAdd.end);
            }

            if (newJoins.Any(e =>
            {
                if (e.end == currentJoin.start || e.end == currentJoin.end) toAdd = e;

                return toAdd != null;
            }))
            {
                if (!_candidats.ContainsKey(toAdd.end))
                {
                    _candidats.Add(toAdd.end, new());
                }
                _candidats[toAdd.end].Add(toAdd.start);
            }
        }

    }

    [Button]
    public void SpawnRoadTiles()
    {
        foreach (var node in _candidats)
        {
            if (node.Value.Count == 4)
            {
                Instantiate(_junction4Dirs, node.Key.position, Quaternion.identity, _tilesParent);
            }
        }
    }

    private void OnDrawGizmos()
    {

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

    private void DrawLine(Node start, Node end, Color color)
    {
        _joins.Add(new Joins()
        {
            start = start,
            end = end,
            color = color,
        });
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
}
