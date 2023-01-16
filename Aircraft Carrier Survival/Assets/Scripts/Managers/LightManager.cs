using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class LightManager : MonoBehaviour
{
    public static LightManager Instance;

    [SerializeField]
    private List<SubSectionRoom> SubRoomsList = new List<SubSectionRoom>();
    [SerializeField]
    private List<LightData> LightDataList = new List<LightData>();
    private Dictionary<SubSectionRoom, LightData> LightDataDict = new Dictionary<SubSectionRoom, LightData>();

    public List<Texture2D> DirTexs;
    public List<Texture2D> LightTexs;
    public List<Texture2D> ShadowTexs;

#if UNITY_EDITOR
    static readonly List<string> SceneStates = new List<string>() { "Normal", "Destroyed" };
    const string LightmapPrefix = @"\Lightmap-";
    const string LightMapSuffix = "_comp_";

    //[MenuItem("Tools/Lighting/More/Save data", false, 201)]
    //static void SaveData()
    //{
    //    var room = FindObjectOfType<SectionRoom>();
    //    var subrooms = room.GetComponentsInChildren<SubSectionRoom>(true);

    //    var datas = new LightData[2] { new LightData(), new LightData() };
    //    var colliders = new MeshCollider[2];

    //    for (int i = 0; i < datas.Length; i++)
    //    {
    //        datas[i].LightmapIndices.Add(subrooms[i].GetComponent<MeshRenderer>().lightmapIndex);
    //        datas[i].LightmapScaleOffsets.Add(subrooms[i].GetComponent<MeshRenderer>().lightmapScaleOffset);

    //        colliders[i] = subrooms[i].gameObject.AddComponent<MeshCollider>();
    //    }

    //    var positions = LightmapSettings.lightProbes.positions;
    //    for (int i = 0; i < positions.Length; i++)
    //    {
    //        var start = positions[i];
    //        start.x += 50f;
    //        var dir = positions[i] - start;
    //        float dist = dir.magnitude;
    //        dir /= dist;
    //        var hits = Physics.RaycastAll(start, dir, dist);

    //        foreach (var hit in hits)
    //        {
    //            for (int j = 0; j < datas.Length; j++)
    //            {
    //                if (hit.collider == colliders[j])
    //                {
    //                    datas[j].LightProbes.Add(i);
    //                    datas[j].ProbesData.Add(LightmapSettings.lightProbes.bakedProbes[i]);
    //                }
    //            }
    //        }
    //    }
    //    foreach(var collider in colliders)
    //    {
    //        DestroyImmediate(collider);
    //    }

    //    var path = SceneManager.GetActiveScene().path;
    //    path = path.Substring(0, path.LastIndexOf("/") + 1);
    //    path += room.name;
    //    path += ".txt";
    //    using (var writer = new StreamWriter(File.Open(path, File.Exists(path) ? FileMode.Truncate : FileMode.Create)))
    //    {
    //        writer.Write(room.name);

    //        foreach(var data in datas)
    //        {
    //            writer.Write(";");
    //            Assert.IsTrue(data.LightmapIndices.Count == 1);
    //            writer.Write(data.LightmapIndices[0]);
    //            writer.Write(";");
    //            Assert.IsTrue(data.LightmapScaleOffsets.Count == 1);
    //            writer.Write(data.LightmapScaleOffsets[0].x);
    //            writer.Write(";");
    //            writer.Write(data.LightmapScaleOffsets[0].y);
    //            writer.Write(";");
    //            writer.Write(data.LightmapScaleOffsets[0].z);
    //            writer.Write(";");
    //            writer.Write(data.LightmapScaleOffsets[0].w);
    //        }

    //        Assert.IsTrue(datas.Length == 2);
    //        for(int i=0;i<datas.Length;i++)
    //        {
    //            Assert.IsTrue(datas[i].LightProbes.Count == datas[i].ProbesData.Count);
    //            for (int j = 0; j < datas[i].LightProbes.Count; j++)
    //            {
    //                writer.Write(";");
    //                writer.Write(LightmapSettings.lightProbes.positions[datas[i].LightProbes[j]].sqrMagnitude);
    //                for (int m = 0; m < 3; m++)
    //                {
    //                    for (int n = 0; n < 9; n++)
    //                    {
    //                        writer.Write(";");
    //                        writer.Write(datas[i].ProbesData[j][m, n]);
    //                    }
    //                }
    //            }
    //            if (i == 0)
    //            {
    //                writer.WriteLine("");
    //            }
    //        }
    //    }
    //}

    //[MenuItem("Tools/Lighting/Generate light and save", false, 301)]
    //static void GenerateLightAndSave()
    //{
    //    Bake();
    //}

    //[MenuItem("Tools/Lighting/Update data", false, 302)]
    //static void UpdateData()
    //{
    //    var lightMan = GameObject.Find("Managers").GetComponent<LightManager>();
    //    Undo.RecordObject(lightMan, "Updated lightmaps");

    //    lightMan.DirTexs = new List<Texture2D>();
    //    lightMan.LightTexs = new List<Texture2D>();
    //    lightMan.ShadowTexs = new List<Texture2D>();

    //    var dict = new Dictionary<SubSectionRoom, LightData>();

    //    var lightmapIndicesInUse = new HashSet<int>();
    //    for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
    //    {
    //        lightmapIndicesInUse.Add(i);
    //    }
    //    var rooms = FindObjectsOfType<SectionRoom>();
    //    foreach (var room in rooms)
    //    {
    //        var subsections = room.GetComponentsInChildren<SubSectionRoom>();
    //        Assert.IsTrue(subsections.Length == 2);
    //        lightmapIndicesInUse.Remove(subsections[0].GetComponent<MeshRenderer>().lightmapIndex);
    //        lightmapIndicesInUse.Remove(subsections[1].GetComponent<MeshRenderer>().lightmapIndex);
    //    }

    //    SectionRoom getSectionRoom(string key)
    //    {
    //        foreach (var room in rooms)
    //        {
    //            if (room.name == key)
    //            {
    //                return room;
    //            }
    //        }
    //        Assert.IsTrue(false);
    //        return null;
    //    }

    //    var folders = Directory.GetDirectories(@"Assets\GameplayAssets\Scenes\LightGeneration");
    //    Assert.IsTrue(folders.Length == SceneStates.Count);
    //    foreach(var folder in folders)
    //    {
    //        int index = SceneStates.IndexOf(folder.Substring(folder.LastIndexOf(@"\") + 1));
    //        Assert.IsFalse(index == -1);
    //        var files = Directory.GetFiles(folder, "*.txt");
    //        foreach (var file in files)
    //        {
    //            string[] data;
    //            using (var enumerator = File.ReadLines(file).GetEnumerator())
    //            {
    //                bool next = enumerator.MoveNext();
    //                Assert.IsTrue(next);

    //                data = enumerator.Current.Split(';');
    //            }
    //            LightData lightData01, lightData02;
    //            int i = 0;
    //            //bool empty = false;

    //            var room = getSectionRoom(data[i++]);
    //            var subRooms = room.GetComponentsInChildren<SubSectionRoom>(true);
    //            Assert.IsTrue(subRooms.Length == 2);
    //            bool found01 = dict.TryGetValue(subRooms[0], out lightData01);
    //            bool found02 = dict.TryGetValue(subRooms[1], out lightData02);
    //            if (!found01)
    //            {
    //                Assert.IsFalse(found02);
    //                lightData01 = new LightData();
    //                lightData02 = new LightData();
    //                dict[subRooms[0]] = lightData01;
    //                dict[subRooms[1]] = lightData02;
    //                for (int j = 0; j < SceneStates.Count; j++)
    //                {
    //                    lightData01.LightmapIndices.Add(-1);
    //                    lightData01.LightmapScaleOffsets.Add(Vector4.zero);
    //                    lightData02.LightmapIndices.Add(-1);
    //                    lightData02.LightmapScaleOffsets.Add(Vector4.zero);
    //                }
    //                //empty = true;
    //            }

    //            lightData01.LightmapIndices[index] = int.Parse(data[i++]);
    //            lightData01.LightmapScaleOffsets[index] = new Vector4(float.Parse(data[i++]), float.Parse(data[i++]), float.Parse(data[i++]), float.Parse(data[i++]));

    //            lightData02.LightmapIndices[index] = int.Parse(data[i++]);
    //            lightData02.LightmapScaleOffsets[index] = new Vector4(float.Parse(data[i++]), float.Parse(data[i++]), float.Parse(data[i++]), float.Parse(data[i++]));

    //         //   AddProbes(index, i, data, lightData01.LightProbes, lightData01.ProbesData, empty);
    //            //if (enumerator.MoveNext())
    //           // {
    //        //        data = enumerator.Current.Split(';');
    //        //        AddProbes(index, 1, data, lightData02.LightProbes, lightData02.ProbesData, empty);
    //       //         next = enumerator.MoveNext();
    //       //         Assert.IsFalse(next);
    //       //     }

    //            var textureParentPath = file.Substring(0, file.LastIndexOf('.'));
    //            textureParentPath += LightmapPrefix;

    //            int oldIndexValue = lightData01.LightmapIndices[index];
    //            GetAndUpdateLightmap(lightData01.LightmapIndices, -1, index, -1, textureParentPath, lightmapIndicesInUse, lightMan.DirTexs, lightMan.LightTexs, lightMan.ShadowTexs);
    //            GetAndUpdateLightmap(lightData02.LightmapIndices, oldIndexValue, index, lightData01.LightmapIndices[index], textureParentPath, lightmapIndicesInUse, lightMan.DirTexs, lightMan.LightTexs, lightMan.ShadowTexs);
    //        }
    //    }

    //    lightMan.SubRoomsList.Clear();
    //    lightMan.LightDataList.Clear();
    //    var dictCoeff = new Dictionary<int, SphericalHarmonicsL2>();
    //    foreach (var pair in dict)
    //    {
    //        var dictCoeff2 = new Dictionary<int, SphericalHarmonicsL2>();
    //        lightMan.SubRoomsList.Add(pair.Key);
    //        lightMan.LightDataList.Add(pair.Value);

    //        for (int i = 0; i < pair.Value.LightProbes.Count; i++)
    //        {
    //            Assert.IsFalse(dictCoeff.ContainsKey(pair.Value.LightProbes[i]));
    //            dictCoeff[pair.Value.LightProbes[i]] = pair.Value.ProbesData[i];

    //            Assert.IsFalse(dictCoeff2.ContainsKey(pair.Value.LightProbes[i]));
    //            dictCoeff2[pair.Value.LightProbes[i]] = pair.Value.ProbesData[i];
    //        }
    //    }

    //    lightMan.SetLightmaps();
    //    EditorUtility.SetDirty(lightMan);
    //    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    //}

    //static void Bake()
    //{
    //    LightmapEditorSettings.textureCompression = false;
    //    LightmapEditorSettings.mixedBakeMode = MixedLightingMode.Shadowmask;

    //    Lightmapping.bakeCompleted -= OnBakeCompleted;
    //    Lightmapping.bakeCompleted += OnBakeCompleted;
    //    Lightmapping.BakeAsync();
    //}

    //static void OnBakeCompleted()
    //{
    //    Lightmapping.bakeCompleted -= OnBakeCompleted;

    //    var parentPath = SceneManager.GetActiveScene().path;
    //    parentPath = parentPath.Substring(0, parentPath.LastIndexOf('.'));
    //    var files = Directory.GetFiles(parentPath, "*dir.png");
    //    foreach (var filePath in files)
    //    {
    //        var importer = TextureImporter.GetAtPath(filePath) as TextureImporter;
    //        importer.textureCompression = TextureImporterCompression.CompressedHQ;
    //        AssetDatabase.ImportAsset(filePath);
    //    }
    //    files = Directory.GetFiles(parentPath, "*.exr");
    //    for (int i = 1; i < files.Length; i++)
    //    {
    //        Assert.IsTrue(files[i - 1].Contains("light"));

    //        var importer = TextureImporter.GetAtPath(files[i - 1]) as TextureImporter;
    //        importer.textureCompression = TextureImporterCompression.CompressedHQ;
    //        AssetDatabase.ImportAsset(files[i - 1]);
    //    }
    //    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    //    SaveData();
    //    Debug.Log("Finished");
    //}

    //static int FindIndex(Vector3[] arr, float mag)
    //{
    //    for (int i = 0; i < arr.Length; i++)
    //    {
    //        if (Mathf.Approximately(arr[i].sqrMagnitude, mag))
    //        {
    //            return i;
    //        }
    //    }

    //    Debug.Log(mag);
    //    foreach(var pos2 in arr)
    //    {
    //        Debug.Log(pos2.sqrMagnitude);
    //    }
    //    Assert.IsTrue(false);
    //    return -1;
    //}

    //static void AddProbes(int index, int i, string[] data, List<int> lightProbes, List<SphericalHarmonicsL2> probesData, bool empty)
    //{
    //    int j = 0;
    //    while (i < data.Length)
    //    {
    //        int newIndex = index;
    //        int probeIndex = FindIndex(LightmapSettings.lightProbes.positions, float.Parse(data[i++]));
    //        if (empty)
    //        {
    //            lightProbes.Add(probeIndex);

    //            newIndex = probesData.Count;
    //            probesData.Add(new SphericalHarmonicsL2());
    //            Assert.IsTrue(lightProbes.Count == probesData.Count);
    //        }
    //        else
    //        {
    //            if (probesData.Count == lightProbes.Count)
    //            {
    //                int count = (SceneStates.Count - 1) * lightProbes.Count;
    //                for (int k = 0; k < count; k++)
    //                {
    //                    probesData.Add(new SphericalHarmonicsL2());
    //                }
    //            }
    //            Assert.IsTrue((lightProbes.Count * SceneStates.Count) == probesData.Count);
    //            Assert.IsTrue(lightProbes[j] == probeIndex);
    //            newIndex = newIndex * lightProbes.Count + j++;
    //        }
    //        var coeff = new SphericalHarmonicsL2();
    //        for (int a = 0; a < 3; a++)
    //        {
    //            for (int b = 0; b < 9; b++)
    //            {
    //                coeff[a, b] = float.Parse(data[i++]);
    //            }
    //        }
    //        probesData[newIndex] = coeff;
    //    }
    //}

    //static void GetAndUpdateLightmap(List<int> indices, int indexValue2, int atIndex, int newLightIndex, string lightmapParentPath, HashSet<int> lightmapIndicesInUse, List<Texture2D> dirTexs, List<Texture2D> lightTexs, List<Texture2D> shadowTexs)
    //{
    //    Assert.IsFalse(indices[atIndex] == -1);
    //    if (indices[atIndex] != indexValue2)
    //    {
    //        bool added = false;
    //        foreach (var index in lightmapIndicesInUse)
    //        {
    //            Assert.IsFalse(added && index < dirTexs.Count);
    //            if (dirTexs.Count == index)
    //            {
    //                added = true;
    //                var lightmap = LightmapSettings.lightmaps[index];
    //                dirTexs.Add(lightmap.lightmapDir);
    //                lightTexs.Add(lightmap.lightmapColor);
    //                shadowTexs.Add(lightmap.shadowMask);
    //            }
    //        }

    //        newLightIndex = dirTexs.Count;

    //        lightmapParentPath += indices[atIndex];
    //        lightmapParentPath += LightMapSuffix;

    //        dirTexs.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(lightmapParentPath + "dir.png"));
    //        Assert.IsNotNull(dirTexs[dirTexs.Count - 1]);
    //        lightTexs.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(lightmapParentPath + "light.exr"));
    //        Assert.IsNotNull(lightTexs[lightTexs.Count - 1]);
    //        shadowTexs.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(lightmapParentPath + "shadowmask.png"));
    //        Assert.IsNotNull(shadowTexs[shadowTexs.Count - 1]);
    //    }
    //    Assert.IsFalse(newLightIndex == -1);
    //    indices[atIndex] = newLightIndex;
    //}
#endif

    void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

//        SetLightmaps();
//        var enumVals = System.Enum.GetValues(typeof(ERoomLightStates));
//#if UNITY_EDITOR
//        Assert.IsTrue(SceneStates.Count == enumVals.Length);
//#endif
//        for (int i = 0; i < SubRoomsList.Count; i++)
//        {
//            var lightData = LightDataList[i];
//            for (int j = 0; j < lightData.LightProbes.Count; j++)
//            {
//                var list = new List<SphericalHarmonicsL2>();
//                for (int k = 0; k < enumVals.Length; k++)
//                {
//                    list.Add(lightData.ProbesData[k]);
//                }

//                lightData.ProbesDict[lightData.LightProbes[j]] = list;
//            }
//            LightDataDict[SubRoomsList[i]] = lightData;
//        }
    }

    public void SetLightmaps()
    {
        //if (DirTexs.Count != 0)
        //{
        //    var lightmapDatas = new LightmapData[DirTexs.Count];
        //    for (int i = 0; i < lightmapDatas.Length; i++)
        //    {
        //        lightmapDatas[i] = new LightmapData() { lightmapColor = LightTexs[i], lightmapDir = DirTexs[i], shadowMask = ShadowTexs[i] };
        //    }
        //    LightmapSettings.lightmaps = lightmapDatas;
        //}
    }

    public void SetLighting(SubSectionRoom room, ERoomLightStates state)
    {
        //var data = LightDataDict[room];
        //room.Renderer.lightmapIndex = data.LightmapIndices[(int)state];
        //room.Renderer.lightmapScaleOffset = data.LightmapScaleOffsets[(int)state];


        //foreach (var pair in data.ProbesDict)
        //{
        //    LightmapSettings.lightProbes.bakedProbes[pair.Key] = pair.Value[(int)state];
        //}
    }
}
