using System;
using UnityEngine;

[Serializable]
public struct DayTime
{
    public int Year;
    public int Month;
    public int Day;
    public int Hour;
    public int Minute;

    public DayTime(int year, int month, int day, int hour, int minute)
    {
        Year = year;
        Month = month;
        Day = day;
        Hour = hour;
        Minute = minute;
    }
}
