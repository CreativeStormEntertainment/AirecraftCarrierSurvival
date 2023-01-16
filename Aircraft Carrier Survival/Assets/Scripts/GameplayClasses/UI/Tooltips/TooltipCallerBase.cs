using System;
using UnityEngine;

public class TooltipCallerBase : MonoBehaviour
{
    public event Action Disabled = delegate { };

    protected event Action ParamsChanged = delegate { };

    protected float TooltipShowDelay => tooltipShowDelay;

    protected Coroutine showTooltipCoroutine = null;

    [SerializeField]
    protected bool isAnimated = default;

    [SerializeField]
    protected float tooltipShowDelay = 0.7f;

    public string TitleID;

    public string DescriptionID;

    public GameObject GameObj
    {
        get;
        private set;
    }

    protected string title;

    protected string description;

    protected bool isShowing;

    [SerializeField]
    protected bool HUDMode = true;

    protected LocalizationManager locMan;

    protected virtual void Awake()
    {
        ParamsChanged += OnParamsChanged;
        GameObj = gameObject;
    }

    private void Start()
    {
        locMan = LocalizationManager.Instance;
        OnParamsChanged();
    }

    public void FireParamsChanged()
    {
        ParamsChanged();
    }

    public void OnExit()
    {
        if (showTooltipCoroutine != null)
        {
            StopCoroutine(showTooltipCoroutine);
            showTooltipCoroutine = null;
        }
        isShowing = false;
        Tooltip.Instance.Hide();
    }

    protected virtual void UpdateText()
    {
        var locMan = LocalizationManager.Instance;
        if (!string.IsNullOrWhiteSpace(TitleID))
        {
            title = locMan.GetText(TitleID);
        }
        if (DescriptionID == "-")
        {
            description = "-";
        }
        else if (!string.IsNullOrWhiteSpace(DescriptionID))
        {
            description = locMan.GetText(DescriptionID);
        }
    }

    private void OnParamsChanged()
    {
        UpdateText();
        if (isShowing)
        {
            Tooltip.Instance.UpdateText(title, description);
        }
    }

    private void OnDisable()
    {
        Disabled();
    }
}
