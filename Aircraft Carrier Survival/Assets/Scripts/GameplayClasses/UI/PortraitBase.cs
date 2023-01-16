using GambitUtils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PortraitBase : MonoBehaviour, IEnableable
{
    protected static readonly int selectedBool = Animator.StringToHash("Selected");
    protected static readonly int normalTrigger = Animator.StringToHash("Normal");

    [FormerlySerializedAs("animator")]
    public Animator Animator = null;

    protected bool isClicked;
    protected Coroutine doubleClickCoroutine;

    protected bool disabled;

    [SerializeField]
    private Button button = null;

    public virtual void SetEnable(bool enable)
    {
        disabled = !enable;
        button.enabled = enable;
    }

    public void OnClick()
    {
        if (isClicked)
        {
            isClicked = false;
            if (doubleClickCoroutine != null)
            {
                StopCoroutine(doubleClickCoroutine);
                doubleClickCoroutine = null;
            }
            DoubleClick();
        }
        else
        {
            isClicked = true;
            doubleClickCoroutine = this.StartCoroutineActionAfterRealtime(() =>
            {
                isClicked = false;
                doubleClickCoroutine = null;
            }, .5f);
            Click();
        }
    }

    protected virtual void Click()
    {

    }

    protected virtual void DoubleClick()
    {

    }

    public virtual void SetSelected(bool value)
    {
        Animator.SetBool(selectedBool, value);
        if (!value)
        {
            Animator.SetTrigger(normalTrigger);
        }
    }
}
