using FMODUnity;
using GambitUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class CreateAdmiralMenu : MonoBehaviour
{
    private static readonly EAdmiralCustomizationCategory[] CustomizationCategoryList = EnumUtils.GetValues<EAdmiralCustomizationCategory>();

    public static CreateAdmiralMenu Instance;

    public EIslandBuff ChoosenIslandBuff
    {
        get;
        set;
    } = EIslandBuff.None;

    [SerializeField]
    private List<AdmiralVisualObjects> visualObjects = null;
    [SerializeField]
    private Text chosenOrderText = null;
    [SerializeField]
    private Button chooseOrderButton = null;
    [SerializeField]
    private GameObject chooseOrderWindow = null;
    [SerializeField]
    private Button continueButton = null;
    [SerializeField]
    private List<string> categoryNames = null;
    [SerializeField]
    private GameObject choosingPanel = null;
    [SerializeField]
    private Text choosingPanelTitle = null;
    [SerializeField]
    private RectTransform choosingPanelContent = null;
    [SerializeField]
    private GameObject optionPrefab = null;
    [SerializeField]
    private GameObject optionsPrefab = null;

    [SerializeField]
    private MyInputField admiralNameField = null;

    [SerializeField]
    private List<string> portraitsNames = null;
    [SerializeField]
    private SpriteAtlas portraitsAtlas = null;
    [SerializeField]
    private Image portraitImage = null;

    [SerializeField]
    private List<StudioEventEmitter> voices = null;

    [SerializeField]
    private TutorialCaptainScene tutorial = null;
    [SerializeField]
    private bool forceGameMode = false;
    [SerializeField]
    private EGameMode gameMode = EGameMode.Fabular;

    [SerializeField]
    private List<RectTransform> categoryButtons = null;

    [SerializeField]
    private Vector3 selectedButtonOffset = new Vector3(-15f, 0f);

    [SerializeField]
    private OfficerList officerList = null;

    private int currentPortrait = 0;

    private Dictionary<EAdmiralCustomizationCategory, AdmiralVisualObjects> visualOptions;

    private Dictionary<EOfficerSkills, List<GameObject>> skillsStars;

    private Dictionary<EAdmiralCustomizationCategory, GameObject> optionsChoosePanel = null;

    private GameObject currentlySelectedPanel = null;
    private RectTransform choosePanelRectTransorm = null;

    private void Awake()
    {
        Instance = this;
        var saveMan = SaveManager.Instance;

        continueButton.interactable = false;

        chooseOrderButton.onClick.AddListener(() => chooseOrderWindow.SetActive(true));

        currentPortrait = saveMan.Data.AdmiralPortrait;
        choosingPanel.SetActive(false);

        optionsChoosePanel = new Dictionary<EAdmiralCustomizationCategory, GameObject>();

        choosePanelRectTransorm = choosingPanel.GetComponent<RectTransform>();

        optionsChoosePanel.Add(EAdmiralCustomizationCategory.Trait, Instantiate(optionsPrefab, choosingPanelContent));
        Transform traitPanelParent = optionsChoosePanel[EAdmiralCustomizationCategory.Trait].transform.GetChild(0).transform.GetChild(0).transform;
        optionsChoosePanel[EAdmiralCustomizationCategory.Trait].gameObject.SetActive(false);
        var locMan = LocalizationManager.Instance;

        skillsStars = new Dictionary<EOfficerSkills, List<GameObject>>();
        skillsStars.Add(EOfficerSkills.CommandingAirForce, new List<GameObject>());
        skillsStars.Add(EOfficerSkills.CommandingNavy, new List<GameObject>());

        visualOptions = new Dictionary<EAdmiralCustomizationCategory, AdmiralVisualObjects>();
        foreach (AdmiralVisualObjects visual in visualObjects)
        {
            visualOptions.Add(visual.Category, visual);
            foreach (GameObject o in visual.Options)
            {
                o.SetActive(false);
            }
            visual.CurrentOption.SetActive(true);
            visual.ButtonText.text = locMan.GetText(visual.Names[visual.Options.IndexOf(visual.CurrentOption)]);
        }

        for (int i = (int)EAdmiralCustomizationCategory.Voice; i < CustomizationCategoryList.Length; i++)
        {
            EAdmiralCustomizationCategory category = CustomizationCategoryList[i];

            optionsChoosePanel.Add(category, Instantiate(optionsPrefab, choosingPanelContent));
            optionsChoosePanel[category].SetActive(false);

            if (visualOptions.TryGetValue(category, out AdmiralVisualObjects visualObject) && visualObject != null)
            {
                Transform parent = optionsChoosePanel[category].transform.GetChild(0).transform.GetChild(0).transform;
                int counter = 0;
                foreach (GameObject o in visualObject.Options)
                {
                    GameObject go = Instantiate(optionPrefab, parent);
                    go.GetComponentInChildren<Text>().text = locMan.GetText(visualOptions[(EAdmiralCustomizationCategory)i].Names[counter]);
                    int tmp = counter;
                    go.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        OptionChoosed(category, tmp);
                    });
                    counter++;
                }
            }
        }

        portraitImage.sprite = officerList.Portraits[currentPortrait].Square;

        admiralNameField.Setup(saveMan.Data.AdmiralName);

        if (saveMan.Data.AdmiralVisual.Count != 0)
        {
            for (int i = (int)EAdmiralCustomizationCategory.Face; i < CustomizationCategoryList.Length; i++)
            {
                EAdmiralCustomizationCategory category = CustomizationCategoryList[i];
                if (visualOptions.TryGetValue(category, out AdmiralVisualObjects visualObject) && visualObject != null)
                {
                    for (int j = 0; j < visualObject.Options.Count; j++)
                    {
                        if (visualObject.Options[j].transform.GetSiblingIndex() == saveMan.Data.AdmiralVisual[i - (int)EAdmiralCustomizationCategory.Face])
                        {
                            OptionChoosed(category, j);
                            break;
                        }
                    }
                }
            }
        }
