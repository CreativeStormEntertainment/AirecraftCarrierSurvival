using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.VFX;

public class EscortAnim : MonoBehaviour
{
    public List<VisualEffect> Smoke => smoke;
    public List<SmokeParticles> SmokeParticles => smokeParticles;

    [SerializeField]
    private PlayableDirector swimAnim = null;
    [SerializeField]
    private PlayableDirector idleAnim = null;
    [SerializeField]
    private List<VisualEffect> smoke = null;
    [SerializeField]
    private List<SmokeParticles> smokeParticles = null;
    [SerializeField]
    private List<Animator> animators = null;
    [SerializeField]
    private List<Transform> escortFoams = null;

    private ParticleSystem particle;

    private List<int> basket = new List<int>();
    private List<int> selectedShips = new List<int>();
    private List<float> selectedShipsTimers = new List<float>();
    private bool destroyed;
    private Vector3 moveBackVector;
    private float timer;

    private void Awake()
    {
        HudManager.Instance.ShipSpeedChanged += OnShipSpeedChanged;
        for (int i = 0; i < smoke.Count; i++)
        {
            basket.Add(i);
        }
        //CameraManager.Instance.WaterAndWorldCamera.EscortFoams.AddRange(escortFoams);
    }

    private void Update()
    {
        if (HudManager.Instance.ShipSpeedup <= 0f || !TacticManager.Instance.Carrier.HasWaypoint)
        {
            SetAnim(swimAnim, idleAnim);
        }
        else
        {
            SetAnim(idleAnim, swimAnim);
        }
        if (destroyed)
        {
            if (basket.Count > 0)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    StartFire();
                }
            }
            for (int i = 0; i < selectedShips.Count; i++)
            {
                animators[selectedShips[i]].transform.position += moveBackVector * Time.deltaTime;
                if (selectedShipsTimers[i] > 0)
                {
                    selectedShipsTimers[i] -= Time.deltaTime;
                    if (selectedShipsTimers[i] <= 0)
                    {
                        animators[selectedShips[i]].enabled = true;
                        smoke[selectedShips[i]].SetFloat("WaterScale", 0f);
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {

    }

    public void ToggleAntiAir(bool active)
    {
        if (smoke != null)
        {
            foreach (var s in smoke)
            {
                if (s != null && s.GetVector3("AALocalPos") != Vector3.zero)
                {
                    s.SetBool("ActivateAA", active);
                }
            }
        }
    }

    public void StartFire()
    {
        destroyed = true;
        var rand = RandomUtils.GetRandom(basket);
        basket.Remove(rand);
        selectedShips.Add(rand);
        selectedShipsTimers.Add(Random.Range(20f, 80f));
        string id = "ActivateFire";
        smoke[rand].SetBool(id + Random.Range(0, 4), true);
    }

    private void SetAnim(PlayableDirector disableAnim, PlayableDirector enableAnim)
    {
        if (!enableAnim.gameObject.activeSelf)
        {
            if (disableAnim.time <= .5d || (disableAnim.time >= (disableAnim.duration - .5d)))
            {
                disableAnim.gameObject.SetActive(false);
                enableAnim.gameObject.SetActive(true);
            }
        }
    }

    private void OnShipSpeedChanged(int speed)
    {
        var hudMan = HudManager.Instance;
        moveBackVector = new Vector3(0f, 0f, hudMan.ShipSpeedup * hudMan.EscortSpeedMultiplier);
    }
}
