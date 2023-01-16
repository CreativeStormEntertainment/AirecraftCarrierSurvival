using UnityEngine;
using UnityEngine.UI;

public class WorldMapRepairBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform rectTransform;
    [SerializeField]
    private Image background = null;
    [SerializeField]
    private Color[] colors = null;
    [SerializeField]
    private Text headText = null;
    [SerializeField]
    private Text stateText = null;
    [SerializeField]
    private Text timeText;
    [SerializeField]
    private Text selectText = null;

    [SerializeField]
    private Button repairButton = null;

    private SubSectionRoom subsection = null;
    private WorldMap worldMap;
    private int n;
    private float repairSpeed = 0;

    public void Setup(int n, WorldMap worldMap, SubSectionRoom subsection, float repairSpeed)
    {
        this.n = n;
        this.worldMap = worldMap;
        this.subsection = subsection;
        this.repairSpeed = repairSpeed;
        headText.text = subsection.Title + " Broken";
        repairButton.onClick.AddListener(Choose);
        Refresh();
    }

    public bool Refresh()
    {
        if (subsection.Destruction.Repair)
        {
            subsection.Destruction.RepairPower = repairSpeed;
            subsection.Destruction.Update();
        }
        stateText.text = (Mathf.Round(subsection.Destruction.RepairData.Percent * 10000f)/100f) + "%";
        bool isDone = subsection.Destruction.RepairData.Percent == 1f;
        if (isDone)
        {
            subsection.Destruction.Repair = false;
        }
        return isDone;
    }

    private void Choose()
    {
        subsection.Destruction.Repair = true;
        background.color = colors[1];
        repairButton.gameObject.SetActive(false);
        selectText.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        subsection.Destruction.Repair = false;
        background.color = colors[0];
        repairButton.gameObject.SetActive(true);
        selectText.gameObject.SetActive(false);
    }
}