#if UNITY_EDITOR
        if (forceGameMode)
        {
            saveMan.Data.GameMode = gameMode;
        }
#endif
        if (saveMan.Data.GameMode == EGameMode.Tutorial)
        {
            tutorial.gameObject.SetActive(true);
            tutorial.enabled = true;
        }
        else
        {
            tutorial.enabled = false;
            tutorial.gameObject.SetActive(false);
        }
        chosenOrderText.text = LocalizationManager.Instance.GetText("ChooseOrderButton");
        if (saveMan.Data.GameMode == EGameMode.Sandbox)
        {
            chooseOrderButton.transform.parent.gameObject.SetActive(false);
            continueButton.interactable = true;
        }
        else if (saveMan.Data.FinishedTutorial)
        {
            SetChosenOrder(EIslandBuff.EnterDefencePosition);
        }
    }

    public void SelectCategory(int category)
    {
        if (currentlySelectedPanel == null)
        {
            choosingPanel.SetActive(true);
        }
        else
        {
            currentlySelectedPanel.SetActive(false);
        }

        currentlySelectedPanel = optionsChoosePanel[(EAdmiralCustomizationCategory)category];
        currentlySelectedPanel.SetActive(true);

        choosingPanelTitle.text = LocalizationManager.Instance.GetText(categoryNames[category]);

        RectTransform rectTransform = currentlySelectedPanel.transform.GetChild(0).transform.GetChild(0).transform.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        float maxHeight = 543f;
        choosePanelRectTransorm.sizeDelta = new Vector2(choosePanelRectTransorm.sizeDelta.x, Mathf.Min(rectTransform.rect.height + 55f, maxHeight));

        foreach (var b in categoryButtons)
        {
            b.anchoredPosition = Vector3.zero;
            b.GetComponent<Button>().interactable = true;
        }
        categoryButtons[category].anchoredPosition = selectedButtonOffset;
        categoryButtons[category].GetComponent<Button>().interactable = false;
    }

    public void AcceptChangesButton()
    {
        var saveMan = SaveManager.Instance;
        saveMan.Data.AdmiralName = admiralNameField.Text;
        saveMan.Data.AdmiralVoice = EVoiceType.Admiral1 + visualOptions[EAdmiralCustomizationCategory.Voice].Options.IndexOf(visualOptions[EAdmiralCustomizationCategory.Voice].CurrentOption);
        saveMan.Data.AdmiralPortrait = currentPortrait;
        saveMan.Data.AdmiralVisual.Clear();
        for (int i = (int)EAdmiralCustomizationCategory.Face; i < CustomizationCategoryList.Length; i++)
        {
            saveMan.Data.AdmiralVisual.Add(visualOptions[CustomizationCategoryList[i]].CurrentOption.transform.GetSiblingIndex());
        }
        if (ChoosenIslandBuff != EIslandBuff.None)
        {
            SaveManager.Instance.Data.MissionRewards.AdmiralUnlockedOrders |= (1 << (int)ChoosenIslandBuff);
        }
        LoadingManager.Instance.CurrentScene = ESceneType.Intermission;
    }

    public void BackToMenuButton()
    {
        LoadingManager.Instance.CurrentScene = ESceneType.MainMenu;
    }

    public void ChangePortrait(bool add)
    {
        currentPortrait += add ? 1 : -1;
        if (currentPortrait < 0)
        {
            currentPortrait = portraitsNames.Count - 1;
        }
        if (currentPortrait >= portraitsNames.Count)
        {
            currentPortrait = 0;
        }
        portraitImage.sprite = officerList.Portraits[currentPortrait].Square;
    }

    public void SetChosenOrder(EIslandBuff islandBuff)
    {
        chosenOrderText.text = LocalizationManager.Instance.GetText(islandBuff.ToString() + "Title");
        ChoosenIslandBuff = islandBuff;
        continueButton.interactable = true;
    }

    private void OptionChoosed(EAdmiralCustomizationCategory cat, int optionNumber)
    {
        if (cat == EAdmiralCustomizationCategory.Trait)
        {
            Debug.LogError("Trait category");
        }
        else
        {
            AdmiralVisualObjects admiralVisual = visualOptions[cat];
            admiralVisual.CurrentOption.SetActive(false);
            admiralVisual.CurrentOption = admiralVisual.Options[optionNumber];
            admiralVisual.ButtonText.text = LocalizationManager.Instance.GetText(admiralVisual.Names[optionNumber]);
            admiralVisual.CurrentOption.SetActive(true);
            if (cat == EAdmiralCustomizationCategory.Voice)
            {
                int index = admiralVisual.Options.IndexOf(admiralVisual.CurrentOption);
                for (int i = 0; i < voices.Count; i++)
                {
                    var voice = voices[i];
                    if (i == index)
                    {
                        if (!voice.IsPlaying())
                        {
                            voice.Play();
                        }
                    }
                    else
                    {
                        voice.Stop();
                    }
                }
            }
        }
    }
}
