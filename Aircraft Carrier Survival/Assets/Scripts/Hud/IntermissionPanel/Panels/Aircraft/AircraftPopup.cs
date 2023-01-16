using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AircraftPopup : MonoBehaviour
{
    public event Action<EPlaneType, int> CurrentAircraftChanged = delegate { };

    public bool Shown => gameObject.activeSelf;
    public EPlaneType CurrentType
    {
        get;
        set;
    }

    [SerializeField]
    private Counter counter = null;
    [SerializeField]
    private Button cancel = null;
    [SerializeField]
    private Button confirm = null;
    [SerializeField]
    private Text title = null;
    [SerializeField]
    private List<AircraftPopupData> datas = null;

    private int minValue;
    private int maxValue;
    private Dictionary<EPlaneType, AircraftPopupData> dict;
    private RectTransform trans;

    public void Show(EPlaneType type, int minValue, int maxValue)
    {
        Init();

        CurrentType = type;
        this.minValue = minValue;
        this.maxValue = maxValue;

        counter.Value = Mathf.Clamp(counter.Value, minValue, maxValue);

        var data = dict[type];
        trans.anchoredPosition = data.Position.anchoredPosition;
        title.text = data.TitleID;

        gameObject.SetActive(true);
        confirm.interactable = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Init()
    {
        if (dict == null)
        {
            counter.Init(OnCounterValueChanged);
            cancel.onClick.AddListener(Hide);
            confirm.onClick.AddListener(() =>
            {
                CurrentAircraftChanged(CurrentType, counter.Value);
                Hide();
            });

            dict = new Dictionary<EPlaneType, AircraftPopupData>();
            var locMan = LocalizationManager.Instance;
            foreach (var data in datas)
            {
                data.TitleID = locMan.GetText(data.TitleID);
                dict[data.Type] = data;
            }

            trans = GetComponent<RectTransform>();
        }
    }

    private void OnCounterValueChanged(int value)
    {
        value = Mathf.Clamp(value, minValue, maxValue);
        counter.Value = value;
        confirm.interactable = value != 0;
    }
}
