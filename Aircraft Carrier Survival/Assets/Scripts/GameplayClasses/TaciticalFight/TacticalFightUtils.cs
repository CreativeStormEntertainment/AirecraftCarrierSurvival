using UnityEngine;
using System.Collections;
using System;

public static class TacticalFightUtils
{
    public static ETacticalFightUnitRotationState GetRotationStateWithOffset(ETacticalFightUnitRotationState originRotation, int offset)
    {
        int rotationIndex = (int)originRotation;
        rotationIndex += offset;

        if (rotationIndex < 0)
            rotationIndex += Enum.GetNames(typeof(ETacticalFightUnitRotationState)).Length;
        else if (rotationIndex > 5)
            rotationIndex -= Enum.GetNames(typeof(ETacticalFightUnitRotationState)).Length;

        return (ETacticalFightUnitRotationState)rotationIndex;

    }
}
