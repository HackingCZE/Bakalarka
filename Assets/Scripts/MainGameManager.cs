using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using static NavigationManager;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager Instance { get; private set; }
    [SerializeField] List<AlgorithmStats> _algorithmStats = new List<AlgorithmStats>();

    int _currentGameScore = 0;
    int _currentGameLives = 0;
    int _currentGameLevel = 0;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") StartGame();
    }

    public void StartGame()
    {
        _currentGameLives = 3;
        _currentGameLevel = 1;
        GenerateMap();

        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.freeview);
        MainGameManagerUI.Instance.UpdateScoreUI(_currentGameScore);
    }

    public void EndGame()
    {
        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.end);
        // TODO: save
    }

    private async void GenerateMap()
    {
         SimpleVisualizer.Instance.Create();
        _algorithmStats = await NavigationManager.Instance.GetOrderOfAlgorithms();
    }

    public bool CheckSelectedAlgorithm(VoteNavigationAlgorithm navigationAlgorithm)
    {
        if (_algorithmStats[0].Algorithm == navigationAlgorithm.navigationAlgorithm) _currentGameScore += 1 * Countdown.Instance.GetLastRemaining();
        else _currentGameLives--;

        MainGameManagerUI.Instance.UpdateScoreUI(_currentGameScore);

        if (_currentGameLives <= 0) EndGame();
        else
        {
            MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.readyToNextLevel);

            return true;
            // TODO : continue after click and show animation/vizualization finding path
        }
        return false;
    }

    public void NextLevel()
    {
        _currentGameLevel++;
        GenerateMap();
        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.freeview);
    }

    public int GetScore() => _currentGameScore;
    public int GetLevel() => _currentGameLevel;

}
