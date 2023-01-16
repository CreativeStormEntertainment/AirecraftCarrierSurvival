using GambitUtils;
using UnityEngine;

[ExecuteAlways]
public class CameraTest : MonoBehaviour
{
    private void Update()
    {
        var cameraMan = SceneUtils.FindObjectOfType<CameraManager>();
        if (cameraMan.Brain.transform.position.magnitude > 16000f)
        {
            string text = "";
            if (cameraMan.Brain == null)
            {
                text = "Brains missing";
            }
            else
            {
                var camera = cameraMan.Brain.ActiveVirtualCamera;
                if (camera == null)
                {
                    text = "Vca missing";
                }
                else
                {
                    var go = camera.VirtualCameraGameObject;
                    if (go == null)
                    {
                        text = "Unknown vca object";
                    }
                    else
                    {
                        int overflow = 0;
                        var trans = go.transform;
                        Transform ancestor = null;
                        while (trans != null)
                        {
                            ancestor = trans;
                            text = trans.name + "/" + text;
                            trans = trans.parent;
                            if (++overflow > 1000)
                            {
                                text += "--ovflow--";
                            }
                        }
                        foreach (var director in ancestor.GetComponentsInChildren<UnityEngine.Playables.PlayableDirector>())
                        {
                            if (director.isActiveAndEnabled)
                            {
                                text += "; " + director.name + ":" + director.time.ToString("F5");
                            }
                        }
                    }
                }
                cameraMan.Brain.enabled = false;
            }

            cameraMan.enabled = false;
            this.enabled = false;

            Debug.LogError(text);
            //ErrorPopup.Instance.Show(text);

            return;
        }
    }
}
