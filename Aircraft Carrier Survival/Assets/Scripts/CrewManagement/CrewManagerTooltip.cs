using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CrewManagerTooltip : MonoBehaviour
{
    [SerializeField] private Text efficiencyText = null;
    [SerializeField] private Text rankText = null;
    [SerializeField] private Text stateText = null;
    [SerializeField] private Text specialtyText = null;

    private Camera cam => Camera.main;

    public void SetTooltip(ECrewUnitRank rank, ECrewUnitState state, ECrewUnitSpecialization specialization, Vector2 pos)
    {
        string _efficiency = string.Format("Efficiency: {0}%", (int)rank + (int)specialization);
        string _rank = string.Format("Rank: {0}", rank.ToString());
        string _state = string.Format("State: {0}", state.ToString());
        string _specialization = string.Format("Specialization: {0}", specialization.ToString());

        efficiencyText.text = _efficiency;
        rankText.text = _rank;
        stateText.text = _state;
        specialtyText.text = _specialization;

        transform.position = pos;
    }
}
