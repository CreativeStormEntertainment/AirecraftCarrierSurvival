using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    public DayTime DayTime => dayTime;
    [SerializeField]
    private DayTime dayTime = default;
}
