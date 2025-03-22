using LootLocker.Requests;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    [SerializeField] int score;
    [SerializeField] string nickname;
    [SerializeField] bool isLogged;
    public Action<int> OnChangeScore;
    public int GetScore => score;
    public string GetNickname => nickname;
    public bool IsLogged => isLogged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Load()
    {
        StartCoroutine(LeaderboardManager.Instance.FetchTopHigscoresRoutine());
        StartCoroutine(LeaderboardManager.Instance.GetCurrentScore());
    }

    public void SetLogged(bool isLogged, string nickname = "")
    {
        this.isLogged = isLogged;
        this.nickname = nickname;
        //nicknameTMP.text = nickname.ToString();
    }


    public void SetScore(int _score, bool withCloudSafe = true)
    {
        score = _score;
        OnChangeScore?.Invoke(score);
        if (withCloudSafe) StartCoroutine(LeaderboardManager.Instance.SubmitScoreRoutine(score));
    }

    public void SetPlayerName(string newNickName, Action callback)
    {
        //Debug.Log("Settin nickname");
        LootLockerSDKManager.SetPlayerName(newNickName, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Succesfully set player name");
                PlayerManager.Instance.SetLogged(true, newNickName);
            }
            else
            {
                //Debug.Log("Could ot set player name " + response.errorData.ToString());
            }
            callback?.Invoke();
        });
    }


}
