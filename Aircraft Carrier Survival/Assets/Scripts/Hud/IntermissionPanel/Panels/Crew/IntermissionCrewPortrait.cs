using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntermissionCrewPortrait : MonoBehaviour
{
    public GameObject Highlight => highlight;
    public GameObject DragHighlight => dragHighlight;

    [SerializeField]
    private CrewDataList dataList = null;
    [SerializeField]
    private Image portrait = null;
    [SerializeField]
    private GameObject highlight = null;
    [SerializeField]
    protected GameObject dragHighlight = null;
    [SerializeField]
    private Image specAImage = null;
    [SerializeField]
    private Image specBImage = null;
    [SerializeField]
    private Text medals = null;
    [SerializeField]
    protected Image levelImage = null;
    [SerializeField]
    protected List<Sprite> levelFrames = null;

    public void Setup(CrewUpgradeSaveData data)
    {
        portrait.sprite = dataList.List[data.CrewDataIndex].Portrait;
        var savedSpecialities = new List<ECrewmanSpecialty>(data.GetSpecialties());
        specAImage.transform.parent.gameObject.SetActive(false);
        specBImage.transform.parent.gameObject.SetActive(false);
        var locMan = LocalizationManager.Instance;
        if (savedSpecialities.Count > 0)
        {
            var specData = dataList.CrewSpecialtyDatas.Find(item => item.Speciality == savedSpecialities[0]);
            if (specData != null)
            {
                specAImage.sprite = specData.Icon;
                specAImage.transform.parent.gameObject.SetActive(true);
            }
        }
        if (savedSpecialities.Count > 1)
        {
            var specDataB = dataList.CrewSpecialtyDatas.Find(item => item.Speciality == savedSpecialities[1]);
            if (specDataB != null)
            {
                specBImage.sprite = specDataB.Icon;
                specBImage.transform.parent.gameObject.SetActive(true);
            }
        }
        medals.text = data.Medals.ToString();
        levelImage.sprite = levelFrames[Mathf.Min(data.Medals, 2)];
    }
}
