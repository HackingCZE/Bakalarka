using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    public static Countdown Instance { get; private set; }
    [SerializeField] float _timeLeft;
    [SerializeField] TMP_Text _text;

    float _lastRemaining = 0;

    bool _isOn = false;

    private void Awake()
    {
        Instance = this;
    }



    public void StartTimer(float seconds)
    {
        _timeLeft = seconds;
        UpdateTimer(_timeLeft);
        _isOn = true;
        CameraController.Instance.SetIsStatic(false);
    }


    public void EndTimer()
    {
        _lastRemaining = _timeLeft;
        _timeLeft = 0;
        _isOn = false;
        MainGameManagerUI.Instance.SwitchState(MainGameManagerUI.UIStates.selection);
    }

    private void Update()
    {
        if(_isOn)
        {
            if(_timeLeft > 0)
            {
                _timeLeft -= Time.deltaTime;
                UpdateTimer(_timeLeft + 1);
            }
            else
            {
                EndTimer();
            }
        }
    }

    void UpdateTimer(float currentTime)
    {
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        _text.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    public int GetLastRemaining()
    {
        return (int)_lastRemaining > 0 ? (int)_lastRemaining : 1;
    }
}
