using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector3 _startScale;

    Vector3 _target;

    private void Start()
    {
        _startScale = transform.localScale;
        _target = _startScale;
    }

    private void LateUpdate()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _target, 10f * Time.deltaTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _target = _startScale + new Vector3(.1f, .1f, .1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _target = _startScale;

    }
}
