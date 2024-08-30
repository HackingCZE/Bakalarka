using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleVisualizer : MonoBehaviour
{
    public LSystemGenerator lSystem;
    [SerializeField] List<Node> _positions = new();
    [SerializeField] List<Node> _candidats = new();
    [SerializeField] List<Joins> _joins = new();
    public bool randomAngle = true;
    public bool changeLength = true;

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

    void Create()
    {
        Length = defaltLenght;
        _positions.Clear();
        _joins.Clear();

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
                    DrawLine(tempPosition, currentPosition, Color.red);
                    if (changeLength) Length -= 2;
                    _positions.Add(currentPosition);
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

    private void GetJunctions()
    {
        List<Joins> newJoins = new(_joins);
        while (newJoins.Count > 0)
        {
            var currentJoin = newJoins[0];
            newJoins.RemoveAt(0);

            Node toAdd = null;
            if (newJoins.Any(e =>
            {
                if (e.start == currentJoin.start || e.start == currentJoin.end) toAdd = e.start;

                return toAdd != null;
            })) _candidats.Add(toAdd);

            if (newJoins.Any(e =>
            {
                if (e.end == currentJoin.start || e.end == currentJoin.end) toAdd = e.end;

                return toAdd != null;
            })) _candidats.Add(toAdd);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var item in _positions)
        {

            Gizmos.DrawWireCube(item.position, new Vector3(.5f, .5f, .5f));
        }

        foreach (var item in _joins)
        {
            Gizmos.color = item.color;
            Gizmos.DrawLine(item.start.position, item.end.position);
        }

        Gizmos.color = Color.blue;
        foreach (var item in _candidats)
        {
            Gizmos.DrawWireCube(item.position, new Vector3(.5f, .5f, .5f));
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

        public Node(Vector3 position)
        {
            this.position = position;
        }
    }
}
