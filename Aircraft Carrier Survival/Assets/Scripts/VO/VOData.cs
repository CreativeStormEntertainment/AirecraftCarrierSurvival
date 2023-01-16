using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VOData", menuName = "Datas/VOData", order = 1)]
public class VOData : ScriptableObject
{
    public List<VOContainer> vosList = new List<VOContainer>();
    private Dictionary<string, Dictionary<EVoiceOver, AudioClip>> VOs = new Dictionary<string, Dictionary<EVoiceOver, AudioClip>>();

    Dictionary<EVoiceOver, AudioClip> currentLanguage = null;

    public void Init()
    {
        for (int i = 0; i < vosList.Count; ++i)
        {
            var language = new Dictionary<EVoiceOver, AudioClip>();
            for (int j = 0; j < vosList[i].Data.Count; ++j)
            {
                language.Add(vosList[i].Data[j].ID, vosList[i].Data[j].Clip);
            }
            VOs.Add(vosList[i].Name, language);
        }
    }

    public void SetLanguage(string lang)
    {
        Dictionary<EVoiceOver, AudioClip> value = null;

        if (!VOs.TryGetValue(lang, out value))
        {
            Debug.LogWarning("Language - " + lang + " not found in VO");
            value = VOs["en"];
        }
        currentLanguage = value;
    }

    public AudioClip GetClip(EVoiceOver id)
    {
        AudioClip value = null;

        if (currentLanguage != null)
        {
            if (!currentLanguage.TryGetValue(id, out value))
            {
                Debug.LogWarning("No VO - " + id);
            }
        }
        return value;
    }
}
