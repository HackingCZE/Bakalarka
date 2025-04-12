using LootLocker.Requests;
using System.Collections;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;
    [SerializeField] private GameObject _leaderboardPrefab;
    [SerializeField] private GameObject _leaderboardParent;
    [SerializeField] int leaderboardID = 19556;

    private void Awake()
    {
        Instance = this;
    }

    [ContextMenu("get leadeboad")]
    public void GetLeaderboard()
    {
        StartCoroutine(FetchTopHigscoresRoutine());
    }

    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
        bool done = false;
        string playerID = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, leaderboardID.ToString(), (response) =>
        {
            if(response.success)
            {
                Debug.Log("Successfully uploaded score");

                done = true;
            }
            else
            {
                //Debug.Log("Failed " + response.errorData);
                done = false;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    public IEnumerator FetchTopHigscoresRoutine()
    {
        bool done = false;

        LootLockerSDKManager.GetScoreList(leaderboardID.ToString(), 30, 0, (response) =>
        {
            if(response.success)
            {
                done = true;
                Debug.Log("Successfully got score list!");

                DeastroyAllLeaderboardItems();
                if(response.items != null)
                {
                    int count = Mathf.Min(50, response.items.Length);
                    _leaderboardParent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 115 * count);
                    for(int i = 0; i < count; i++)
                    {
                        var currentItem = response.items[i];
                        var obj = Instantiate(_leaderboardPrefab, _leaderboardParent.transform);
                        obj.GetComponent<LeaderboardItem>().SetValues(currentItem.rank.ToString(), currentItem.player.name, currentItem.score.ToString());
                    }
                }
            }
            else
            {
                done = false;
                Debug.Log("Could not get score list!");
                //Debug.Log(response.errorData.ToString());
            }

        });
        yield return new WaitWhile(() => done == false);
    }

    private void DeastroyAllLeaderboardItems()
    {
        foreach(Transform child in _leaderboardParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ReloadLeaderboard()
    {
        StartCoroutine(FetchTopHigscoresRoutine());
    }


    public IEnumerator GetCurrentScore()
    {
        bool done = false;
        LootLockerSDKManager.GetMemberRank(leaderboardID.ToString(), PlayerPrefs.GetString("PlayerID"), (response) =>
        {
            if(response.success)
            {
                PlayerManager.Instance.SetScore(response.score, false);
                done = true;
            }
            else
            {
                //Debug.Log("Failed " + response.errorData);
                done = true;
            }

        });
        yield return new WaitWhile(() => done == false);
    }
}
