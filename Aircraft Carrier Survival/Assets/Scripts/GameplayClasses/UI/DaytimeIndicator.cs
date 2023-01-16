using UnityEngine;
using UnityEngine.UI;

public class DaytimeIndicator : MonoBehaviour
{
    [SerializeField]
    private Text hourText = null;
    //[SerializeField]
    //private Text yearText = null;
    //[SerializeField]
    //private Text monthText = null;
    [SerializeField]
    private Text dayText = null;
    [SerializeField]
    private Image dayNightIcon = null;
    [SerializeField]
    private Sprite dayIcon = null;
    [SerializeField]
    private Sprite nightIcon = null;

    private string currentHourText = "";

    private TimeManager timeManager;

    private void Awake()
    {
        timeManager = TimeManager.Instance;
        timeManager.MinutePassed += OnMinutePassed;
        timeManager.HourPassed += SetTime;
        timeManager.DateChanged += OnDateChanged;
    }

    private void OnEnable()
    {
        SetTime();
        OnDateChanged();
    }

    private void OnMinutePassed()
    {
        hourText.text = currentHourText + timeManager.CurrentMinute.ToString("00");
    }

    private void SetTime()
    {
        currentHourText = timeManager.CurrentHour.ToString() + ":";
        dayNightIcon.sprite = timeManager.IsDay ? dayIcon : nightIcon;
        OnMinutePassed();
    }

    private void OnDateChanged()
    {
        dayText.text = timeManager.CurrentMonth.ToString("00") + "." + timeManager.CurrentDay.ToString("00") + "." + timeManager.CurrentYear.ToString();
        //monthText.text = timeManager.CurrentMonth.ToString();
        //yearText.text = timeManager.CurrentYear.ToString();
    }
}
