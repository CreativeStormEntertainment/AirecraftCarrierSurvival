using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Transform objectToShow = null;

    [SerializeField]
    private float delay = 1f;

    private Coroutine coroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(WaitToShow());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        objectToShow.gameObject.SetActive(false);
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    IEnumerator WaitToShow()
    {
        yield return new WaitForSeconds(delay);
        objectToShow.gameObject.SetActive(true);
    }
}
