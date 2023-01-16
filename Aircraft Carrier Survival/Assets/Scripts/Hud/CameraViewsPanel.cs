using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraViewsPanel : MonoBehaviour
{
    [SerializeField]
    private List<CameraViewData> datas = null;
    [SerializeField]
    private Text text = null;

    private ECameraView current;

    private void Start()
    {
        CameraManager.Instance.ViewChanged += OnCameraViewChanged;
        for (int i = 0; i < datas.Count; i++)
        {
            var view = (ECameraView)i;
            datas[i].Button.onClick.AddListener(() => SetCurrentView(view));
            datas[i].Highlight.SetActive(false);
        }
    }

    public void SelectNextView()
    {
        if (CameraManager.Instance.CurrentCameraView == ECameraView.PreviewCamera)
        {
            SetCurrentView(current);
        }
        else
        {
            SetCurrentView((ECameraView)((int)(current + 1) % 4));
        }
    }

    private void OnCameraViewChanged(ECameraView view)
    {
        if ((int)view < datas.Count)
        {
            SetView(view);
        }
    }

    public void SetCurrentView(ECameraView view)
    {
        SetView(view);
        CameraManager.Instance.SwitchMode(view);
    }

    private void SetView(ECameraView view)
    {
        datas[(int)current].Highlight.SetActive(false);
        current = view;
        datas[(int)current].Highlight.SetActive(true);
        text.text = LocalizationManager.Instance.GetText(view.ToString());
    }
}
