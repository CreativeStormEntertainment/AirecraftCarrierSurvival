using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DifficultyLvl : MonoBehaviour
{
    private int difficultyLvl = 0;
    private Image image = null;
    private Text text = null;

    [SerializeField]
    private Sprite easyLvl = null;
    [SerializeField]
    private Sprite mediumLvl = null;
    [SerializeField]
    private Sprite hardLvl = null;

    internal void SetDifficulty(EMissionDifficulty missionDifficulty)
    {
        if (image == null)
        {
            image = GetComponent<Image>();
            text = transform.GetChild(0).GetComponent<Text>();
        }

        difficultyLvl = (int)missionDifficulty;

        switch (missionDifficulty)
        {
            case EMissionDifficulty.Easy:
                image.sprite = easyLvl;
                text.text = LocalizationManager.Instance.GetText("Easy");
                break;

            case EMissionDifficulty.Medium:
                image.sprite = mediumLvl;
                text.text = LocalizationManager.Instance.GetText("Medium");
                break;

            case EMissionDifficulty.Hard:
                image.sprite = hardLvl;
                text.text = LocalizationManager.Instance.GetText("Hard");
                break;
        }
    }
}
