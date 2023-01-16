using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPlaneColor : MonoBehaviour
{
    public EPlaneType planeType;

    [SerializeField]
    private List<Material> materialsForA = null;
    [SerializeField]
    private List<Material> materialsForB = null;
    [SerializeField]
    private List<Material> materialsForC = null;

    [SerializeField]
    private UpgradePlanesButton upgradeButton = null;

    [SerializeField]
    private Button button = null;

    [SerializeField]
    private Sprite on = null;
    [SerializeField]
    private Sprite off = null;

    [SerializeField]
    private int maxColors = 5;

    private List<Image> toggles;

    private int currentColor;

    AircraftSubpanel aircraftPanel;

    public void Setup(AircraftSubpanel aircraftPanel)
    {
        this.aircraftPanel = aircraftPanel;
        toggles = new List<Image>();

        var trans = button.transform;
        button.onClick.AddListener(() => SetColor(0));
        toggles.Add(trans.GetComponent<Image>());

        for (int i = 1; i < maxColors; i++)
        {
            var button2 = Instantiate(button, trans.parent);
            int color = i;
            button2.onClick.AddListener(() => SetColor(color));
            toggles.Add(button2.transform.GetComponent<Image>());
        }
        currentColor = SaveManager.Instance.Data.PlaneColorIndices.Find(x => x.Type == planeType).Value;
        SetColor(currentColor);
    }

    public void SetColor(int color)
    {
        currentColor = color;
        SetPlaneMaterial(upgradeButton.PlanesTypeA, materialsForA[currentColor]);
        SetPlaneMaterial(upgradeButton.PlanesTypeB, materialsForB[currentColor]);
        SetPlaneMaterial(upgradeButton.PlanesTypeC, materialsForC[currentColor]);

        foreach (var toggle in toggles)
        {
            toggle.sprite = off;
        }
        toggles[currentColor].sprite = on;

        aircraftPanel.UpdatePlaneColor(planeType, currentColor);
    }

    private void SetPlaneMaterial(List<GameObject> planes, Material material)
    {
        foreach (var plane in planes)
        {
            foreach (var renderer in plane.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.sharedMaterial.name.Contains("glass"))
                {
                    renderer.material = material;
                }
            }
        }
    }
}
