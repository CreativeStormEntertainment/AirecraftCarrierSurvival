using System;

[Serializable]
public class ReportData
{
    [NonSerialized]
    public bool HasShip;
    public string GoodText;
    public string BadText;
}
