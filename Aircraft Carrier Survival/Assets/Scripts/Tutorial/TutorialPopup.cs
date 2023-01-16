using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    public static bool ClicksBlocked = false;

    public Button PreviousButton => previousButton;

    public Button NextButton => nextButton;

    public AnimationCurve HighlightCurve => highlightCurve;

    public float HighlightTime => highlightTime;

    public RectTransform ClickBlocker => clickBlocker;

    protected bool hidePopup = false;

    [SerializeField]
    protected Button previousButton = null;

    [SerializeField]
    protected Button nextButton = null;

    [SerializeField]
    protected RectTransform clickBlocker = null;

    [SerializeField]
    protected Text pressAnyText = null;

    [SerializeField]
    private Text title = null;

    [SerializeField]
    private Text content = null;

    [SerializeField]
    private Image portrait;

    [SerializeField]
    private GameObject popupContent = null;

    [SerializeField]
    private Vector2 clickBlockerCutoutSize = new Vector2(10f, 10f);
    [SerializeField]
    private RectTransform clickBlockerCutoutPrefab = null;

    public AnimationCurve highlightCurve = null;

    public float highlightTime = 3;

    private float pressAnyTextTS = 0;

    [SerializeField]
    private Color pressAnyTextColor = Color.white;

    //[Header("VO")]
    //[SerializeField]
    //protected AudioSource audioSource;

    [SerializeField]
    private float fadeoutTime = 0.5f;

    [SerializeField]
    private float helperArrowDelay = 5f;
    [SerializeField]
    private float cursorDelayTime = 1f;
    [SerializeField]
    private float cursorMoveTime = 2f;

    [SerializeField]
    private TutorialArrow arrow = null;

    [SerializeField]
    private StudioEventEmitter emitter = null;
    [SerializeField]
    private NarratorSound narrator = null;

    private List<RectTransform> clickBlockerCutouts = new List<RectTransform>();

    private List<string> textHistory = new List<string>();
    private int historyPointer = 0;

    private void Start()
    {
        previousButton.onClick.AddListener(PreviousText);
        nextButton.onClick.AddListener(NextText);
    }

    private void LateUpdate()
    {
        ////if (helperArrow != EHelperArrow.None)
        ////{
        ////    helperTime += Time.deltaTime;
        ////    if (helperTime > helperArrowDelay)
        ////    {
        ////        helperArrow = EHelperArrow.None;
        ////        arrow.gameObject.SetActive(true);
        ////        if (helperArrow == EHelperArrow.Up)
        ////        {
        ////            arrow.ownRectTransform.rotation = Quaternion.identity;
        ////        }
        ////        else
        ////        {
        ////            arrow.ownRectTransform.rotation = Quaternion.Euler(0f, 0f, 90f);
        ////        }
        ////    }
        ////}

        if (pressAnyText.gameObject.activeInHierarchy)
        {
            pressAnyTextTS += Time.unscaledDeltaTime;
            if (pressAnyTextTS >= highlightTime)
            {
                pressAnyTextTS = 0f;
            }
            pressAnyTextColor.a = highlightCurve.Evaluate(pressAnyTextTS / highlightTime);
            pressAnyText.color = pressAnyTextColor;
        }
        if (hidePopup)
        {
            hidePopup = false;
            popupContent.SetActive(false);
            clickBlocker.gameObject.SetActive(false);
        }
        //if (isFadingOut)
        //{
        //    fadeoutTS += Time.unscaledDeltaTime;
        //    audioSource.volume = Mathf.Lerp(1f, 0f, fadeoutTS / fadeoutTime);
        //    if (fadeoutTS >= fadeoutTime)
        //    {
        //        isFadingOut = false;
        //        audioSource.Stop();
        //    }
        //}
    }

    public void Setup(string title, string text, EVoiceOver vo, bool showArrows, EHelperArrow helperArrow)
    {
        emitter.Play();
        //helperTransform.gameObject.SetActive(false);

        if (title == null)
        {
            this.title.text = LocalizationManager.Instance.GetText("TutorialTitle");
        } 
        else
        {
            this.title.text = LocalizationManager.Instance.GetText(title);
        }
        content.text = text;

        PlayVO(vo);

        if (!textHistory.Contains(text))
        {
            textHistory.Add(text);
        }

        historyPointer = textHistory.Count - 1;
        if (showArrows && textHistory.Count > 1)
        {
            nextButton.gameObject.SetActive(true);
            previousButton.gameObject.SetActive(true);
            nextButton.interactable = false;
            previousButton.interactable = true;
        } 
        else
        {
            nextButton.gameObject.SetActive(false);
            previousButton.gameObject.SetActive(false);
        }

        popupContent.SetActive(true);
    }

    public void HideAllClickBlockerCutouts()
    {
        for (int i = 0; i < clickBlockerCutouts.Count; ++i)
        {
            clickBlockerCutouts[i].gameObject.SetActive(false);
        }
    }

    public void SetClickBlockerCutout(int index, Transform parent)
    {
        while (index > clickBlockerCutouts.Count - 1)
        {
            var cutout = Instantiate(clickBlockerCutoutPrefab, transform);
            cutout.gameObject.SetActive(false);
            clickBlockerCutouts.Add(cutout);
        }
        clickBlockerCutouts[index].SetParent(parent);
        clickBlockerCutouts[index].offsetMin = clickBlockerCutoutSize;
        clickBlockerCutouts[index].offsetMax = -clickBlockerCutoutSize;
        clickBlockerCutouts[index].SetParent(transform.parent);
        clickBlockerCutouts[index].SetSiblingIndex(clickBlocker.GetSiblingIndex() - 1);
        clickBlockerCutouts[index].gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (ClicksBlocked)
        {
            hidePopup = true;
        }
        else
        {
            popupContent.SetActive(false);
            clickBlocker.gameObject.SetActive(false);
        }
        HideAllClickBlockerCutouts();
        FadeoutVO();
    }

    public void DisplayHelperArrow(TutorialStep step)
    {
        StopAllCoroutines();

		StartCoroutine(StartHelperArrowLoop(step));
    }

    public void HideTutorialArrow()
    {
        StopAllCoroutines();

        arrow.ownAnimator.enabled = false;
        arrow.gameObject.SetActive(false);
    }

    public void SetPressAny(bool state, bool textState, int siblingIndex)
    {
        clickBlocker.gameObject.SetActive(state);
        pressAnyText.gameObject.SetActive(textState);
        if (state)
        {
            clickBlocker.SetParent(transform.parent);
            transform.SetAsLastSibling();
            clickBlocker.sizeDelta = Vector2.zero;
            clickBlocker.anchoredPosition = Vector2.zero;
        }
        ClicksBlocked = state;
    }

    public void PlayVO(EVoiceOver vo)
    {
        narrator.PlayEvent(vo);
    }

    public void FadeoutVO()
    {
        narrator.Stop(true);
    }

    private IEnumerator StartHelperArrowLoop(TutorialStep step)
    {
		yield return new WaitForSecondsRealtime(helperArrowDelay);

		StartCoroutine(FadeHelperArrow(step));
	}

	private IEnumerator FadeHelperArrow(TutorialStep step)
	{
		arrow.ownRectTransform.position = step.arrowPositions.start.position;

        yield return null;

        arrow.ownAnimator.enabled = true;
        arrow.gameObject.SetActive(true);
        
        StartCoroutine(MoveArrow(step));
	}
    
    private IEnumerator MoveArrow(TutorialStep step)
	{
        float start = Time.realtimeSinceStartup;
		float timer;

		do
        {
            timer = Time.realtimeSinceStartup - start;

            arrow.ownRectTransform.position = Vector3.Lerp(step.arrowPositions.start.position, step.arrowPositions.end.position, timer / cursorMoveTime);

            yield return null;
        } 
        while (timer <= cursorMoveTime);

        StartCoroutine(DelayedRepeat(step));
	}

	private IEnumerator DelayedRepeat(TutorialStep step)
	{
        yield return new WaitForSecondsRealtime(fadeoutTime);

        arrow.gameObject.SetActive(false);
        arrow.ownAnimator.enabled = false;

        yield return new WaitForSecondsRealtime(cursorDelayTime);

        StartCoroutine(FadeHelperArrow(step));
	}

    private void PreviousText()
    {
        --historyPointer;
        nextButton.interactable = true;
        if (historyPointer <= 0)
        {
            previousButton.interactable = false;
        }
        content.text = LocalizationManager.Instance.GetText(textHistory[historyPointer]);
    }

    private void NextText()
    {
        ++historyPointer;
        previousButton.interactable = true;
        if (historyPointer >= textHistory.Count-1)
        {
            nextButton.interactable = false;
        }
        content.text = LocalizationManager.Instance.GetText(textHistory[historyPointer]);
    }
}
