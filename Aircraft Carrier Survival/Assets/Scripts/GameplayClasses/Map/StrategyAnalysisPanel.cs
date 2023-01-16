using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrategyAnalysisPanel : MonoBehaviour
{
    [SerializeField] private Text predictedDamageText = null;
    [SerializeField] private Text predictedLossesText = null;

    public void SetPredictedDataTexts(FightModifiersData data)
    {
        SetPredictedDataTexts(
            Mathf.Clamp(data.DamageMin, 0, 100),
            Mathf.Clamp(data.DamageMax, 0, 100),
            Mathf.Clamp(data.CasualtiesMin, 0, 100),
            Mathf.Clamp(data.CasualtiesMax, 0, 100));
    }

    public void SetPredictedDataTexts(int predictedDamageMin, int predictedDamageMax, int predictedLossesMin, int predictedLossesMax)
    {
        var locMan = LocalizationManager.Instance;
        if (predictedDamageMin == predictedDamageMax)
        {
            predictedDamageText.text = locMan.GetText("OurPredictedDamage") + predictedDamageMin.ToString("00") + "%";
        }
        else
        {
            string minString = predictedDamageMin.ToString((predictedDamageMin / 10) > 0 ? "00" : "");
            string maxString = predictedDamageMax.ToString((predictedDamageMax / 10) > 0 ? "00" : "");
            predictedDamageText.text = locMan.GetText("OurPredictedDamage") + minString + "% - " + maxString + "%";
        }

        predictedLossesText.text = locMan.GetText("OurPredictedLossesUnknown");

        if (predictedLossesMin == predictedLossesMax)
        {
            predictedLossesText.text = locMan.GetText("OurPredictedLosses") + predictedLossesMin.ToString("00") + "%";
        }
        else
        {
            string minString = predictedLossesMin.ToString((predictedLossesMin / 10) > 0 ? "00" : "");
            string maxString = predictedLossesMax.ToString((predictedLossesMax / 10) > 0 ? "00" : "");
            predictedLossesText.text = locMan.GetText("OurPredictedLosses") + minString + "% - " + maxString + "%";
        }
    }
}
