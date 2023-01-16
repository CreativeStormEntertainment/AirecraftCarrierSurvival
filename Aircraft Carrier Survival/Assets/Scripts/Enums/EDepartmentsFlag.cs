using System;

[Flags]
public enum EDepartmentsFlag
{
    Deck        = 1 << EDepartments.Deck,
    Air         = 1 << EDepartments.Air,
    Medical     = 1 << EDepartments.Medical,
    Engineering = 1 << EDepartments.Engineering,
    Navigation  = 1 << EDepartments.Navigation,
    AA          = 1 << EDepartments.AA,
    Idle        = 1 << EDepartments.Count,
    Healing     = 1 << (EDepartments.Count + 1)
}
