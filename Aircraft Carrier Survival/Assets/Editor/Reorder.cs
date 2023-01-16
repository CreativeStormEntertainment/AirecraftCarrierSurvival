using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Reorder
{
    [MenuItem("Tools/Utils/Set game object order", false, 205)]
    private static void SetGameObjectOrder()
    {
        Selection.activeGameObject.transform.SetAsFirstSibling();
    }
}
