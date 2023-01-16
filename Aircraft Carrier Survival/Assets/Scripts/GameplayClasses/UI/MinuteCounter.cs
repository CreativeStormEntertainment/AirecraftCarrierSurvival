using UnityEngine;
using UnityEngine.Events;

public class MinuteCounter
{
    private int savedMinutes;

    public int Count(TimeManager timeManager, UnityAction action, int minutes)
    {
        int savedMinute = savedMinutes;
        int currentMinute = timeManager.CurrentMinute;
        if (savedMinute < 30)
        {
            savedMinute += 60;
        }
        if (currentMinute < 30)
        {
            currentMinute += 60;
        }
        currentMinute -= savedMinute;
        if (Mathf.Abs(currentMinute) < 5)
        {
            savedMinutes = timeManager.CurrentMinute;
            minutes -= currentMinute;
            minutes = Mathf.Max(minutes, 0);
            action?.Invoke();
        }
        else
        {
            savedMinutes = 0;
            minutes = 0;
        }
        return minutes;
    }
}
