using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TacticalUnitItem : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Button identifyButton = null;
    [SerializeField] private Image itemBack = null;
    [SerializeField] private Image unitIcon = null;
    [SerializeField] private Image arrowIcon = null;
    [SerializeField] private Text arrowText = null;
    [SerializeField] private Sprite normalBackground = null;
    [SerializeField] private Sprite selectedBackground = null;
    [SerializeField] private Sprite hiddenBackground;
    //[SerializeField] private Sprite revealedBackground = null;
    [SerializeField] private Sprite normalNavalUnit;
    [SerializeField] private Sprite hiddenNavalUnit = null;
    [SerializeField] private Sprite arrowNormal = null;
    [SerializeField] private Sprite arrowSelected = null;

    [SerializeField] private Text mastsText = null;
    [SerializeField] private Text funnelsText = null;
    [SerializeField] private Text armamentText = null;
    [SerializeField] private Text lengthText = null;

    [SerializeField]
    private GameObject destroyedImage = null;

    private EnemyUnit unit;
    private Transform parent;

    public void SetUnit(EnemyUnit _unit, TacticalEnemyMapButton fleet, Transform _parent, int idx)
    {
        unit = _unit;
        parent = _parent;

        destroyedImage.SetActive(unit.IsDead);

        if (unit.isHidden && unit.enemyType != EEnemyTypeDemo.Unsure)
        {
            mastsText.text = "?";
            armamentText.text = "?";
            funnelsText.text = "?";
            lengthText.text = "?";
            unitIcon.sprite = unit.guessedEnemyType == EEnemyTypeDemo.Unsure ? hiddenNavalUnit : TacticManager.Instance.availableUnits[unit.GuessedId].Sprite;
        }
        else if (unit.enemyType == EEnemyTypeDemo.Unsure)
        {
            if (fleet.DetectedMast)
            {
                mastsText.text = unit.MastsLength.ToString();
                lengthText.text = "?";
            }
            else
            {
                mastsText.text = "?";
                lengthText.text = unit.UnitLength.ToString();
            }
            armamentText.text = "?";
            funnelsText.text = unit.FunnelCount.ToString();
            unitIcon.sprite = unit.guessedEnemyType == EEnemyTypeDemo.Unsure ? hiddenNavalUnit : TacticManager.Instance.availableUnits[unit.GuessedId].Sprite;
            /*if (unit.enemyType == EEnemyTypeDemo.Unsure)
            {
                unitIcon.sprite = unit.guessedEnemyType == EEnemyTypeDemo.Unsure ? hiddenNavalUnit : TacticManager.Instance.availableUnits[unit.GuessedId].Sprite;
            }
            else
            {
                unitIcon.sprite = TacticManager.Instance.availableUnits[unit.Index].Sprite;
            }*/
            //unitIcon.sprite = unit.enemyType == EEnemyTypeDemo.Unsure ? (unit.guessedEnemyType == EEnemyTypeDemo.Unsure ? hiddenNavalUnit : TacticManager.Instance.availableUnits[unit.GuessedId].Sprite) : TacticManager.Instance.availableUnits[unit.Index].Sprite;
        }
        else
        {
            mastsText.text = unit.MastsLength.ToString();
            lengthText.text = unit.UnitLength.ToString();
            armamentText.text = unit.ArmamentCount.ToString();
            funnelsText.text = unit.FunnelCount.ToString();
            unitIcon.sprite = TacticManager.Instance.availableUnits[unit.ShipId].Sprite;
        }
        identifyButton.gameObject.SetActive(unit.isHidden);

        arrowText.text = idx.ToString();

        identifyButton.onClick.AddListener(OpenPreviewPanel);
    }

    private void OpenPreviewPanel()
    {
        if (unit.enemyType == EEnemyTypeDemo.Unsure || unit.isHidden)
        {
            var tacMan = TacticManager.Instance;
            int index = tacMan.availableUnits.FindIndex((x) => x.GuessedId == unit.GuessedId);
            if (index == -1)
            {
                index = 0;
            }
        }
    }

    private void HighlightUnit()
    {
        List<Transform> childrenList = new List<Transform>();
        foreach (Transform child in parent)
        {
            childrenList.Add(child);
        }
        childrenList.ForEach(child => child.GetComponent<TacticalUnitItem>().itemBack.sprite = child.GetComponent<TacticalUnitItem>().normalBackground);
        childrenList.ForEach(child => child.GetComponent<TacticalUnitItem>().arrowIcon.sprite = child.GetComponent<TacticalUnitItem>().arrowNormal);
        itemBack.sprite = selectedBackground;
        arrowIcon.sprite = arrowSelected;
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        HighlightUnit();
    }
}
