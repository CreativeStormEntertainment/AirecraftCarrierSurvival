using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class ObjectivesManager : MonoBehaviour, IPopupPanel, IEnableable
{
    public event Action<bool> ObjectiveChanged = delegate { };
    public event Action<int, bool> ObjectiveFinishing = delegate { };
    public event Action<int, bool> ObjectiveFinished = delegate { };
    public event Action ObjectivesOpened = delegate { };

    public static bool Ignore = false;
    public static ObjectivesManager Instance = null;

    public List<Objective> Objectives => newObjectives;
    public EWindowType Type => EWindowType.Other;

    [SerializeField]
    private List<ObjectiveMissionData> objectives = null;
    [SerializeField]
    private GameObject objectivePrefab = null;

    [SerializeField]
    private RectTransform mainRect = null;

    [SerializeField]
    private GameObject objectiveCompletePopup = null;
    [SerializeField]
    private GameObject objectiveFailedPopup = null;
    [SerializeField]
    private GameObject panel = null;

    [SerializeField]
    private string finalObjectiveTitleID = null;
    [SerializeField]
    private string finalObjectiveDescID = null;

    private List<ObjectiveObject> objectivesObjects = null;

    private Vector2 startPos;

    private List<Objective> newObjectives;
    private Dictionary<int, bool> finished;
    private bool loading;

    private bool disable;

    private List<int> dummy = new List<int>();

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        newObjectives = new List<Objective>();
        finished = new Dictionary<int, bool>();
    }

    private void Start()
    {
        objectivesObjects = new List<ObjectiveObject>();
        //foreach (ObjectiveMissionData data in objectives)
        //{
        //    objectivesObjects.Add(Instantiate(objectivePrefab, mainRect).GetComponent<ObjectiveObject>().Setup(data.Title, data.Steps, stepPrefab, data.Hidden));
        //}
        UpdateRect();
        UpdateNumeration();

        //startPos = mainRect.anchoredPosition;
        //this.StartCoroutineActionAfterFrames(() =>
        //{
        //    mainRect.anchoredPosition = startPos;
        //    mainRect.gameObject.SetActive(false);
        //}, 10);
    }

    private void Update()
    {
        foreach (var objective in newObjectives)
        {
            objective.Update();
        }
        UpdateRect();
    }

    public void SetEnable(bool enable)
    {
        disable = !enable;
        if (disable && panel.activeSelf)
        {
            SetShowObjectivesPanel(false);
        }
    }

    public void Setup()
    {
        SetShowObjectivesPanel(SaveManager.Instance.Data.GameMode != EGameMode.Sandbox);

        UpdateRect();

        objectiveCompletePopup.SetActive(false);
        objectiveFailedPopup.SetActive(false);
    }

    public void LoadStart()
    {
        loading = true;
    }

    public void LoadData(List<ObjectiveSaveData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (i < newObjectives.Count)
            {
                newObjectives[i].LoadData(list[i]);
            }
        }

        foreach(var objective in newObjectives)
        {
            objective.LateLoad();
        }

        loading = false;
    }

    public void SaveData(List<ObjectiveSaveData> list)
    {
        list.Clear();

        foreach (var objective in newObjectives)
        {
            list.Add(objective.SaveData());
        }
    }

    public void UpdateObjectives(List<ObjectiveData> list)
    {
        SimpleTimerLabel.Instance.Stop();
        UIManager.Instance.ShowAllies(false, dummy);

        finished.Clear();
        foreach (var objective in newObjectives)
        {
            objective.Stop(true);
        }
        newObjectives.Clear();
        foreach (var obj in objectivesObjects)
        {
            Destroy(obj.gameObject);
        }
        objectivesObjects.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            newObjectives.Add(new Objective(list[i], i));
        }
        TacticManager.Instance.Markers.UpdateObjectives();
        if (!loading)
        {
            foreach (var objective in newObjectives)
            {
                objective.Start();
            }
        }
        UpdateRect();
    }

    public void UpdateRect()
    {
        ////ResizeObjectiveBar.Instance.RebuildContent(objectivesObjects);

        LayoutRebuilder.MarkLayoutForRebuild(mainRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainRect);
        ////rightPanelRect.sizeDelta = new Vector2(50f, mainRect.rect.height);
    }

    public void UpdateNumeration()
    {
        for (int i = 0, j = 0; i < newObjectives.Count; i++)
        {
            var data = newObjectives[i];
            if (data.Visible)
            {
                j++;
            }
            objectivesObjects[i].SetObjectiveNumber(j, data.Visible && !data.Finished, j.ToString());
        }
    }

    public ObjectiveObject GetObjectiveObject(int id)
    {
        return objectivesObjects[id];
    }

    public void AddObjective(string title, string description, List<int> enemyIDs, List<Vector2> poses, bool uoObjectives, string[] param)
    {
        var spritesData = new List<ObjectiveSpriteData>();
        for (int i = 0; i < enemyIDs.Count; i++)
        {
            var spriteData = new ObjectiveSpriteData();
            if (enemyIDs[i] > -1)
            {
                spriteData.Ship = TacticManager.Instance.GetShip(enemyIDs[i]);
                spriteData.Parent = spriteData.Ship.RectTransform;
                spriteData.Pos = Vector2.zero;
            }
            else
            {
                spriteData.Parent = TacticManager.Instance.Map.ObjectivesParent;
                spriteData.Pos = poses[i];
            }
            spritesData.Add(spriteData);
        }
        if (uoObjectives)
        {
            var data = new ObjectiveSpriteData();
            data.Pos = new Vector2(-3f, 0f);
            data.UO = true;
            spritesData.Add(data);
        }

        objectivesObjects.Add(Instantiate(objectivePrefab, mainRect).GetComponent<ObjectiveObject>().Setup(title, description, spritesData, param));
    }

    public Objective GetObjective(int id)
    {
        return newObjectives[id];
    }

    public void SetShowObjective(int id, bool show)
    {
        objectivesObjects[id].gameObject.SetActive(show);
        UpdateRect();
        UpdateNumeration();

        ObjectiveChanged(show);
    }

    public void FinishObjective(int id, bool success)
    {
        if (CheckFinished(id, out _))
        {
            Debug.LogError("Objective " + id + " - " + objectivesObjects[id].Title + " is already finished");
            return;
        }
        if (newObjectives[id].Data.InverseComplete)
        {
            success = !success;
        }
        ObjectiveFinishing(id, success);
        finished[id] = success;
        objectivesObjects[id].Finish(success);
        newObjectives[id].Finish(success);

        UpdateNumeration();

        ObjectiveFinished(id, success);
    }

    public bool CheckFinished(int id, out bool success)
    {
        return finished.TryGetValue(id, out success);
    }

    public void UnlockObjective(int ID)
    {
        if (objectives.Count > ID && objectives[ID].Hidden)
        {
            objectives[ID].Hidden = false;
            objectivesObjects[ID].gameObject.SetActive(true);
            objectivesObjects[ID].transform.SetAsLastSibling();

            foreach (var step in objectives[ID].Steps)
            {
                if (!step.Hidden && step.ObjectiveSprite != null)
                {
                    step.ObjectiveSprite.SetActive(true);
                }
            }

            UpdateRect();
            UpdateNumeration();
        }
    }

    public void UnlockObjectiveStep(int ID, int step, bool setAsLastSibling)
    {
        if (objectives.Count > ID && objectives[ID].Steps.Count > step && objectives[ID].Steps[step].Hidden)
        {
            var stepData = objectives[ID].Steps[step];
            stepData.Hidden = false;
            objectivesObjects[ID].UnlockStep(step, setAsLastSibling);

            if (stepData.ObjectiveSprite != null)
            {
                stepData.ObjectiveSprite.SetActive(true);
            }
        }
    }

    public void HideObjective(int ID)
    {
        if (objectives.Count > ID && !objectives[ID].Hidden)
        {
            objectives[ID].Hidden = false;
            objectivesObjects[ID].gameObject.SetActive(false);


            foreach (var step in objectives[ID].Steps)
            {
                if (step.ObjectiveSprite != null)
                {
                    step.ObjectiveSprite.SetActive(false);
                }
            }

            UpdateRect();
            UpdateNumeration();
        }
    }

    public void HideObjectiveStep(int ID, int step)
    {
        if (objectives.Count > ID && objectives[ID].Steps.Count > step && !objectives[ID].Steps[step].Hidden)
        {
            var stepData = objectives[ID].Steps[step];
            stepData.Hidden = true;
            objectivesObjects[ID].HideStep(step);

            if (stepData.ObjectiveSprite != null)
            {
                stepData.ObjectiveSprite.SetActive(false);
            }

            UpdateRect();
        }
    }

    public void SetStepState(int objectiveID, int step, bool completed)
    {
        if (objectiveID == 1)
        {
            Ignore = true;
        }

        if (objectives.Count > objectiveID && objectives[objectiveID].Steps.Count > step && !objectives[objectiveID].Steps[step].Hidden)
        {
            objectivesObjects[objectiveID].SetStepState(step, completed);
            UpdateRect();
            if (completed)
            {
                objectiveCompletePopup.SetActive(false);
                objectiveCompletePopup.SetActive(true);
            }
            else
            {
                objectiveFailedPopup.SetActive(false);
                objectiveFailedPopup.SetActive(true);
            }
            if (objectives[objectiveID].Steps[step].ObjectiveSprite != null)
            {
                objectives[objectiveID].Steps[step].ObjectiveSprite.SetActive(false);
            }
        }
    }

    public void SetInteractable(bool interactable)
    {
        foreach (var objective in objectivesObjects)
        {
            foreach (var data in objective.TacticalMapImages)
            {
                data.RaycastTargetMap.raycastTarget = interactable;
                data.RaycastTargetObject.raycastTarget = interactable;
            }
        }
    }

    public void ToggleObjectives()
    {
        SetShowObjectivesPanel(!panel.activeSelf);
    }

    public void SetShowObjectivesPanel(bool show)
    {
        show = show && !disable;
        if (show)
        {
            HudManager.Instance.PopupShown(this);
            ObjectivesOpened();
        }
        else
        {
            HudManager.Instance.PopupHidden(this);
        }
        panel.SetActive(show);
    }

    public void Hide()
    {
        SetShowObjectivesPanel(false);
    }

    public void SetObjectiveText(int id, string title, string description, int paramA, int paramB)
    {
        newObjectives[id].SetText(title, description, paramA, paramB);
        objectivesObjects[id].SetText(title, description, newObjectives[id].Data.Params);
    }

    public string GetTitle(int id)
    {
        return objectivesObjects[id].GetTitle();
    }

    public ObjectiveUISpriteData GetMagicSprite(RectTransform parent)
    {
        for (int i = 0; i < newObjectives.Count; i++)
        {
            var obj = objectivesObjects[i];
            if (obj.UOMapImagePrefab != null)
            {
                var img = Instantiate(obj.UOMapImagePrefab, parent);
                img.GetComponent<TacticalMapTooltipCaller>().Localized = true;
                obj.UOMapImagePrefab.gameObject.SetActive(true);
                img.Parent = obj.UOMapImagePrefab;

                obj.UOMapImages.Add(img);
                return img;
            }
        }
        return null;
    }

    public void ReleaseMagicSprite(ObjectiveUISpriteData data)
    {
        foreach (var obj in objectivesObjects)
        {
            if (obj.UOMapImagePrefab == data.Parent)
            {
                obj.UOMapImages.Remove(data);
                Destroy(data.gameObject);
                return;
            }
        }
    }

    public void WriteFinishedObjectives()
    {
        foreach (var obj in newObjectives)
        {
            if (obj.Finished)
            {
                Debug.Log($"Finished - {obj.Data.Name}");
            }
        }
    }
}
