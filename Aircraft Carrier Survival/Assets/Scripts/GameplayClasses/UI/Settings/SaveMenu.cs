using GambitUtils;
using GambitUtils.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveMenu : MonoBehaviour, IFakeScrollView
{
    public event Action WantsToClose = delegate { };

    public event Action SaveDeleted = delegate { };

    public static readonly Comparison<SaveMenuData> Comparison = (x, y) => Comparer<long>.Default.Compare(x.WriteTime, y.WriteTime);

    [SerializeField]
    private GameObject container = null;
    [SerializeField]
    private SaveConfirm confirmWindow = null;
    [SerializeField]
    private Text titleContainer = null;
    [SerializeField]
    private MyInputField inputField = null;
    [SerializeField]
    private Button saveButton = null;
    [SerializeField]
    private FakeScrollView fakeScrollView = null;

    [SerializeField]
    private GameObject saveButtonObj = null;
    [SerializeField]
    private RectTransform content = null;
    [SerializeField]
    private RectTransform scrollbar = null;

    [SerializeField]
    private string saveText = "SaveTitle";
    [SerializeField]
    private string loadText = "LoadTitle";
    [SerializeField]
    private string overrideTitleText = "OverrideTitle";
    [SerializeField]
    private string overrideButtonText = "OverrideButton";
    [SerializeField]
    private string deleteTitleText = "DeleteTitle";
    [SerializeField]
    private string deleteButtonText = "DeleteButton";

    private string saveTextLocalized;
    private string loadTextLocalized;
    private string deleteTitleTextLocalized;
    private string deleteButtonTextLocalized;

    private Vector2 contentSaveSize;
    private Vector2 contentLoadSize;
    private Vector2 scrollbarSaveSize;
    private Vector2 scrollbarLoadSize;

    private List<SaveMenuData> saveList = new List<SaveMenuData>();

    private string selectedSave;
    private bool saveMode;
    private bool delete;

    private bool setup;

    private void Awake()
    {
        saveButton.onClick.AddListener(Save);

        if (fakeScrollView.Initialize<SaveButton>(out var buttons))
        {
            foreach (var button in buttons)
            {
                button.Clicked += () => OnSaveItemClicked(button);
                button.Deleted += () => OnSaveItemDeleted(button);
            }
        }

        confirmWindow.ConfirmClosed += OnConfirmClosed;
        inputField.TextChanged += OnTextChanged;

        UpdateLocalization();
        LocalizationManager.Instance.LanguageChanged += UpdateLocalization;

        contentSaveSize = contentLoadSize = content.offsetMin;
        contentLoadSize.y -= 40f;
        scrollbarSaveSize = scrollbarLoadSize = scrollbar.offsetMin;
        scrollbarLoadSize.y -= 40f;
    }

    public void OnDestroy()
    {
        LocalizationManager.Instance.LanguageChanged -= UpdateLocalization;
    }

    public bool Show()
    {
        return container.activeSelf;
    }

    public void ForceHide()
    {
        container.SetActive(false);
        confirmWindow.Hide();
    }

    public bool SetShow(bool show, bool saveMode = true)
    {
        this.saveMode = saveMode;

        if (show)
        {
            inputField.gameObject.SetActive(saveMode);
            saveButtonObj.SetActive(saveMode);
            if (saveMode)
            {
                inputField.Setup(saveTextLocalized);
                OnTextChanged();
                titleContainer.text = saveTextLocalized;

                content.offsetMin = contentSaveSize;
                scrollbar.offsetMin = scrollbarSaveSize;
            }
            else
            {
                titleContainer.text = loadTextLocalized;

                content.offsetMin = contentLoadSize;
                scrollbar.offsetMin = scrollbarLoadSize;
            }
            if (setup)
            {
                Refresh();
            }
            else
            {
                this.StartCoroutineActionAfterPredicate(Refresh, () => !fakeScrollView.LayoutInitialized);
            }
        }
        else if (confirmWindow.gameObject.activeSelf)
        {
            confirmWindow.Hide();
            return false;
        }
        container.SetActive(show);
        return true;
    }

    private void Save()
    {
        var locMan = LocalizationManager.Instance;
        foreach (var save in saveList)
        {
            if (save.Name == inputField.Text)
            {
                delete = false;
                confirmWindow.Show(locMan.GetText(overrideTitleText), locMan.GetText(overrideButtonText));
                return;
            }
        }

        RealSave();
        saveList.Add(new SaveMenuData(inputField.Text, DateTime.Now.Ticks));
        saveList.Sort(Comparison);

        fakeScrollView.SetContent(saveList);
    }

    private void OnSaveItemClicked(SaveButton button)
    {
        if (saveMode)
        {
            inputField.Setup(button.Name);
            OnTextChanged();
        }
        else
        {
            var saveMan = SaveManager.Instance;
            if (saveMan.LoadData(button.Name))
            {
                LoadingManager.Instance.ChangeScene(saveMan.Data);
            }
        }
    }

    private void OnSaveItemDeleted(SaveButton button)
    {
        selectedSave = button.Name;
        delete = true;
        confirmWindow.Show(deleteTitleTextLocalized, deleteButtonTextLocalized);
        SaveDeleted();
    }

    private void OnConfirmClosed(bool success)
    {
        if (success)
        {
            var saveMan = SaveManager.Instance;
            if (delete)
            {
                if (selectedSave != null)
                {
                    saveMan.DeleteData(selectedSave);
                    int i;
                    for (i = 0; i < saveList.Count; i++)
                    {
                        if (saveList[i].Name == selectedSave)
                        {
                            break;
                        }
                    }
                    if (i < saveList.Count)
                    {
                        saveList.RemoveAt(i);
                        fakeScrollView.SetContent(saveList);
                    }
                }
            }
            else
            {
                RealSave();
            }
        }
        selectedSave = null;
    }

    private void OnTextChanged()
    {
        saveButton.interactable = !string.IsNullOrWhiteSpace(inputField.Text);
    }

    private void Refresh()
    {
        setup = true;
        saveList.Clear();
        foreach (var data in SaveManager.Instance.GetSaves())
        {
            AddSave(data);
        }
        fakeScrollView.SetContent(saveList);
    }

    private void AddSave(SaveMenuData data)
    {
        saveList.Add(data);
        saveList.Sort(Comparison);
    }

    private void RealSave()
    {
        var gameSceneMan = GameSceneManager.Instance;
        if (gameSceneMan == null)
        {
            IntermissionManager.Instance.UpdateSave();
        }
        else
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.Tutorial)
            {
                return;
            }
            gameSceneMan.UpdateSave();
        }
        SaveManager.Instance.SaveData(inputField.Text);
        WantsToClose();
    }

    private void UpdateLocalization()
    {
        var locMan = LocalizationManager.Instance;
        saveTextLocalized = locMan.GetText(saveText);
        loadTextLocalized = locMan.GetText(loadText);
        deleteTitleTextLocalized = locMan.GetText(deleteTitleText);
        deleteButtonTextLocalized = locMan.GetText(deleteButtonText);
    }
}
