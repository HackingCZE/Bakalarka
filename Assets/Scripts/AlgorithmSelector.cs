using UnityEngine;

public class AlgorithmSelector : MonoBehaviour
{
    public void SelectAlgorithm(VoteNavigationAlgorithm navigationAlgorithm)
    {
        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.none);
        MainGameManager.Instance.CheckSelectedAlgorithm(navigationAlgorithm);

    }
}
