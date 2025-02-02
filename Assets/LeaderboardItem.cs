using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _rank;
    [SerializeField] TextMeshProUGUI _nickname;
    [SerializeField] TextMeshProUGUI _score;

    public void SetValues(string rank, string nickname, string score)
    {
        _rank.text = rank;
        _nickname.text = nickname;
        _score.text = score;
    }
}
