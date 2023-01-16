using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntermissionOfficerPortrait : MonoBehaviour
{
    public GameObject Highlight => highlight;
    public GameObject DragHighlight => dragHighlight;

    [SerializeField]
    private OfficerList officerList = null;
    [SerializeField]
    protected Image levelImage = null;
    [SerializeField]
    protected GameObject highlight = null;
    [SerializeField]
    protected GameObject dragHighlight = null;
    [SerializeField]
    protected Text medalsCount = null;
    [SerializeField]
    protected List<Sprite> levelFrames = null;
    [SerializeField]
    private Image officerImage = null;
    [SerializeField]
    private GameObject navyImage = null;
    [SerializeField]
    private GameObject airImage = null;
    [SerializeField]
    private Text navyPoints = null;
    [SerializeField]
    private Text airPoints = null;

    private OfficerUpgrades upgrade;

    public void SetupOfficer(OfficerUpgrades intermissionData, int index)
    {
        upgrade = intermissionData;
        var data = officerList.Officers[index];
        var portraitData = officerList.Portraits[data.PortraitNumber];
        officerImage.sprite = portraitData.Square;
        bool showNavy = data.HasNavy;
        navyImage.gameObject.SetActive(showNavy);
        airImage.gameObject.SetActive(!showNavy);

        if (showNavy)
        {
            navyPoints.text = (data.NavyLvl + upgrade.UpgradedNavyPoints).ToString();
        }
        else
        {
            airPoints.text = (data.AirLvl + upgrade.UpgradedAirPoints).ToString();
        }

        UpdateLevelVisualization();
    }

    public void UpdateLevelVisualization()
    {
        int level = upgrade.GetLevel(officerList.OfficerLevelThreshold);
        if (level > 0)
        {
            levelImage.sprite = levelFrames[level - 1];
        }
        else
        {
            levelImage.sprite = levelFrames[0];
        }
        medalsCount.text = upgrade.Medals.ToString();
    }
}
