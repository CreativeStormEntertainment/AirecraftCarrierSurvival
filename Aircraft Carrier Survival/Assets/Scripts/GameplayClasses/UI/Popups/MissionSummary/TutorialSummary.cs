using UnityEngine;
using UnityEngine.UI;

public class TutorialSummary : MonoBehaviour
{
    [SerializeField]
    private Button mainMenuButton = null;
    [SerializeField]
    private Button nextTutorialButton = null;
    [SerializeField]
    private GameObject demoStuff = null;

    private SOTacticMap nextTutorial;

    private void Start()
    {
        mainMenuButton.onClick.AddListener(MainMenu);
        nextTutorialButton.onClick.AddListener(NextTutorial);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        nextTutorial = TacticManager.Instance.SOTacticMap.Overrides.NextMission;
        nextTutorialButton.gameObject.SetActive(nextTutorial != null);
        //demoStuff.SetActive(nextTutorial == null);
    }

    private void MainMenu()
    {
        LoadingManager.Instance.CurrentScene = ESceneType.MainMenu;
    }

    private void NextTutorial()
    {
        var saveMan = SaveManager.Instance;
        saveMan.Data.MissionInProgress.InProgress = false;
        saveMan.TransientData.FabularTacticMap = nextTutorial;
        saveMan.Data.SavedInIntermission = false;
        LoadingManager.Instance.ForceReloadCurrentScene();
    }
}
