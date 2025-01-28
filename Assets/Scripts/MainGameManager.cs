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
    [SerializeField] SpreadAlgorithms _spreadAlgorithms;
    

    int _currentGameScore = 0;
    int _currentGameLives = 0;
    int _currentGameLevel = 0;
    [SerializeField] int _startLives = 3;

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

    public void ClearAlgos()
    {
        _spreadAlgorithms.Clear();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") StartGame();
    }

    public void StartGame()
    {
        _currentGameLives = _startLives;
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
         //SimpleVisualizer.Instance.Create();
         LSystemVisualizer.Instance.VisualizeMap();
        _algorithmStats = await NavigationManager.Instance.GetOrderOfAlgorithms();
        CameraController.Instance.CalculateBounds(LSystemVisualizer.Instance.GetNodesAsVector());

        Debug.Log(_algorithmStats[0].Algorithm.ToString());
    }

    public void CheckSelectedAlgorithm(VoteNavigationAlgorithm navigationAlgorithm)
    {
        if (_algorithmStats[0].Algorithm == navigationAlgorithm.navigationAlgorithm) _currentGameScore += 1 * Countdown.Instance.GetLastRemaining();
        else _currentGameLives--;

        MainGameManagerUI.Instance.UpdateBtns(_algorithmStats[0].Algorithm);
        MainGameManagerUI.Instance.UpdateScoreUI(_currentGameScore);

        if (_currentGameLives <= 0) EndGame();
        else
        {
            MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.readyToNextLevel);

            _spreadAlgorithms.SpreadOnAxis(_algorithmStats);

            // TODO : continue after click and show animation/vizualization finding path
        }
    }

    public void NextLevel()
    {
        _spreadAlgorithms.Clear();
        _currentGameLevel++;
        GenerateMap();
        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.freeview);
    }

    public int GetScore() => _currentGameScore;
    public int GetLevel() => _currentGameLevel;

}
