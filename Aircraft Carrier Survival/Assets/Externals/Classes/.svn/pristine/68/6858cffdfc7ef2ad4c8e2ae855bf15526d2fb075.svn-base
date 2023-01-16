using GambitDebug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GambitLocalization
{
    public class LocalizationContext
    {
        private Dictionary<string, string[]> localizations;
        private Dictionary<string, Sprite[]> localizedImages;

        public LocalizationContext(Dictionary<string, string[]> localizations, Dictionary<string, Sprite[]> localizedImages)
        {
            this.localizations = localizations;
            this.localizedImages = localizedImages;
        }

        public string GetText(string key)
        {
            if (localizations.TryGetValue(key, out string[] value))
            {
                return value[LocalizationManager.Instance.LanguageIndex];
            }
            Dbg.W("Localization", "The following key: " + key + " couldn't be found");
            return key;
        }

        public Sprite GetSprite(string key)
        {
            if (localizedImages.TryGetValue(key, out Sprite[] value))
            {
                return value[LocalizationManager.Instance.LanguageIndex];
            }
            Dbg.E("Localization", "The following key: " + key + " couldn't be found");
            return null;
        }
    }
}
