using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static NavigationManager;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager Instance { get; private set; }
    [SerializeField] List<AlgorithmStats> _algorithmStats = new List<AlgorithmStats>();
    [SerializeField] SpreadAlgorithms _spreadAlgorithms;

    public Action<int> OnEnergyChange;

    int _currentGameScore = 0;
    int _currentGameLives = 0;
    int _currentGameLevel = 0;
    [SerializeField] int _startLives = 3;

    private void Awake()
    {
        if(Instance == null)
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
        if(scene.name == "GameScene") StartGame();
    }

    public void StartGame()
    {
        _currentGameLives = _startLives;
        _currentGameScore = 0;
        _currentGameLevel = 1;
        GenerateMap();

        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.freeview);
        MainGameManagerUI.Instance.UpdateScoreUI(_currentGameScore);
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.UpArrow)) _currentGameLevel++;
        else if(Input.GetKey(KeyCode.DownArrow)) _currentGameLevel--;
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) Debug.LogError("Current LEVEL: " + _currentGameLevel);
    }

    public void EndGame()
    {
        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.end);
        PlayerManager.Instance.SetScore(GetTotalScore());
    }

    private async void GenerateMap()
    {
        //SimpleVisualizer.Instance.Create();
        LSystemVisualizer.Instance.VisualizeMap();
        _algorithmStats = await NavigationManager.Instance.GetOrderOfAlgorithms();
        CameraController.Instance.CalculateBounds(LSystemVisualizer.Instance.GetNodesAsVector());

        foreach(var item in FindObjectsOfType<VoteNavigationAlgorithm>(true))
        {
            item.indicator.color = MainGameManager.Instance.transform.GetComponent<SpreadAlgorithms>()._algorithms.Where(e => e.algorithm == item.navigationAlgorithm).ToList()[0].color;
        }

        Debug.Log(_algorithmStats[0].Algorithm.ToString());
    }

    public void CheckSelectedAlgorithm(VoteNavigationAlgorithm navigationAlgorithm)
    {
        if(_algorithmStats[0].Algorithm == navigationAlgorithm.navigationAlgorithm)
        {
            _currentGameScore += 1 * Countdown.Instance.GetLastRemaining();
            PopUpText.Instance.ShowText("RIGHT", Color.green, 1.5f);
        }
        else
        {
            _currentGameLives--;
            PopUpText.Instance.ShowText("BAD", Color.red, 1.5f);
        }

        MainGameManagerUI.Instance.UpdateBtns(_algorithmStats[0].Algorithm);
        MainGameManagerUI.Instance.UpdateScoreUI(_currentGameScore);
        OnEnergyChange?.Invoke(_currentGameLives);

        if(_currentGameLives <= 0) EndGame();
        else
        {
            MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.readyToNextLevel);

            _spreadAlgorithms.SpreadOnAxis(_algorithmStats);
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
    public int GetTotalScore() => _currentGameScore + _currentGameLevel * 5;

}
