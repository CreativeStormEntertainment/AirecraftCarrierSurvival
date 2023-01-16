using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlaneManager : MonoBehaviour
{
    public static PlaneManager Instance;

    public Material TransparentMaterial = null;

    [SerializeField]
    private List<ObjectMaterials> planeMaterials = null;

    private Dictionary<EPlaneType, ObjectMaterials> dict;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        dict = new Dictionary<EPlaneType, ObjectMaterials>();
        foreach (var data in planeMaterials)
        {
            dict[data.PlaneType] = data;
        }
    }


	public Material GetPlaneMaterial(EPlaneType planeType)
	{
        var data = SaveManager.Instance.Data.IntermissionData;

        return dict[planeType].LevelMaterialsList[data.GetUpgrade(planeType)].Materials[data.GetSkin(planeType)];
	}
}
