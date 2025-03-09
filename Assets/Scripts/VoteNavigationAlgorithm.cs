using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static NavigationManager;

public class VoteNavigationAlgorithm : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public NavigationAlgorithm navigationAlgorithm;
    public Image indicator;
    public TextMeshProUGUI countText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        MainGameManager.Instance.GetComponent<TubeMouseDetector>().Highlight(navigationAlgorithm.ToString(), true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MainGameManager.Instance.GetComponent<TubeMouseDetector>().Highlight(navigationAlgorithm.ToString(), false);
    }
}
