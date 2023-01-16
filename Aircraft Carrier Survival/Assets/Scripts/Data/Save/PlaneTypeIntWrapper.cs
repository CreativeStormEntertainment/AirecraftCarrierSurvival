using System;

[Serializable]
public class PlaneTypeIntWrapper
{
	public EPlaneType Type;
	public int Value;

	public PlaneTypeIntWrapper(EPlaneType t, int i)
	{
		Type = t;
		Value = i;
	}

	private PlaneTypeIntWrapper()
    {

    }

	public PlaneTypeIntWrapper Duplicate()
    {
		var result = new PlaneTypeIntWrapper();

		result.Type = Type;
		result.Value = Value;

		return result;
    }
}