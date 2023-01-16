using System;

[Flags]
public enum EDcCategoryFlag
{
    Fire        = 1 << EDcCategory.Fire,
    Water       = 1 << EDcCategory.Water,
    Injured     = 1 << EDcCategory.Injured,
    Mechanic    = 1 << EDcCategory.Crash,
}
