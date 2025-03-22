using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        if(scene.name == "GameScene") _ = StartCoroutine(StartGame());
    }

    public IEnumerator StartGame()
    {
        _currentGameLives = _startLives;
        _currentGameScore = 0;
        _currentGameLevel = 1;
        LoadingSceneManager.Instance.ShowLoading();

        yield return GenerateMap();

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
        _spreadAlgorithms.Clear();
        PlayerManager.Instance.SetScore(GetTotalScore());
    }

    private IEnumerator GenerateMap()
    {
        //SimpleVisualizer.Instance.Create();
        LSystemVisualizer.Instance.VisualizeMap();

        float progressValue = 0;
        var progress = new Progress<float>(value =>
        {
            progressValue = value;
            LoadingSceneManager.Instance.UpdateProgressBar(progressValue);
        });

        Task<List<AlgorithmStats>> task = NavigationManager.Instance.GetOrderOfAlgorithms(progress);

        while(!task.IsCompleted)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1);

        _algorithmStats = task.Result;
        CameraController.Instance.CalculateBounds(LSystemVisualizer.Instance.GetNodesAsVector());

        foreach(var item in FindObjectsOfType<VoteNavigationAlgorithm>(true))
        {
            try
            {
                item.indicator.color = MainGameManager.Instance.transform.GetComponent<SpreadAlgorithms>()._algorithms.Where(e => e.algorithm == item.navigationAlgorithm).ToList()[0].color;
                item.countText.text = _algorithmStats.Where(e => e.Algorithm == item.navigationAlgorithm).ToList()[0].VisitedNodes.Count.ToString();
            }
            catch { }
        }

        Debug.Log(_algorithmStats[0].Algorithm.ToString());
    }


    public void CheckSelectedAlgorithm(VoteNavigationAlgorithm navigationAlgorithm)
    {
        var rightAlgorithms = NavigationManager.Instance.GetRightAlgorithms(_algorithmStats);
        if(rightAlgorithms.Any(e => e.Algorithm == navigationAlgorithm.navigationAlgorithm))
        {
            _currentGameScore += 1 * Countdown.Instance.GetLastRemaining();
            PopUpText.Instance.ShowText("RIGHT", Color.green, 1.5f);
        }
        else
        {
            _currentGameLives--;
            PopUpText.Instance.ShowText("BAD", Color.red, 1.5f);
        }

        MainGameManagerUI.Instance.UpdateBtns(rightAlgorithms.ConvertAll<NavigationAlgorithm>(e => e.Algorithm).ToList());
        MainGameManagerUI.Instance.UpdateScoreUI(_currentGameScore);
        OnEnergyChange?.Invoke(_currentGameLives);

        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.readyToNextLevel);
        //Debug.Log("SPREAD");

        _spreadAlgorithms.SpreadOnAxis(_algorithmStats);

        if(_currentGameLives <= 0) EndGame();
    }

    public void AddLevel() => _currentGameLevel++;

    public IEnumerator NextLevel()
    {
        _spreadAlgorithms.Clear();
        _currentGameLevel++;
        LoadingSceneManager.Instance.ShowLoading();

        yield return new WaitForSeconds(1f);

        yield return GenerateMap();

        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.freeview);
    }

    public int GetScore() => _currentGameScore;
    public int GetLevel() => _currentGameLevel;
    public int GetTotalScore() => _currentGameScore + _currentGameLevel * 5;

}
