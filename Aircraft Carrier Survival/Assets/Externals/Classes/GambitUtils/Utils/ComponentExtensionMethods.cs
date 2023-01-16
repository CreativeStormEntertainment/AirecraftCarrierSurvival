using UnityEngine;

namespace GambitUtils
{
    public static class ComponentExtensionMethods
    {
        public static void SetGameObjectActive(this Component component, bool active)
        {
            component.gameObject.SetActive(active);
        }

        public static bool IsGameObjectActiveSelf(this Component component)
        {
            return component.gameObject.activeSelf;
        }

        public static bool IsGameObjectActiveInHierarchy(this Component component)
        {
            return component.gameObject.activeInHierarchy;
        }
    }
}
