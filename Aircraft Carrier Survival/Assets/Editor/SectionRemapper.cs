using UnityEditor;

public class SectionRemapper : AssetPostprocessor
{
    public void OnPreprocessModel()
    {
        //var modelImp = assetImporter as ModelImporter;
        //foreach (var path in MeshMerger.SectionsFolders[0])
        //{
        //    string parentPath = MeshMerger.ParentPath + path;
        //    if (modelImp.assetPath.StartsWith(parentPath))
        //    {
        //        modelImp.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Everywhere);
        //        break;
        //    }
        //}
    }
}
