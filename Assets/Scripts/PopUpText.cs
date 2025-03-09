using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PopUpText : MonoBehaviour
{
    public static PopUpText Instance;
    [SerializeField] float startSize;
    [SerializeField] float endSize;
    [SerializeField] float time;
    [SerializeField] Image background;
    [SerializeField] TextMeshProUGUI backgroundText;
    [SerializeField] TextMeshProUGUI foregroundText;

    private void Awake()
    {
        Instance = this;
    }

    public async void ShowText(string text, Color color, float time = 1.1f, System.Action callback = null)
    {
        foregroundText.text = text;
        backgroundText.text = text;

        foregroundText.color = color;
        backgroundText.color = new Color(color.r, color.g, color.b, .3f);

        foregroundText.fontSize = startSize;
        backgroundText.fontSize = startSize * 2.3f;

        background.gameObject.SetActive(true);

        await AnimateTextWithCallback(time);

        background.gameObject.SetActive(false);
        callback?.Invoke();
    }


    Coroutine _lastCoroutine;

    private async Task AnimateTextWithCallback(float time)
    {
        if (_lastCoroutine != null) StopCoroutine(_lastCoroutine);

        var tcs = new TaskCompletionSource<bool>();

        _lastCoroutine = StartCoroutine(AnimateText(tcs, time));

        await tcs.Task;
    }

    private IEnumerator AnimateText(TaskCompletionSource<bool> tcs, float time)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / time);
            foregroundText.fontSize = Mathf.Lerp(startSize, endSize, progress);
            backgroundText.fontSize = foregroundText.fontSize * 2.5f;
            yield return null;
        }

        tcs.SetResult(true);

    }
}
