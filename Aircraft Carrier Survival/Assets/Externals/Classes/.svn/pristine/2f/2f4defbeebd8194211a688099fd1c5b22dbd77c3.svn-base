#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GambitUtils
{
    public struct EditorApplicationUtils
    {
#if UNITY_EDITOR
        public static bool IsCompilingOrUpdating => EditorApplication.isCompiling || EditorApplication.isUpdating;
        public static bool IsInEditMode => !EditorApplication.isPlaying;
#else
        public static bool IsCompilingOrUpdating => false;
        public static bool IsInEditMode => false;
#endif
    }
}