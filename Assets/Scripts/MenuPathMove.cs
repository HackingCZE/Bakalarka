using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPathMove : MonoBehaviour
{
    public static MenuPathMove Instance;
    public List<TrailRenderer> trails = new();
    public List<SpreadAlgorithms.Spread> paths = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MainGameManager.Instance.GetComponent<SpreadAlgorithms>().MainMenuPaths();
    }
}

