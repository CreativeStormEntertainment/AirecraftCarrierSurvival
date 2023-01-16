using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GambitUtils.UI
{
    public static class DropdownExtensionMethods
    {
        public static void SetOptions<T>(this Dropdown dropdown, string firstItemTitle, Func<T, string> getItemTitle, params T[] items)
        {
            List<string> titles = new List<string>();
            if (!string.IsNullOrEmpty(firstItemTitle))
            {
                titles.Add(firstItemTitle);
            }
            foreach (var item in items)
            {
                titles.Add(getItemTitle(item));
            }
            dropdown.ClearOptions();
            dropdown.AddOptions(titles);
        }

        public static void SetListener(this Dropdown dropdown, UnityAction<int> onValueChanged)
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(onValueChanged);
        }
    }
}