using UnityEditor;
using UnityEngine;

public class HierarchyFix : MonoBehaviour
{
    [MenuItem("Tools/Utils/FixObject")]
    private static void FixObject()
    {
        var trans = Selection.activeTransform;
        if (trans.parent != null)
        {
            trans.SetAsFirstSibling();
            EditorUtility.SetDirty(trans);
            EditorUtility.SetDirty(trans.parent);
        }
    }

    [MenuItem("Tools/Utils/MoveUp")]
    private static void MoveUp()
    {
        var trans = Selection.activeTransform;
        if (trans.parent != null)
        {
            trans.SetSiblingIndex(trans.GetSiblingIndex() - 1);
            EditorUtility.SetDirty(trans);
            EditorUtility.SetDirty(trans.parent);
        }
    }

    [MenuItem("Tools/Utils/MoveDown")]
    private static void MoveDown()
    {
        var trans = Selection.activeTransform;
        if (trans.parent != null)
        {
            int index = trans.GetSiblingIndex();
            trans.SetSiblingIndex(index + 1);
            if (trans.GetSiblingIndex() == index)
            {
                trans.SetAsFirstSibling();
            }
            EditorUtility.SetDirty(trans);
            EditorUtility.SetDirty(trans.parent);
        }
    }
}