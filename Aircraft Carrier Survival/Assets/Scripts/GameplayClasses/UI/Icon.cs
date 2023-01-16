using UnityEngine;

public class Icon : MonoBehaviour
{
    protected int fillID;

    protected void Start()
    {
        fillID = Shader.PropertyToID("_Fill");
    }

    public void SetFill(Renderer renderer, float fill)
    {
        renderer.material.SetFloat(fillID, Mathf.Lerp(.15f, 1f, fill));
    }
}
