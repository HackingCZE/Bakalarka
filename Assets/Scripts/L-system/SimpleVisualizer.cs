using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleVisualizer : MonoBehaviour
{
    public LSystemGenerator lSystem;
    [SerializeField] List<Node> _positions = new();
    [SerializeField] Dictionary<Node, List<Node>> _candidats = new();
    [SerializeField] List<Joins> _joins = new();
    [SerializeField] Transform _tilesParent;
    [SerializeField] int _minLenght;
    [SerializeField] float _distance;
    public bool randomAngle = true;
    public bool changeLength = true;
    public bool distanceCheck = true;
    public bool gizmosCandidats = false;

    [SerializeField] GameObject _roadLine, _roadCurve, _junction3Dirs, _junction4Dirs;

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
                    currentPosition.position2 = currentPosition.position;

                    Node findedNode = null;
                    if (!_positions.Any(e =>
                    {
                        if (distanceCheck)
                        {
                            if (Vector3.Distance(e.position, currentPosition.position2) <= _distance) findedNode = e;
                        }
                        else
                        {
                            if(e.position == currentPosition.position2) findedNode = e;
                        }
                        return findedNode != null;
                    }))
                    {
                        DrawLine(tempPosition, currentPosition, Color.red);
                        //PlaceStreetPos(tempPosition.position, Vector3Int.RoundToInt(direction), Length);
                        if (changeLength) Length -= 2;
                        if (Length < _minLenght) Length = _minLenght;
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
        GetJunctions();
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
        Gizmos.color = Color.green;
        foreach (var item in _positions)
        {

            Gizmos.DrawWireCube(item.position2, new Vector3(.5f, .5f, .5f));
        }

        foreach (var item in _joins)
        {
            Gizmos.color = item.color;
            Gizmos.DrawLine(item.start.position2, item.end.position2);
        }

        if (!gizmosCandidats) return;
        foreach (var item in _candidats)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(item.Key.position, new Vector3(.5f, .5f, .5f));
            Handles.Label(item.Key.position + new Vector3(0, 2, 0), item.Value.Count.ToString());
            foreach (var node in item.Value)
            {
                Gizmos.color = Color.black;
                Vector3 nesPos = node.position + new Vector3(0, 1, 0);
                Gizmos.DrawLine(item.Key.position, nesPos);
                Gizmos.DrawSphere(nesPos, .5f);
            }
        }
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

        public Node(Vector3 position)
        {
            this.position = position;
            this.position2 = position;
        }
    }
}
