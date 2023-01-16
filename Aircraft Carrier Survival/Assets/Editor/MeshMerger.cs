using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class MeshMerger : EditorWindow
{
    public const string ParentPath = "Assets/ArtAssets/Mesh/";
    private const string ErrorMatPath = @"Assets\ArtAssets\Materials\ErrorMat.mat";
    private const string ErrorTexPath = @"Assets\ArtAssets\Texture\BaseTextures\Objects\Simple\meshmetalplates_m.png";
    private const string TextureArrayStartPath = @"Assets\ArtAssets\Texture\Generated\";
    private const string ErrorTextPath = @"Assets\ArtAssets\Mesh\meshErrors.txt";

    private const string TempPath = @"Assets\Temp\";
    private const string TempPathMeta = @"Assets\Temp.meta";

    private const string UnityBuiltin = "unity_builtin_extra";

    public static readonly List<List<string>> SectionsFolders = new List<List<string>>()
        {
            new List<string>() { "Sections", "Sections_destroyed" },
            new List<string>() { "Sections CV5", "Section_destroyed_CV5"},
            new List<string>() { "Sections CV9", "Sections_destroyed CV9" }
        };

    private static readonly List<string> MeshDataPaths = new List<string>() { @"Assets\ArtAssets\Mesh\meshData.asset", @"Assets\ArtAssets\Mesh\meshData.cv5.asset", @"Assets\ArtAssets\Mesh\meshData.cv9.asset" };

    private static readonly List<string> TextureNames = new List<string>() { "Diffuse", "Metallic", "Normal" };
    private static readonly List<string> ShipTexturesPostifxes = new List<string>() { "", "_CV5", "_CV9" };
    private static readonly List<int> ShaderProperties = new List<int>() { Shader.PropertyToID("_MainTex"), Shader.PropertyToID("_MetallicGlossMap"), Shader.PropertyToID("_BumpMap") };

    private static readonly List<string> TextureLabels = new List<string>() { "Generate diffuses", "Generate metallics", "Generate normals" };
    private static readonly List<string> ShipsLabels = new List<string>() { "CV3", "CV5", "CV9" };

    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private List<Action> generateMeshes;
    private List<Action> generateAllTextures;
    private List<List<Action>> generateTextures;

    [MenuItem("Tools/Meshes/Editor", false, 201)]
    private static void ShowWindow()
    {
        GetWindow<MeshMerger>().Show();
    }

    private MeshMerger()
    {
        generateMeshes = new List<Action>();
        for (int i = 0; i < MeshDataPaths.Count; i++)
        {
            int index = i;
            generateMeshes.Add(() => MergeMeshes(MeshDataPaths[index], SectionsFolders[index]));
        }

        generateTextures = new List<List<Action>>();
        for (int i = 0; i < TextureLabels.Count; i++)
        {
            int index = i;
            var list = new List<Action>();

            for (int j = 0; j < MeshDataPaths.Count; j++)
            {
                int index2 = j;
                list.Add(() => GenerateTextureArray(ShaderProperties[index], MeshDataPaths[index2], ShipTexturesPostifxes[index2], TextureNames[index], index == 2));
            }

            generateTextures.Add(list);
        }

        generateAllTextures = new List<Action>();
        for (int i = 0; i < MeshDataPaths.Count; i++)
        {
            int index = i;
            generateAllTextures.Add(() =>
                {
                    foreach(var action in generateTextures)
                    {
                        action[index]();
                    }
                });
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Generate meshes");
        Buttons(generateMeshes);
        EditorGUILayout.LabelField("Generate all textures");
        Buttons(generateAllTextures);
        for (int i = 0; i < TextureLabels.Count; i++)
        {
            EditorGUILayout.LabelField(TextureLabels[i]);
            Buttons(generateTextures[i]);
        }
    }

    private void Buttons(List<Action> buttonActions)
    {
        GUILayout.BeginHorizontal();

        for (int i = 0; i < ShipsLabels.Count; i++)
        {
            if (GUILayout.Button(ShipsLabels[i]))
            {
                buttonActions[i]();
            }
        }
        GUILayout.EndHorizontal();
    }

    private void MergeMeshes(string meshDataPath, List<string> folders)
    {
        var errorLogBuilder = new StringBuilder();
        var errorMat = AssetDatabase.LoadAssetAtPath<Material>(ErrorMatPath);
        var matDict = new Dictionary<Material, int>();

        var helper = new List<string>();

        Directory.CreateDirectory(TempPath);
        var tempToPathDict = new Dictionary<string, string>();

        bool stopAssetEditing = true;
        AssetDatabase.StartAssetEditing();

        try
        {
            int current = 0;
            int count = 0;
            foreach (var folder in folders)
            {
                var files = Directory.GetFiles(ParentPath + folder, "*.fbx");
                count += files.Length;
                var mergedFolder = ParentPath + "Merged_" + folder;
                Directory.CreateDirectory(mergedFolder);

                var set = new HashSet<string>(Directory.GetFiles(mergedFolder, "*.asset"));

                foreach (var file in files)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Merging meshes", $"Processed : {current}/{count} meshes", current / (float)count))
                    {
                        if (stopAssetEditing)
                        {
                            stopAssetEditing = false;
                            AssetDatabase.StopAssetEditing();
                        }
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                    current++;
                    if (file.Contains("sekcje_") || file.Contains("wieza_") || file.EndsWith("ship_selections.fbx") || file.Contains("glebokosc") || file.EndsWith("selections_cv5.fbx") || file.EndsWith("selections_cv9.fbx"))
                    {
                        continue;
                    }
                    var objs = AssetDatabase.LoadAllAssetsAtPath(file);
                    bool gotRoot = false;
                    var meshes = new List<Mesh>();
                    var mats = new List<Material>();
                    var transforms = new List<Transform>();
                    foreach (var obj in objs)
                    {
                        var go = obj as GameObject;
                        if (go != null)
                        {
                            if (!go.TryGetComponent(out MeshFilter filter))
                            {
                                if (gotRoot)
                                {
                                    Debug.LogError("asd");
                                }
                                Assert.IsFalse(gotRoot);
                                gotRoot = true;
                                Assert.IsTrue(Mathf.Approximately(go.transform.position.sqrMagnitude, 0f));
                                Assert.IsTrue(Mathf.Approximately(Quaternion.Angle(go.transform.rotation, Quaternion.identity), 0f));
                                Assert.IsTrue(Mathf.Approximately((go.transform.localScale - Vector3.one).sqrMagnitude, 0f));
                            }
                            else
                            {
                                meshes.Add(filter.sharedMesh);
                                Assert.IsNotNull(filter.sharedMesh);
                                transforms.Add(go.GetComponent<Transform>());

                                var renderer = go.GetComponent<MeshRenderer>();
                                var mat = renderer.sharedMaterial;
                                if (mat == null)
                                {
                                    mat = errorMat;

                                    PushError(errorLogBuilder, file, go.name, "has no material");
                                }

                                mats.Add(mat);

                                var path = AssetDatabase.GetAssetPath(mat);
                                if (path.Contains(UnityBuiltin))
                                {
                                    helper.Add(file);
                                }
                                if (!matDict.ContainsKey(mat))
                                {
                                    matDict[mat] = matDict.Count;
                                    Assert.IsTrue(matDict.Count < 255);
                                }

                                if (renderer.sharedMaterials.Length > 1)
                                {
                                    PushError(errorLogBuilder, file, go.name, "has multiple materials");
                                }
                            }
                        }
                    }

                    var combine = new CombineInstance[meshes.Count];
                    int vertCount = 0;
                    for (int i = 0; i < combine.Length; i++)
                    {
                        Color color;
                        if (mats[i].HasProperty(ColorProperty))
                        {
                            color = mats[i].GetColor(ColorProperty);
                        }
                        else
                        {
                            color = Color.white;
                            PushError(errorLogBuilder, file, mats[i].name, "has no color");
                        }
                        color.a = matDict[mats[i]] / 255f;
                        var colors = new Color[meshes[i].vertexCount];
                        for (int j = 0; j < colors.Length; j++)
                        {
                            colors[j] = color;
                        }
                        //meshes[i].colors = colors;
                        meshes[i].SetColors(colors);
                        vertCount += meshes[i].vertexCount;
                        combine[i].mesh = meshes[i];
                        combine[i].transform = transforms[i].localToWorldMatrix;
                    }

                    var mesh = new Mesh();
                    mesh.indexFormat = vertCount > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
                    mesh.CombineMeshes(new CombineInstance[0]);
                    mesh.CombineMeshes(combine);

                    int index = file.LastIndexOf(@"\");

                    var newPath = mergedFolder.Insert(mergedFolder.Length, file.Substring(index, file.Length - index - 3)) + "asset";
                    if (set.Remove(newPath))
                    {
                        var tempPath = $"{TempPath}{tempToPathDict.Count}.asset";
                        AssetDatabase.CreateAsset(mesh, tempPath);
                        tempToPathDict.Add(tempPath, newPath);
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(mesh, newPath);
                    }
                }

                foreach (var path in set)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }

            foreach (var element in tempToPathDict)
            {
                File.Copy(element.Key, element.Value, true);
            }

            AssetDatabase.DeleteAsset(meshDataPath);

            var meshDataBuilder = new StringBuilder();
            var matList = new List<Material>();
            for (int i = 0; i < matDict.Count; i++)
            {
                matList.Add(null);
            }
            foreach (var pair in matDict)
            {
                matList[pair.Value] = pair.Key;
            }
            foreach (var mat in matList)
            {
                var path = AssetDatabase.GetAssetPath(mat);
                if (path.EndsWith(".fbx"))
                {
                    PushError(errorLogBuilder, path, mat.name, "is not mapped");
                }
                else if (path.Contains(UnityBuiltin))
                {
                    foreach (var patho in helper)
                    {
                        PushError(errorLogBuilder, patho, "", "has builtin material");
                    }
                }
                meshDataBuilder.Append(path);
                meshDataBuilder.Append("\n");
            }

            EditorUtility.DisplayProgressBar("Merging meshes", $"Saving assets", 1f);

            var meshDataAsset = new TextAsset(meshDataBuilder.ToString());
            AssetDatabase.CreateAsset(meshDataAsset, meshDataPath);

            if (stopAssetEditing)
            {
                stopAssetEditing = false;
                AssetDatabase.StopAssetEditing();
            }

            Directory.Delete(TempPath, true);
            File.Delete(TempPathMeta);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            errorLogBuilder.Append(ex.ToString());
            Debug.LogException(ex);
        }
        if (stopAssetEditing)
        {
            AssetDatabase.StopAssetEditing();
        }
        EditorUtility.UnloadUnusedAssetsImmediate(true);

        EditorUtility.ClearProgressBar();

        File.WriteAllText(ErrorTextPath, errorLogBuilder.ToString());
    }

    private void GenerateTextureArray(int texID, string meshDataPath, string postfix, string texType, bool normal)
    {
        var lowercaseTexType = texType.ToLower();
        var path = TextureArrayStartPath + texType + "s" + postfix;
        var errorPath = path + "Errors.txt";
        path += ".asset";

        var errorLogBuilder = new StringBuilder();

        try
        {
            var errorMat = AssetDatabase.LoadMainAssetAtPath(ErrorMatPath) as Material;
            var errorTex = AssetDatabase.LoadMainAssetAtPath(ErrorTexPath) as Texture2D;
            var texes = new List<Texture2D>();

            var data = AssetDatabase.LoadMainAssetAtPath(meshDataPath) as TextAsset;
            Assert.IsNotNull(data, "Merge meshes first " + meshDataPath);
            var lines = data.text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            int current = 0;
            int overallCurrent = 0;
            float count = 2f * lines.Length;
            foreach (var line in lines)
            {
                if (EditorUtility.DisplayCancelableProgressBar($"Collecting {lowercaseTexType} textures", $"Processed : {current}/{lines.Length} materials", current / count))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                current++;
                overallCurrent++;
                var mat = AssetDatabase.LoadMainAssetAtPath(line) as Material;
                if (mat == null)
                {
                    Assert.IsTrue(line.Contains(UnityBuiltin) || line.EndsWith(".fbx"), "Cannot find material at " + line);
                    mat = errorMat;
                }

                if (mat == errorMat)
                {
                    texes.Add(null);
                }
                else
                {
                    var tex = mat.GetTexture(texID) as Texture2D;
                    if (tex == null)
                    {
                        PushError(errorLogBuilder, "", line, "has no", lowercaseTexType, "texture");
                    }
                    else
                    {
                        var type = (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex)) as TextureImporter).textureType;
                        if (type != (normal ? TextureImporterType.NormalMap : TextureImporterType.Default))
                        {
                            PushError(errorLogBuilder, "", line, lowercaseTexType, "texture - ", tex.name, "has bad import type -", type.ToString(), "", "", "", "");

                            tex = null;
                        }
                        else if (texes.Count > 0)
                        {
                            if (texes[0].height != tex.height || texes[0].width != tex.width)
                            {
                                PushError(errorLogBuilder, "", line, lowercaseTexType, "texture - ", tex.name, "has different dimensions -", "" + tex.height, "x", "" + tex.width,
                                    "instead of", "" + texes[0].height, "x", "" + texes[0].width, "like", texes[0].name);

                                tex = null;
                            }
                            else if (texes[0].format != tex.format)
                            {
                                PushError(errorLogBuilder, "", line, lowercaseTexType, "texture - ", tex.name, "has different format -", tex.format.ToString(), "instead of", texes[0].format.ToString(),
                                    "like", texes[0].name);

                                tex = null;
                            }
                            else if (texes[0].filterMode != tex.filterMode)
                            {
                                PushError(errorLogBuilder, "", line, lowercaseTexType, "texture - ", tex.name, "has different filter mode -", tex.filterMode.ToString(), "instead of", texes[0].filterMode.ToString(),
                                    "like", texes[0].name);

                                tex = null;
                            }
                            else if (texes[0].wrapMode != tex.wrapMode)
                            {
                                PushError(errorLogBuilder, "", line, lowercaseTexType, "texture - ", tex.name, "has different wrap mode -", tex.wrapMode.ToString(), "instead of", texes[0].wrapMode.ToString(),
                                    "like", texes[0].name);

                                tex = null;
                            }
                            else if (texes[0].mipmapCount != tex.mipmapCount)
                            {
                                PushError(errorLogBuilder, "", line, lowercaseTexType, "texture - ", tex.name, "has different mip map size -", tex.mipmapCount.ToString(), "instead of", texes[0].mipmapCount.ToString(),
                                    "like", texes[0].name);

                                tex = null;
                            }
                        }
                    }
                    texes.Add(tex);
                }
            }

            count /= 2f;

            int width = texes[0].width;
            int height = texes[0].height;
            var textureArray = new Texture2DArray(width, height, texes.Count, texes[0].format, texes[0].mipmapCount > 1, normal);

            textureArray.filterMode = texes[0].filterMode;
            textureArray.wrapMode = texes[0].wrapMode;

            current = 0;
            count += texes.Count;
            for (int i = 0; i < texes.Count; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar($"Generating {lowercaseTexType} texture array", $"Processed : {current}/{texes.Count} textures", overallCurrent / count))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                current++;
                overallCurrent++;
                if (texes[i] == null)
                {
                    texes[i] = errorTex;
                }

                var rawData = texes[i].GetRawTextureData();

                int startIndex = 0;
                for (int j = 0; j < texes[0].mipmapCount; j++)
                {
                    textureArray.SetPixelData(rawData, j, i, startIndex);
                    int nextIndex = width * height;
                    for (int k = 0; k < j; k++)
                    {
                        nextIndex /= 4;
                    }
                    startIndex += nextIndex;
                }
            }
            textureArray.Apply(false);

            EditorUtility.DisplayProgressBar($"Generating {lowercaseTexType} texture array", $"Saving assets", 1f);

            var outArray = AssetDatabase.LoadMainAssetAtPath(path) as Texture2DArray;
            if (outArray == null)
            {
                AssetDatabase.CreateAsset(textureArray, path);
            }
            else
            {
                EditorUtility.CopySerialized(textureArray, outArray);
            }
            AssetDatabase.SaveAssets();
        }
        catch(Exception ex)
        {
            errorLogBuilder.Append(ex.ToString());
            Debug.LogException(ex);
        }
        EditorUtility.UnloadUnusedAssetsImmediate(true);

        EditorUtility.ClearProgressBar();

        File.WriteAllText(errorPath, errorLogBuilder.ToString());
    }

    static void PushError(StringBuilder errorLog, string file, string name, params string[] errors)
    {
        var builder = new StringBuilder();
        if (file.Length > 0)
        {
            builder.Append(file);
            builder.Append(", ");
        }
        builder.Append(name);
        foreach (var error in errors)
        {
            builder.Append(" ");
            builder.Append(error);
        }
        Debug.LogError(builder.ToString());
        errorLog.Append(builder.ToString());
        errorLog.Append("\n");
    }
}
