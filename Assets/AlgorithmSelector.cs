using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlgorithmSelector : MonoBehaviour
{
    public void SelectAlgorithm(VoteNavigationAlgorithm navigationAlgorithm)
    {
        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.none);
        bool isRight = MainGameManager.Instance.CheckSelectedAlgorithm(navigationAlgorithm);


        if (!isRight)
        {
            GetComponent<Image>().color = new Color(214, 37, 37, 255);
        }
        else
        {
            GetComponent<Image>().color = new Color(37, 214, 37, 255);

        }
    }
}
