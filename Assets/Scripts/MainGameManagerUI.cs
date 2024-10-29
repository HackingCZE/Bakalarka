using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainGameManagerUI : MonoBehaviour
{
    public static MainGameManagerUI Instance { get; private set; }
    [SerializeField] Transform _btns;
    [SerializeField] TMP_Text _scoreText, _endText;
    [SerializeField] Transform _skipBtn, _mid, _end, _nextBtn;
    [SerializeField] Image _blur;
    [SerializeField] UIStates _currentState;

    private void Awake()
    {
        Instance = this;
        SwitchState(_currentState);
    }

    public enum UIStates
    {
        none,
        freeview,
        selection,
        readyToNextLevel,
        end
    }

    public void SwitchState(UIStates newState)
    {
        _currentState = newState;
        switch (_currentState)
        {
            case UIStates.none:
                DisableAll();
                _blur.enabled = true;
                UpdateScoreUI(MainGameManager.Instance.GetScore());
                break;
            case UIStates.freeview:
                DisableAll();
                _mid.gameObject.SetActive(true);
                _skipBtn.gameObject.SetActive(true);
                Countdown.Instance.StartTimer(30);
                break;
            case UIStates.selection:
                DisableAll();
                foreach(Button btn in _btns.GetComponentsInChildren<Button>())
                {
                    btn.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    btn.interactable = true;
                }
                _blur.enabled = true;
                _btns.gameObject.SetActive(true);
                break;
            case UIStates.readyToNextLevel:
                DisableAll();
                foreach (Button btn in _btns.GetComponentsInChildren<Button>())
                {
                    btn.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    btn.interactable = false;
                }
                _blur.enabled = true;
                _btns.gameObject.SetActive(true);
                _nextBtn.gameObject.SetActive(true);
                break;
            case UIStates.end:
                DisableAll();
                _end.gameObject.SetActive(true);
                _endText.text = "";
                _endText.text += "<b>Tvoje scoré:</b> " + MainGameManager.Instance.GetScore();
                _endText.text += "\n";
                _endText.text += "<b>Tvoje úroveò:</b> " + MainGameManager.Instance.GetLevel();
                break;
            default:
                break;
        }
    }

    public void UpdateScoreUI(int score)
    {
        _scoreText.text = score.ToString();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void NextLevel()
    {
        MainGameManager.Instance.NextLevel();
    }

    void DisableAll()
    {
        _blur.enabled = false;
        _end.gameObject.SetActive(false);
        _nextBtn.gameObject.SetActive(false);
        _btns.gameObject.SetActive(false);
        _skipBtn.gameObject.SetActive(false);
        _mid.gameObject.SetActive(false);
    }
}
