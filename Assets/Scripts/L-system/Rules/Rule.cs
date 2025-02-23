using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="L_System/Rule")]
public class Rule : ScriptableObject
{
    public string letter;
    [SerializeField] private string[] _results = null;
    [SerializeField] bool _randomResult = false;

    public string GetResult => _randomResult ? _results[Random.Range(0,_results.Length)] : _results[0];
}
