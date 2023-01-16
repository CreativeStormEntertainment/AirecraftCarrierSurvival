using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyLastSeen : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private GameObject highlightImage = null;

    [NonSerialized]
    public int SeenTimer;

    [NonSerialized]
    public TacticalEnemyMapButton Fleet;

    public RectTransform Transform;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OpenIdentifyPanel);
    }
    private void OnDisable()
    {
        SwitchHighlight(false);
        StopAllCoroutines();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        SwitchHighlight(false);
    }

    public void HighlightPosition(float time)
    {
        StartCoroutine(DelayedHighlightShutoff(time));
    }


    private void OpenIdentifyPanel()
    {
        TacticManager.Instance.ShowIdentificationPanel(Fleet);
    }

    private void SwitchHighlight(bool isShow)
    {
        highlightImage.SetActive(isShow);
    }

    private IEnumerator DelayedHighlightShutoff(float time)
    {
        SwitchHighlight(true);
        yield return new WaitForSecondsRealtime(time);
        SwitchHighlight(false);
    }
}
