using System.Collections.Generic;

public class ExternalLibsInfo
{
    private const string FMOD = "Made with FMOD Studio by Firelight Technologies Pty Ltd.";
    private const string ExcelReader = "ExcelDataReader, Copyright (c) 2014, full license file in StreamingAssets/ExcelDataReader/LICENSE";
    private const string NuGetForUnity = "NuGetForUnity, Copyright (c) 2018 Patrick McCarthy, full license file in StreamingAssets/NuGetForUnity/LICENSE";
    private const string UnitySimpleEditorShortcuts = "UnitySimpleEditorShortcutsToolsCollection, Copyright (c) 2017 Gojko Radonjić, full license file in StreamingAssets/NuGetForUnity/LICENSE";

    public static IEnumerable<string> GetAllLibsInfos()
    {
        yield return FMOD;
        yield return ExcelReader;
        yield return NuGetForUnity;
        yield return UnitySimpleEditorShortcuts;
    }
}
