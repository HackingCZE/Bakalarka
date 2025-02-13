using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlgorithmSelector : MonoBehaviour
{
    public void SelectAlgorithm(VoteNavigationAlgorithm navigationAlgorithm)
    {
        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.none);
        MainGameManager.Instance.CheckSelectedAlgorithm(navigationAlgorithm);

    }
}
