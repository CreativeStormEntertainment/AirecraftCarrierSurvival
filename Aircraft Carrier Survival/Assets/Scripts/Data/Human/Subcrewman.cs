using System;
using System.Collections.Generic;

public class Subcrewman
{
    public Person Parent;
    public EDCType DCType;
    public SubSectionRoom Subsection;
    public SubcrewmanIcon icon;
    [NonSerialized]
    public HashSet<InstanceGroup> Groups;

    public Subcrewman(Person parent, SubcrewmanIcon icon)
    {
        Groups = new HashSet<InstanceGroup>();
        Parent = parent;
        this.icon = icon;
        icon.Subcrewman = this;
    }

    public void UpdatePortrait()
    {
        UpdatePortrait(EDCType.None, null);
    }

    public void UpdatePortrait(EDCType dcType, SubSectionRoom subsection)
    {
        DCType = dcType;
        Subsection = subsection;

        var iconMan = IconManager.Instance;
        if (subsection != null)
        {
            icon.SectionIcon.gameObject.SetActive(true);
            icon.SectionCategoryIcon.sprite = iconMan.SectionCategoryIconsDict[Subsection.ParentSection.Category];
            icon.SectionIcon.sprite = dcType == EDCType.None ? Subsection.ParentSection.Icon : iconMan.DCJobIcon;
        }
        else
        {
            icon.SectionCategoryIcon.sprite = iconMan.NoSectionCategoryIcon;
            icon.SectionIcon.gameObject.SetActive(false);
        }
    }
}