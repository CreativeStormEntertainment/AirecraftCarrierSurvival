using System.Collections.Generic;
using UnityEngine.Assertions;

public abstract class Person
{
    public List<Subcrewman> Subcrewmen;
    public float Effectiveness;
    //public readonly WorkerPortrait Portrait;
    public readonly PersonData Data;

    private bool selected;
    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            //Portrait.SetSelect(value);
            foreach (var subcrewman in Subcrewmen)
            {
                foreach (var group in subcrewman.Groups)
                {
                    group.SetSelected(value);
                }
            }
        }
    }

    private int maxCrewmen;
    public int MaxCrewmen
    {
        get => maxCrewmen;
        set
        {
            Assert.IsFalse(value > Subcrewmen.Count, Subcrewmen.Count.ToString());
            maxCrewmen = value;
            /*for (int i = 0; i < MaxCrewmen; i++)
            {
                Portrait.SubcrewmenIcons[i].gameObject.SetActive(true);
            }
            for (int i = MaxCrewmen; i < Subcrewmen.Count; i++)
            {
                Portrait.SubcrewmenIcons[i].gameObject.SetActive(false);
            }

            Portrait.UpdatePortait(HealthStatus);*/
        }
    }

    protected virtual EHealthStatus HealthStatus
    {
        get => Data.Health;
        set => Data.Health = value;
    }

    public Person(PersonData data)
    {
        this.Data = data;

        //Portrait = portrait;
        //Portrait.Person = this;
    }

    public Subcrewman GetFreeSubcrewman(SectionRoom section)
    {
        for (int i = 0; i < MaxCrewmen; i++)
        {
            if (Subcrewmen[i].Subsection == null)
            {
                return Subcrewmen[i];
            }
            if (Subcrewmen[i].DCType == EDCType.None && Subcrewmen[i].Subsection.ParentSection == section)
            {
                return Subcrewmen[i];
            }
        }
        return null;
    }

    public void ThrowDownSubcrewman(Subcrewman subcrewman)
    {
        int i;
        for (i = 0; i < Subcrewmen.Count && Subcrewmen[i] != subcrewman; i++) ;
        Assert.IsFalse(i == Subcrewmen.Count);
        for (int j = i + 1; j < Subcrewmen.Count; j++)
        {
            Utils.Swap(ref Subcrewmen[j].icon, ref Subcrewmen[j - 1].icon);
            Utils.Swap(ref Subcrewmen[j].icon.Subcrewman, ref Subcrewmen[j - 1].icon.Subcrewman);
            Subcrewmen[j].UpdatePortrait(Subcrewmen[j].DCType, Subcrewmen[j].Subsection);
        }
        Subcrewmen.RemoveAt(i);
        Subcrewmen.Add(subcrewman);
        subcrewman.UpdatePortrait();
    }
}
