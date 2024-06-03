using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LSystemGenerator : MonoBehaviour
{
    public Rule[] rules;
    public string rootSentence;
    [Range(1, 10)] public int iterationLimit = 1;

    public bool randomIgnoreRuleModifer = true;
    [Range(0,1)]
    public float changeToIngoreRule = .35f;

    private void Start()
    {
        Debug.Log(GenerateSentence());
    }



    public string GenerateSentence(string word = null)
    {
        if (word == null) word = rootSentence;
        return GrowRecursive(word);
    }

    private string GrowRecursive(string word, int iterationIndex = 0)
    {
        if (iterationIndex >= iterationLimit) return word;

        StringBuilder sb = new StringBuilder();
        foreach (var item in word)
        {
            sb.Append(item);
            ProcessRulesRecursivelly(sb, item, iterationIndex);
        }

        return sb.ToString();
    }

    private void ProcessRulesRecursivelly(StringBuilder sb, char item, int iterationIndex)
    {
        foreach (var rule in rules)
        {
            if (rule.letter == item.ToString())
            {
                if(randomIgnoreRuleModifer && iterationIndex > 1)
                {
                    if(UnityEngine.Random.value < changeToIngoreRule) return;
                }
                sb.Append(GrowRecursive(rule.GetResult, iterationIndex + 1));
            }
        }
    }
}
