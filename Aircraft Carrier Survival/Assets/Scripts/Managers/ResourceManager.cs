using GambitUtils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour, ITickable, IEnableable
{
    public event Action<float> SuppliesAmountChanged = delegate { };
    public event Action<float> CrewStatusChanged = delegate { };

    public static ResourceManager Instance;

    private static readonly int IdleAnimHash = Animator.StringToHash("Idle");

    private static readonly int StartAnimHash = Animator.StringToHash("Start");
    private static readonly int EndAnimHash = Animator.StringToHash("End");
    private static readonly int SupplyAnimHash = Animator.StringToHash("Supply");

    private const string StartAnim = "Start";
    private const string EndAnim = "End";
    private const string SupplyAnim = "Supply";

    public float Supplies
    {
        get => supplies;
        private set
        {
            supplies = value;
            SuppliesAmountChanged(Mathf.Clamp01(supplies / (float)SuppliesCapacity));
        }
    }

    public bool IsRefilling
    {
        get;
        private set;
    }

    public int SuppliesCapacity
    {
        get;
        private set;
    }

    public float RessuplySpeedModifier
    {
        get;
        private set;
    } = 1f;

    [NonSerialized]
    public float RefillSpeedup = 1f;

    [SerializeField]
    private int baseSuppliesCapacity = 12000;
    [SerializeField]
    private int startingSupplies = 8000;
    [SerializeField]
    private float suppliesConsume = 10f;
    [SerializeField]
    private float maxMoreSuppliesToConsume = 2.222f;
    [SerializeField]
    private int ticksToRefill = 40;
    [SerializeField]
    private int lowSupplies = 2500;
    [SerializeField]
    private ResupplySounds resupplySounds = null;

    [SerializeField]
    private List<Animator> supplyCargoAnims;
    [SerializeField]
    private List<Animator> supplyFuelAnims;
    [SerializeField]
    private List<GameObject> cargoLines1;
    [SerializeField]
    private List<GameObject> cargoLines2;
    [SerializeField]
    private Animator supplyShip;

    private float refillCounter = 0f;
    private int suppliesRefillValue = 5000;
    private int resupplyAnimDelay = 10;
    private float supplies = 0;

    private Dictionary<int, AnimationClip> supplyShipAnimDict;
    private Dictionary<int, AnimationClip> supplyCargoAnimDict;

    private bool disable;

    private void Awake()
    {
        Instance = this;

        supplyShipAnimDict = new Dictionary<int, AnimationClip>();
        foreach (var anim in supplyShip.runtimeAnimatorController.animationClips)
        {
            if (anim.name == StartAnim)
            {
                supplyShipAnimDict.Add(StartAnimHash, anim);
            }
            else if (anim.name == EndAnim)
            {
                supplyShipAnimDict.Add(EndAnimHash, anim);
            }
        }

        supplyCargoAnimDict = new Dictionary<int, AnimationClip>();
        foreach (var anim in supplyCargoAnims[0].runtimeAnimatorController.animationClips)
        {
            if (anim.name == StartAnim)
            {
                supplyCargoAnimDict.Add(StartAnimHash, anim);
            }
            else if (anim.name == EndAnim)
            {
                supplyCargoAnimDict.Add(EndAnimHash, anim);
            }
            else if (anim.name == SupplyAnim)
            {
                supplyCargoAnimDict.Add(SupplyAnimHash, anim);
            }
        }
    }

    private void Start()
    {
        WorldMap.Instance.Toggled += OnWorldMapToggled;
    }

    public void SetEnable(bool enable)
    {
        disable = !enable;
    }

    public void Setup()
    {
        TimeManager.Instance.AddTickable(this);
        SuppliesCapacity = baseSuppliesCapacity;
        Supplies = startingSupplies;
    }

    public void LoadData(ref SuppliesSaveData data)
    {
        Supplies = data.Supplies;
        if (data.TimePassedRefilling >= 0f)
        {
            StartAnimation(data.TimePassedRefilling);
        }
        else
        {
            StopAnim();
        }
    }

    public void SaveData(ref SuppliesSaveData data)
    {
        data.Supplies = Supplies;
        data.TimePassedRefilling = IsRefilling ? refillCounter : -1f;
    }

    public void Tick()
    {
        if (IsRefilling)
        {
            refillCounter += RefillSpeedup * RessuplySpeedModifier;
            if (refillCounter >= (ticksToRefill + resupplyAnimDelay))
            {
                refillCounter = 0f;
                IsRefilling = false;
                Supplies = SuppliesCapacity;
                PlaySupplyEndAnim();
            }
        }
        else
        {
            if (HudManager.Instance.InWorldMap)
            {
                return;
            }
            float oldSupplies = Supplies;
            if (HudManager.Instance.HasNo(ETutorialMode.DisableUsingSupplies) && !disable)
            {
                Supplies -= suppliesConsume + maxMoreSuppliesToConsume;
            }

            if (Supplies <= Mathf.Epsilon)
            {
                Supplies = 0f;
                if (oldSupplies > Mathf.Epsilon)
                {
                    EventManager.Instance.NoSuppliesPopup();
                }
            }
            else if (Supplies <= lowSupplies && oldSupplies >= lowSupplies)
            {
                EventManager.Instance.LowSuppliesPopup();
            }
        }
    }

    public void ChangeSuppliesAmount(int suppliesModifier)
    {
        Supplies = SuppliesCapacity * suppliesModifier / 100f;
    }

    public void RefillSupplies()
    {
        if (!IsRefilling)
        {
            StartAnimation(0f);
        }
    }

    public bool CanMoveShip()
    {
        return !IsRefilling;
    }

    public void SetMaxSuppliesBonus(float percent)
    {
        SuppliesCapacity = baseSuppliesCapacity + Mathf.RoundToInt(baseSuppliesCapacity * percent);
        Supplies = Supplies;
    }

    public void SetResupplySpeedModifier(float percent)
    {
        RessuplySpeedModifier = 1f + percent;
    }

    public void StartAnimation(float time)
    {
        supplyShip.enabled = true;
        IsRefilling = true;
        var currentTime = refillCounter = time * RessuplySpeedModifier;
        supplyShip.gameObject.SetActive(true);

        if (currentTime < supplyShipAnimDict[StartAnimHash].length)
        {
            resupplySounds.PlayEvent(EResupplySound.Start, TimeManager.Instance.CurrentSpeed);
            resupplySounds.SetTimelinePosition((int)currentTime);
            supplyShip.Play(StartAnimHash, 0, time / supplyShipAnimDict[StartAnimHash].length);
        }
        else if (currentTime < supplyShipAnimDict[StartAnimHash].length + supplyCargoAnimDict[StartAnimHash].length + supplyCargoAnimDict[SupplyAnimHash].length + supplyCargoAnimDict[EndAnimHash].length)
        {
            supplyShip.Play(StartAnimHash, 0, 1f);
            currentTime -= supplyShipAnimDict[StartAnimHash].length;
            if (currentTime < supplyCargoAnimDict[StartAnimHash].length)
            {
                foreach (var anim in supplyCargoAnims)
                {
                    anim.Play(StartAnimHash, 0, currentTime / supplyCargoAnimDict[StartAnimHash].length);
                }
            }
            else if (currentTime < supplyCargoAnimDict[StartAnimHash].length + supplyCargoAnimDict[SupplyAnimHash].length)
            {
                currentTime -= supplyCargoAnimDict[StartAnimHash].length;
                foreach (var anim in supplyCargoAnims)
                {
                    anim.Play(SupplyAnimHash, 0, currentTime / supplyCargoAnimDict[SupplyAnimHash].length);
                }
            }
            else
            {
                currentTime -= supplyCargoAnimDict[StartAnimHash].length + supplyCargoAnimDict[SupplyAnimHash].length;
                foreach (var anim in supplyCargoAnims)
                {
                    anim.Play(SupplyAnimHash, 0, currentTime / supplyCargoAnimDict[EndAnimHash].length);
                }
            }
            resupplySounds.PlayEvent(EResupplySound.Loop, TimeManager.Instance.CurrentSpeed);
            resupplySounds.SetTimelinePosition((int)currentTime);
        }
        else
        {
            currentTime -= supplyShipAnimDict[StartAnimHash].length + supplyCargoAnimDict[StartAnimHash].length + supplyCargoAnimDict[SupplyAnimHash].length + supplyCargoAnimDict[EndAnimHash].length;
            supplyShip.Play(EndAnimHash, 0, currentTime / supplyShipAnimDict[EndAnimHash].length);
            resupplySounds.PlayEvent(EResupplySound.End, TimeManager.Instance.CurrentSpeed);
            resupplySounds.SetTimelinePosition((int)(refillCounter - supplyShipAnimDict[StartAnimHash].length));
        }
    }

    public void SwitchObjectsState(bool value)
    {
        for (int i = 0; i < supplyCargoAnims.Count; i++)
        {
            cargoLines1[i].SetActive(value);
            cargoLines2[i].SetActive(!value);
        }
    }

    public void PlaySupplyStartAnim()
    {
        for (int i = 0; i < supplyCargoAnims.Count; i++)
        {
            supplyCargoAnims[i].Play(StartAnimHash);
        }
        resupplySounds.SetParameter(EResupplySound.Loop);
    }

    public void PlaySupplyAnim()
    {
        foreach (var anim in supplyCargoAnims)
        {
            anim.Play(SupplyAnimHash);
        }
    }

    public void PlaySupplyEndAnim()
    {
        SwitchObjectsState(true);
        foreach (var anim in supplyCargoAnims)
        {
            anim.Play(EndAnimHash);
        }
        resupplySounds.SetParameter(EResupplySound.End);
    }

    public void PlayEndAnim()
    {
        supplyShip.Play(EndAnimHash);
    }

    public void SetSupplies(float supplies)
    {
        Supplies = supplies;
        if (Supplies <= Mathf.Epsilon)
        {
            Supplies = 0f;
            EventManager.Instance.NoSuppliesPopup();
        }
        else if (Supplies <= lowSupplies)
        {
            EventManager.Instance.LowSuppliesPopup();
        }
    }

    private void OnWorldMapToggled(bool state)
    {
        if (state)
        {
            InstantRefillSupplies();
        }
    }

    private void InstantRefillSupplies()
    {
        Supplies = SuppliesCapacity;
        StopAnim();
    }

    private void StopAnim()
    {
        supplyShip.Play(StartAnimHash, 0, 0f);
        this.StartCoroutineActionAfterFrames(() => supplyShip.Play(IdleAnimHash, 0, 0f), 1);
        refillCounter = 0f;
        IsRefilling = false;
    }
}