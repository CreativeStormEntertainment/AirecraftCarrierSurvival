using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class StrategyItemTooltip : MonoBehaviour
{
    public static StrategyItemTooltip Instance = null;

    [SerializeField]
    private Text strategyTitleText = null;

    [SerializeField]
    private Text torpedoText = null;
    [SerializeField]
    private Text bomberText = null;
    [SerializeField]
    private Text fighterText = null;

    [SerializeField]
    private Text strategyDescText = null;

    [SerializeField]
    private List<Text> valueTexts = null;
    [SerializeField]
    private Image radiusImage = null;
    [SerializeField]
    private Image fuelImage = null;

    [SerializeField]
    private List<Sprite> valueButtons = null;

    [SerializeField]
    private List<string> ourDamageTexts = null;
    [SerializeField]
    private List<string> ourLosesTexts = null;
    [SerializeField]
    private List<string> radiusAreasTexts = null;
    [SerializeField]
    private List<string> fuelTexts = null;


    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(Strategy strategy)
    {
        strategyTitleText.text = strategy.strategyName;
        torpedoText.text = strategy.torpedoCount.ToString() + "x";
        bomberText.text = strategy.bombersCount.ToString() + "x";
        fighterText.text = strategy.fightersCount.ToString() + "x";
        strategyDescText.text = strategy.strategyDescription;

        valueTexts[0].text = ourDamageTexts[strategy.damageBonusLvl];
        valueTexts[1].text = ourLosesTexts[strategy.loselBonusLvl];

        if (strategy.radius == 0f)
        {
            valueTexts[2].text = radiusAreasTexts[0];
            radiusImage.sprite = valueButtons[0];
        }
        else if (strategy.radius > 0)
        {
            valueTexts[2].text = radiusAreasTexts[1];
            radiusImage.sprite = valueButtons[1];
        }
        else
        {
            valueTexts[2].text = radiusAreasTexts[2];
            radiusImage.sprite = valueButtons[2];
        }

        if (strategy.fuel == 0f)
        {
            valueTexts[3].text = fuelTexts[0];
            fuelImage.sprite = valueButtons[0];
        }
        else if (strategy.fuel > 0)
        {
            valueTexts[3].text = fuelTexts[1];
            fuelImage.sprite = valueButtons[1];
        }
        else
        {
            valueTexts[3].text = fuelTexts[2];
            fuelImage.sprite = valueButtons[2];
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
