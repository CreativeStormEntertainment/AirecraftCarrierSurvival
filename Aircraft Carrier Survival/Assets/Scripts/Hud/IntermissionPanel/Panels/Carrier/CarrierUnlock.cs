using System;
using UnityEngine;
using UnityEngine.UI;

public class CarrierUnlock : MonoBehaviour
{
    public event Action CarrierChanged = delegate { };

    [SerializeField]
    private Text elevators = null;
    [SerializeField]
    private Text squadronsSlots = null;
    [SerializeField]
    private Text sections = null;
    [SerializeField]
    private Text islandRooms = null;

    [SerializeField]
    private GameObject radar = null;
    [SerializeField]
    private GameObject hangar = null;
    [SerializeField]
    private GameObject aa = null;
    [SerializeField]
    private GameObject crew = null;
    [SerializeField]
    private GameObject officers = null;

    [SerializeField]
    private Button unlockButton = null;
    [SerializeField]
    private BuyButton buyButton = null;
    [SerializeField]
    private Button selectButton = null;

    [SerializeField]
    private GameObject unlocks = null;

    [SerializeField]
    private GameObject blink = null;

    [SerializeField]
    private float blinkTime = 0.5f;

    private CarrierUpgradeData data;
    private float currentTime = 0f;
    private int carrier;
    private void Update()
    {
        currentTime = (currentTime + Time.deltaTime / blinkTime) % 1f;
        blink.SetActive(currentTime > 0.5f);
    }

    public void Init(bool buy)
    {
        unlockButton.gameObject.SetActive(!buy);
        buyButton.gameObject.SetActive(buy);

        buyButton.Setup(false, 1, OnUnlock);

        unlockButton.onClick.AddListener(OnUnlock);
        selectButton.onClick.AddListener(() => CarrierChanged());

        SetBlink(false);
        gameObject.SetActive(false);
    }

    public void Setup(CarrierUpgradeData data, bool unlocked, int carrier)
    {
        this.data = data;
        this.carrier = carrier;
        elevators.text = data.Elevators.ToString();
        squadronsSlots.text = data.SquadronSlots.ToString();
        sections.text = data.Sections.ToString();
        islandRooms.text = data.IslandRooms.ToString();

        if (unlocked)
        {
            unlocks.SetActive(false);
            selectButton.gameObject.SetActive(true);
            selectButton.interactable = (int)IntermissionManager.Instance.CurrentCarrier != carrier;
        }
        else
        {
            unlockButton.interactable = data.Active;
            buyButton.SetUpgrade(0, data.Price, true);

            unlocks.SetActive(true);
            selectButton.gameObject.SetActive(false);
        }

        radar.SetActive(data.Radar < 0);
        hangar.SetActive(data.Hangar < 0);
        aa.SetActive(data.AA < 0);
        crew.SetActive(data.Crew < 0);
        officers.SetActive(data.Officer < 0);
        SetBlink(unlockButton.interactable);

        gameObject.SetActive(true);
    }

    public void SetBlink(bool blink)
    {
        enabled = blink;
        this.blink.SetActive(false);
        currentTime = 0f;
    }

    private void OnUnlock()
    {
        SaveManager.Instance.Data.IntermissionData.AvailableCarriers |= (1 << carrier);
        IntermissionManager.Instance.FireCarrierBought((ECarrierType)carrier);
        Setup(data, true, carrier);
    }
}
