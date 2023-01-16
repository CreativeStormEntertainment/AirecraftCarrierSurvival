using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GambitUtils
{
    public struct EditorUtils
    {
        public static bool SetDirtyOnEndChangeCheck(UnityEngine.Object target)
        {
            if (EditorGUI.EndChangeCheck())
            {
                SetSceneOrPrefabDirty(target);
                return true;
            }
            return false;
        }

        public static void SetSceneOrPrefabDirty(UnityEngine.Object target)
        {
            if (target && PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                EditorUtility.SetDirty(target);
            }
            else
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
}
