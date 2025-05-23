using LootLocker.Requests;
using System.Collections;
using TMPro;
using UnityEngine;

public class LootLockerManager : MonoBehaviour
{
    bool logged = false;
    [SerializeField] string nickname;
    public TMP_InputField nicknameField;
    [SerializeField] string _toScene = "MainMenu";


    private void Start()
    {
        StartCoroutine(LoginRoutine());
    }

    public void FirstLogin()
    {
        PlayerManager.Instance.SetPlayerName(nicknameField.GetComponent<TMP_InputField>().text, LoadMainMenu);
    }

    private void LoadMainMenu()
    {
        LoadingSceneManager.Instance.LoadScene(_toScene);
    }

    public void SecondLogin()
    {
        PlayerManager.Instance.SetLogged(logged, nickname);
        LoadingSceneManager.Instance.LoadScene(_toScene);
    }

    [ContextMenu("reset player")]
    public void ResetPlayer()
    {
        PlayerPrefs.DeleteKey("PlayerID");
    }

    IEnumerator LoginRoutine()
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if(response.success)
            {
                Debug.Log("Player was logged in");
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());

                logged = true;
                LootLockerSDKManager.GetPlayerName((playerName) =>
                {
                    nickname = playerName.name;
                    if(playerName.name != "")
                    {
                        SecondLogin();
                    }
                });
                done = true;
            }
            else
            {
                Debug.Log("Could not start session");
                done = false;
            }
        });
        yield return new WaitWhile(() => done == false);
    }
}
