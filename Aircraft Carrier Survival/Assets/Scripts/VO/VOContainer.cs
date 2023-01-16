using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VOContainer
{
    public string Name;
    public List<VO> Data;
    public VOContainer(string Name, List<VO> Data)
    {
        this.Name = Name;
        this.Data = Data;
    }
}
