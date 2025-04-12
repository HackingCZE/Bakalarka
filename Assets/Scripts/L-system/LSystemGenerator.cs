using System;
using System.Text;
using UnityEngine;

public class LSystemGenerator : MonoBehaviour
{
    public Rule[] rules;
    public string rootSentence;
    [Range(1, 10)] public int iterationLimit = 1;

    public bool randomIgnoreRuleModifer = true;
    [Range(0, 1)]
    public float changeToIngoreRule = .35f;


    public string GenerateSentence(string word = null)
    {
        if(MainGameManager.Instance.GetLevel() > 50)
        {
            iterationLimit = Mathf.Max((MainGameManager.Instance.GetLevel() / 50) * 1 + 5, 1);
        }
        else if(MainGameManager.Instance.GetLevel() > 40)
        {
            iterationLimit = 5;
        }
        else if(MainGameManager.Instance.GetLevel() > 25)
        {
            iterationLimit = 4;
        }
        else if(MainGameManager.Instance.GetLevel() > 15)
        {
            iterationLimit = 3;
        }
        else if(MainGameManager.Instance.GetLevel() > 5)
        {
            iterationLimit = 2;
        }
        else
        {
            iterationLimit = 1;
        }

        // Debug.Log("CURENT INTERATION: " + iterationLimit);
        if(word == null) word = rootSentence;
        return GrowRecursive(word);
    }

    private string GrowRecursive(string word, int iterationIndex = 0)
    {
        if(iterationIndex >= iterationLimit) return word;

        StringBuilder sb = new StringBuilder();
        foreach(var item in word)
        {
            sb.Append(item);
            ProcessRulesRecursivelly(sb, item, iterationIndex);
        }

        return sb.ToString();
    }

    private void ProcessRulesRecursivelly(StringBuilder sb, char item, int iterationIndex)
    {
        foreach(var rule in rules)
        {
            if(rule.letter == item.ToString())
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
