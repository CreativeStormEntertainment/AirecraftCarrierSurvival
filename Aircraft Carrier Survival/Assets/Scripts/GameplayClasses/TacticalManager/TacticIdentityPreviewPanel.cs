using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticIdentityPreviewPanel : MonoBehaviour
{
    //[SerializeField] private Image unitImage = null;
    [SerializeField] private Text nameText = null;
    [SerializeField] private Text mastsText = null;
    [SerializeField] private Text funnelsText = null;
    [SerializeField] private Text armamentText = null;
    [SerializeField] private Text lengthText = null;
    [SerializeField] private Button prevButton = null;
    [SerializeField] private Button nextButton = null;
    [SerializeField] private Button chooseButton = null;
    [SerializeField] private Text countText = null;
    [SerializeField] private Image image = null;

    private int currentIndex = 0;
    private EnemyUnit selectedUnit = null;

    public EnemyUnit currentUnit { get; set; }

    public void Start()
    {
        prevButton.onClick.AddListener(PreviousUnit);
        nextButton.onClick.AddListener(NextUnit);
        chooseButton.onClick.AddListener(ChooseUnit);
        gameObject.SetActive(false);
    }

    public void Set(EnemyUnit unit, int index)
    {
        currentUnit = unit;
        selectedUnit = TacticManager.Instance.availableUnits[index];
        currentIndex = index;
        UpdateText();
    }

    private void PreviousUnit()
    {
        ChangeUnit(-1);
        UpdateText();
    }
    private void NextUnit()
    {
        ChangeUnit(1);
        UpdateText();
    }

    private void ChangeUnit(int delta)
    {
        int value = TacticManager.Instance.availableUnits.Count;
        currentIndex = (value + currentIndex + delta) % value;
    }

    private void UpdateText()
    {
        var units = TacticManager.Instance.availableUnits;
        mastsText.text = units[currentIndex].MastsMinLength + " - " + units[currentIndex].MastsMaxLength;
        funnelsText.text = units[currentIndex].FunnelCount.ToString();
        armamentText.text = units[currentIndex].ArmamentMinCount + " - " + units[currentIndex].ArmamentMaxCount;
        lengthText.text = units[currentIndex].UnitMinLength + " - " + units[currentIndex].UnitMaxLength;
        countText.text = (currentIndex + 1) + "/" + (units.Count);
        SetUnitOnPreview();
    }

    private void SetUnitOnPreview()
    {
        selectedUnit = TacticManager.Instance.availableUnits[currentIndex];
        nameText.text = LocalizationManager.Instance.GetText(selectedUnit.UnitName);
        image.sprite = selectedUnit.Sprite;
    }

    private void ChooseUnit()
    {
        var tacMan = TacticManager.Instance;
        currentUnit.guessedEnemyType = selectedUnit.enemyType;
        currentUnit.IsLightGuessed = selectedUnit.IsLight;
        currentUnit.IsHeavyGuessed = selectedUnit.IsHeavy;
        currentUnit.IsCarrierGuessed = selectedUnit.IsCarrier;
        currentUnit.GuessedId = currentIndex;
        gameObject.SetActive(false);
        tacMan.ShowIdentificationPanel(TacticManager.Instance.CurrentFleetBtn);
    }
}
