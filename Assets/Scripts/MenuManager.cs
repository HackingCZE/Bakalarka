using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        MainGameManager.Instance.ClearAlgos();
        LoadingSceneManager.instance.LoadScene("GameScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
