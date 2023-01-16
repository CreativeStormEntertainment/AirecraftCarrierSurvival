using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewManSpecialityTooltip : TooltipCaller
{
    public ECrewmanSpecialty Speciality
    {
        get;
        set;
    }

    private CrewManager crewMan;

    private void Start()
    {
        crewMan = CrewManager.Instance;
    }

    protected override void UpdateText()
    {
        if (locMan == null)
        {
            locMan = LocalizationManager.Instance;
        }

        title = crewMan.CrewSpecialityTitle;
        switch (Speciality)
        {
            case ECrewmanSpecialty.Air:
                description = locMan.GetText(crewMan.AirDesc, crewMan.SpecialityBonus.ToString());
                break;
            case ECrewmanSpecialty.AntiAircraft:
                description = locMan.GetText(crewMan.AntiAirDesc, crewMan.SpecialityBonus.ToString());
                break;
            case ECrewmanSpecialty.Deck:
                description = locMan.GetText(crewMan.DeckDesc, crewMan.SpecialityBonus.ToString());
                break;
            case ECrewmanSpecialty.DepartmentFull:
                description = locMan.GetText(crewMan.TeamPlayerDesc, crewMan.FullDepartBonus.ToString());
                break;
            case ECrewmanSpecialty.DepartmentSolo:
                description = locMan.GetText(crewMan.LoneWolfDesc, crewMan.AloneDepartBonus.ToString());
                break;
            case ECrewmanSpecialty.Engineering:
                description = locMan.GetText(crewMan.EngineeringDesc, crewMan.SpecialityBonus.ToString());
                break;
            case ECrewmanSpecialty.Medical:
                description = locMan.GetText(crewMan.MedicalDesc, crewMan.SpecialityBonus.ToString());
                break;
            case ECrewmanSpecialty.Navigation:
                description = locMan.GetText(crewMan.NavigationsDesc, crewMan.SpecialityBonus.ToString());
                break;
            case ECrewmanSpecialty.GeneralBoost:
                description = locMan.GetText(crewMan.GeneralDesc, crewMan.GeneralistBonus.ToString());
                break;
            case ECrewmanSpecialty.None:
                break;
            default:
                break;
        }
    }
}
