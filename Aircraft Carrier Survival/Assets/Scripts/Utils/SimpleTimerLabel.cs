using UnityEngine;
using UnityEngine.UI;

public class SimpleTimerLabel : MonoBehaviour
{
	public static SimpleTimerLabel Instance;

	[SerializeField]
	private Text hourLabel = null;
	[SerializeField]
	private Text minuteLabel = null;
	[SerializeField]
	private GameObject root = null;
	[SerializeField]
	private Text timerText = null;
	[SerializeField]
	private TooltipCaller caller = null;

	private float timer;

	private string id;
	private string ttipTitleId;
	private string ttipDescId;

	private void Awake()
	{
		Instance = this;

		root.SetActive(false);
	}

	private void Update()
	{
		if (timer > 0f)
		{
			timer = Mathf.Max(0f, timer - Time.deltaTime);
			UpdateTimer(timer);
		}
	}

	public void LoadData(ref TimerSaveData data)
	{
		if (data.Time > 0f)
		{
			StartTimer(10, data.TextId, data.TooltipTitleId, data.TooltipDescId);
			timer = data.Time;
		}
	}

	public void SaveData(ref TimerSaveData data)
	{
		if (root.activeSelf)
		{
			data.Time = timer;
			data.TextId = id;
			data.TooltipTitleId = ttipTitleId;
			data.TooltipDescId = ttipDescId;
		}
		else
		{
			data.Time = -1f;
		}
	}

	public void StartTimer(int time, string textID, string tooltipTitleID, string tooltipDescID)
	{
		root.SetActive(true);
		timer = time;

		id = textID;
		ttipTitleId = tooltipTitleID;
		ttipDescId = tooltipDescID;

		var locMan = LocalizationManager.Instance;
		timerText.text = locMan.GetText(textID);
		if (string.IsNullOrWhiteSpace(tooltipTitleID))
		{
			caller.enabled = false;
		}
		else
		{
			caller.enabled = true;
			caller.SetTitles(tooltipTitleID, tooltipDescID);
		}
	}

	public void Stop()
	{
		root.SetActive(false);
	}

	public void UpdateTimer(float time)
	{
		time /= TimeManager.Instance.TicksForHour;

		int hours = (int)time;
		hourLabel.text = hours.ToString("00");

		int minutes = (int)((time - hours) * 60f);
		minuteLabel.text = minutes.ToString("00");
	}
}