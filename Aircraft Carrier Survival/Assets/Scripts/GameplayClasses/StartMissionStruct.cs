using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System;

[Serializable]
public struct StartMissionStruct
{
    public EMissionOrderType Mission;
    public int Count;
}