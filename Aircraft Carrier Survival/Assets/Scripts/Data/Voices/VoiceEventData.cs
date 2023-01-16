using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceData", menuName = "Datas/Voice events data", order = 1)]
public class VoiceEventData : ScriptableObject
{
    [EventRef]
    public string SelectEvent;
    [EventRef]
    public string PositiveEvent;
    [EventRef]
    public string NegativeEvent;
}
