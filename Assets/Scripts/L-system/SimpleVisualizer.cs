using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleVisualizer : MonoBehaviour
{
    public LSystemGenerator lSystem;
    List<Vector3> _positions = new();
    List<Joins> _joins = new();
    public bool randomAngle = true;
    public bool changeLength = true;

    private class Joins
    {
        public Vector3 start;
        public Vector3 end;
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
        StartCoroutine(DynamicallyChange());
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

        var currentPosition = Vector3.zero;

        Vector3 direction = Vector3.forward;
        Vector3 tempPosition = Vector3.zero;

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
                    currentPosition += direction * _length;
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

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var item in _positions)
        {

            Gizmos.DrawWireCube(item, new Vector3(.5f, .5f, .5f));
        }

        foreach (var item in _joins)
        {
            Gizmos.color = item.color;
            Gizmos.DrawLine(item.start, item.end);
        }
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color)
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
}
