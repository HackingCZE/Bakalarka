using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    public static LoadingSceneManager Instance;
    [SerializeField] GameObject _loadingScreen;
    [SerializeField] Slider _loadingSlider;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadScene(string sceneName)
    {
        _loadingSlider.value = 0;
        _loadingScreen.gameObject.SetActive(true);

        StartCoroutine(LoadSceneASync(sceneName));
    }

    private IEnumerator LoadSceneASync(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncOperation.isDone)
        {
            _loadingSlider.value = Mathf.Clamp01(asyncOperation.progress / .9f);
            yield return null;
        }

        _loadingScreen.gameObject.SetActive(false);
    }
}
