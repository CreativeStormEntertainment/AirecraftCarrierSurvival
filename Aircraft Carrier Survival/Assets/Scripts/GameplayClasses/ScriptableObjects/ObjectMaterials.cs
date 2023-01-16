using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ACS/Materials")]
public class ObjectMaterials : ScriptableObject
{
	[FormerlySerializedAs("planeType")]
	public EPlaneType PlaneType;

	[FormerlySerializedAs("materialsA")]
	public List<Material> MaterialsA;
	[FormerlySerializedAs("materialsB")]
	public List<Material> MaterialsB;
	[FormerlySerializedAs("materialsC")]
	public List<Material> MaterialsC;

	public List<MaterialsList> LevelMaterialsList;
}