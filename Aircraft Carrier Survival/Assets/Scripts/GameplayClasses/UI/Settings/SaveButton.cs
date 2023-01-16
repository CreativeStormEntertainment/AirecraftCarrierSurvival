using UnityEngine;
using UnityEngine.UI;
using GambitUtils.UI;
using System;

public class SaveButton : MonoBehaviour, IFakeScrollButton
{
    public event Action Clicked = delegate { };
    public event Action Deleted = delegate { };

    public string Name => text.text;

    public Transform FakeScrollButtonTransform => transform;

    [SerializeField]
    private Text text = null;
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private Button deleteButton = null;

    private void Awake()
    {
        button.onClick.AddListener(() => Clicked());
        deleteButton.onClick.AddListener(() => Deleted());
    }

    public void ClearFakeScrollButtonContent()
    {
        text.text = null;
    }

    public void SetupFakeScrollButtonContent(object content)
    {
        text.text = ((SaveMenuData)content).Name;
    }
}
