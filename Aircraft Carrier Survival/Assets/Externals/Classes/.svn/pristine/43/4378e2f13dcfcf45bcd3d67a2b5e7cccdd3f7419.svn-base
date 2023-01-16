using UnityEditor;
using UnityEngine;

public static class RenameShortcut {
    [MenuItem("Edit/Rename Util %r", false, -101)]
    public static void Rename() {
        var e = new Event { keyCode = KeyCode.F2, type = EventType.KeyDown };
        EditorWindow.focusedWindow.SendEvent(e);
    }
}
