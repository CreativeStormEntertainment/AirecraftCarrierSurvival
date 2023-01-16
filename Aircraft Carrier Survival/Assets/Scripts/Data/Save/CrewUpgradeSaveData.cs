using System;
using System.Collections.Generic;

[Serializable]
public struct CrewUpgradeSaveData
{
    public int Medals;
    public int Specialties;
    public int CrewDataIndex;
    public int Cost;

    public IEnumerable<ECrewmanSpecialty> GetSpecialties()
    {
        for (int i = 0; i < (int)ECrewmanSpecialty.Count + 1; i++)
        {
            if ((Specialties & (1 << i)) != 0)
            {
                var specialty = (ECrewmanSpecialty)i;
                yield return specialty == ECrewmanSpecialty.Count ? ECrewmanSpecialty.GeneralBoost : specialty;
            }
        }
    }

    public void SetSpecialties(IEnumerable<ECrewmanSpecialty> specialties)
    {
        Specialties = 0;
        bool haveGeneralBoost = false;
        foreach (var specialty in specialties)
        {
            int specialtyIndex = (int)specialty;
            if (specialty == ECrewmanSpecialty.GeneralBoost)
            {
                if (haveGeneralBoost)
                {
                    specialtyIndex++;
                }
                haveGeneralBoost = true;
            }
            Specialties |= (1 << specialtyIndex);
        }
    }
}
