using System;
using System.Collections.Generic;

[Serializable]
public class ListInt
{
    public List<int> List;

    public ListInt()
    {
        List = new List<int>();
    }

    public ListInt(List<int> data)
    {
        List = new List<int>(data);
    }
}
