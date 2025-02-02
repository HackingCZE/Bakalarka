using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI nicknameTMP;
    [SerializeField] TextMeshProUGUI scoreTMP;
    [SerializeField] GameObject leaderboard;


    private void Start()
    {
        PlayerManager.Instance.Load();
        nicknameTMP.text = PlayerManager.Instance.GetNickname;
        PlayerManager.Instance.OnChangeScore += UpdateScore;
    }


    private void OnDisable()
    {        
        PlayerManager.Instance.OnChangeScore -= UpdateScore;
    }

    public void PlayGame()
    {
        MainGameManager.Instance.ClearAlgos();
        LoadingSceneManager.Instance.LoadScene("GameScene");
    } 

    private void UpdateScore(int score)
    {
        scoreTMP.text = score.ToString();
    }
    
    public void ShowLeaderboard()
    {
        leaderboard.SetActive(true);
    }

    public void CloseLeaderboard()
    {   
        leaderboard.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }


}
