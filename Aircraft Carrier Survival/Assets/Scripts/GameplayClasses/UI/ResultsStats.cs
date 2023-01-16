using UnityEngine;
using UnityEngine.UI;

public class ResultsStats : MonoBehaviour
{
    [SerializeField]
    private RectTransform trans = null;
    [SerializeField]
    private GameObject okBG = null;
    [SerializeField]
    private GameObject brokenBG = null;
    [SerializeField]
    private GameObject lostBG = null;
    [SerializeField]
    private GameObject japanFlag = null;
    [SerializeField]
    private Text shipName = null;

    [SerializeField]
    private float verticalSize = 30f;

    public void Setup(ref Vector2 pos, bool ok, bool broken, bool japan, string name)
    {
        trans.anchoredPosition = pos;
        okBG.SetActive(ok);
        brokenBG.SetActive(broken);
        lostBG.SetActive(!broken && !ok);
        japanFlag.SetActive(japan);
        shipName.text = name;
        pos.y -= verticalSize;
    }
}
