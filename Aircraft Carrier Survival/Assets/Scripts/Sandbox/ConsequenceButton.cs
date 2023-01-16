using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsequenceButton : MonoBehaviour
{
    public event Action<SandboxEventConsequence> OnClick = delegate { };

    [SerializeField]
    private Button button = null;
    [SerializeField]
    private Text buttonText = null;
    [SerializeField]
    private Text costText = null;

    private SandboxEventConsequence consequence;

    private void Start()
    {
        button.onClick.AddListener(Click);
    }

    public bool Setup(SandboxEventConsequence eventConsequence)
    {
        consequence = eventConsequence;
        bool canShow = CanShow(consequence);
        var locMan = LocalizationManager.Instance;
        buttonText.text = locMan.GetText("Incident_" + (consequence.EventIndex + 1).ToString("00") + "_Option_" + (consequence.ConsequenceIndex + 1).ToString("00"));
        costText.text = "";
        bool once = false;
        for (int i = 0; i < consequence.PossibleCosts.Count; i++)
        {
            string descIndex = "";
            if (consequence.PossibleCosts.Count > 1)
            {
                switch (i)
                {
                    case 0:
                        descIndex = "A";
                        break;
                    case 1:
                        descIndex = "B";
                        break;
                    case 2:
                        descIndex = "C";
                        break;
                }
            }
            if (once)
            {
                costText.text += "\n";
            }
            costText.text += locMan.GetText("Incident_" + (consequence.EventIndex + 1).ToString("00") + "_Costs_" + (consequence.ConsequenceIndex + 1).ToString("00") + descIndex);
            once = true;
        }
        gameObject.SetActive(canShow);
        return canShow;
    }

    private bool CanShow(SandboxEventConsequence eventConsequence)
    {
        bool can = true;
        foreach (var cost in eventConsequence.PossibleCosts)
        {
            switch (cost.ConsequenceType)
            {
                case ESandboxEventConsequence.CrewMemberInjured:
                case ESandboxEventConsequence.CrewMemberKilled:
                    if (CrewStatusManager.Instance.AliveCrews < 3 + cost.Value)
                    {
                        can = false;
                    }
                    break;
                case ESandboxEventConsequence.EscortShipDamaged:
                case ESandboxEventConsequence.EscortShipDestroyed:
                case ESandboxEventConsequence.EscortShipToPearlHarbour:
                    if (StrikeGroupManager.Instance.AliveMembers.Count == 0)
                    {
                        can = false;
                    }
                    break;
                case ESandboxEventConsequence.SquadronsLost:
                    if (AircraftCarrierDeckManager.Instance.GetAllSquadronsCount() < cost.Value)
                    {
                        can = false;
                    }
                    break;
                case ESandboxEventConsequence.SpecialEscortShipUsed:
                    if (!StrikeGroupManager.Instance.HaveEscortShipOfType(EStrikeGroupType.Destroyer))
                    {
                        can = false;
                    }
                    break;
                case ESandboxEventConsequence.CommandPointsLost:
                    if (SaveManager.Instance.Data.IntermissionData.CommandPoints < cost.Value)
                    {
                        can = false;
                    }
                    break;
            }
        }
        return can;
    }

    private void Click()
    {
        OnClick(consequence);
    }
}
