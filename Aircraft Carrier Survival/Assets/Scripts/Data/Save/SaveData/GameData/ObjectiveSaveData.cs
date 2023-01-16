using System;
using System.Collections.Generic;

[Serializable]
public struct ObjectiveSaveData
{
    public bool Visible;
    public bool Active;
    public bool Finished;
    public bool Success;
    public List<int> Progress;

    public bool TextChanged;
    public string Title;
    public string Description;
    public int ParamA;
    public int ParamB;

    public bool MidwayCAP;

    public ObjectiveSaveData Duplicate()
    {
        var result = this;
        result.Progress = new List<int>(Progress);
        if (Title != null)
        {
            result.Title = Title;
        }
        if (Description != null)
        {
            result.Description = Description;
        }
        return result;
    }
}
