
using MyBox;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class Gun : MonoBehaviour
{
    public Item Item
    {
        get
        {
            if (_item == null)
                _item = GetComponent<Item>();
            return _item;
        }
    }
    private Item _item;
    public GunSlide GunSlide
    {
        get
        {
            if (_gunSlide == null)
                _gunSlide = GetComponentInChildren<GunSlide>();
            return _gunSlide;
        }
    }
    private GunSlide _gunSlide;
    public Animator Anim { get { return Item.Animator; } }

    [Foldout("Shooting", true)]
    public Transform Muzzle;
    public FireMode FireMode = FireMode.Semi;
    [PositiveValueOnly]
    public float MaxRPM = 300f;
    public bool BulletInChamber = false;
    public int MagazineBullets = 0;
    [PositiveValueOnly]
    public int MagazineCapacity = 30;

    [Foldout("ADS", true)]
    public bool ADS = false;
    [PositiveValueOnly]
    public float ADSTime = 0.3f;
    public AnimationCurve ADSCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Foldout("Bullet Casings", true)]
    public Transform CasingSpawnPoint;
    public bool SpawnCasingOnShoot = false;
    [MinMaxRange(-1f, 5f)]
    public RangedFloat CasingVerticalVel;
    [MinMaxRange(-1f, 5f)]
    public RangedFloat CasingHorizontalVel;
    [MinMaxRange(-5f, 5f)]
    public RangedFloat CasingForwardVel;
    public Vector3 CasingAngularVel = new Vector3(1000f, 500f, 100f);
    [DisplayInspector]
    public BulletCasingData CasingData;

    [Foldout("Other effects", true)]
    public MuzzleFlash MuzzleFlashPrefab;

    public int TotalCurrentBullets { get { return MagazineBullets + (BulletInChamber ? 1 : 0); } }
    public bool IsReloading { get; protected set; }
    public bool CanADS { get { return (Anim.GetCurrentAnimatorStateInfo(0).IsTag("ADS") && !Anim.IsInTransition(0)) && todoAfterUnADS.Count == 0; } }

    public float ADSLerp { get; private set; }
    public bool IsInADS { get { return ADSLerp > 0f; } }

    private Queue<System.Action> todoAfterUnADS = new Queue<System.Action>();
    private float fireTimer;
    private bool shootQueued = false;

    private void Start()
    {
        MagazineBullets = MagazineCapacity;
    }

    private void Update()
    {
        if (Item.State != ItemState.Active)
            return;

        UpdateInput();
        UpdateADS();

        Anim.SetBool("Empty", !BulletInChamber);

        // There is no bullet in chamber, but there are bullets in magazine.
        // This should cause the chamber animation to play.
        // However, the trigger also needs to be reset once chamber is triggered, since the empty reload animation
        // has a delay between reloading and chambering.
        if(!BulletInChamber && MagazineBullets > 0)
        {
            Anim.SetTrigger("Chamber");
        }
        else
        {
            Anim.ResetTrigger("Chamber");
        }

        var slide = GunSlide;
        if(slide != null)
        {
            // Override slide behaviour 
            slide.Override = !BulletInChamber;
            slide.OverrideLerp = 1f;
            slide.IsInTransition = Anim.IsInTransition(0);
        }
    }
    
    private void UpdateADS()
    {
        if (ADSTime < 0.05f)
            ADSTime = 0.05f;

        float time = ADSTime;

        if (ADS && CanADS)
        {
            if(ADSLerp < 1f)
                ADSLerp += Time.deltaTime * (1f / time);
        }
        else
        {
            if(ADSLerp > 0f)
                ADSLerp -= Time.deltaTime * (1f / time);
            if(ADSLerp < 0f)
            {
                // This means we just hit the 'un ADS'ed' state. We need to run any actions that are required.
                while(todoAfterUnADS.Count > 0)
                {
                    var action = todoAfterUnADS.Dequeue();
                    action?.Invoke();
                }
            }
        }
        ADSLerp = Mathf.Clamp01(ADSLerp);

        float finalForAnimation = Mathf.Clamp01(ADSCurve.Evaluate(ADSLerp));
        Anim.SetLayerWeight(1, finalForAnimation);
    }

    private void UpdateInput()
    {
        ADS = Input.GetKey(KeyCode.Mouse1);
        
        if (Input.GetKeyDown(KeyCode.R))
            TriggerReload();
        if (Input.GetKeyDown(KeyCode.Mouse3))
            TriggerMelee();
        if (Input.GetKeyDown(KeyCode.F))
            TriggerInspect();

        fireTimer += Time.deltaTime;
        float minInterval = 1f / (MaxRPM / 60f);

        switch (FireMode)
        {
            case FireMode.Semi:

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    shootQueued = true;
                }

                if(fireTimer >= minInterval && shootQueued)
                {
                    TriggerShoot();
                    fireTimer = 0f;
                    shootQueued = false;
                }
                break;
            case FireMode.Auto:
                if (Input.GetKey(KeyCode.Mouse0) && fireTimer >= minInterval)
                {
                    TriggerShoot();
                    fireTimer = 0f;
                }
                break;
        }
    }

    public void TriggerShoot()
    {
        if(BulletInChamber)
            Anim.SetTrigger("Shoot");
    }

    public void TriggerReload()
    {
        if (IsReloading)
        {
            Debug.LogWarning("Already reloading...");
            return;
        }

        // Don't reload if we already have max bullets.
        if (MagazineBullets >= MagazineCapacity)
            return;

        // This action is the code that actually needs to be run.
        var a = new System.Action(() =>
        {
            // Make sure that the empty state is updated.
            Anim.SetBool("Empty", !BulletInChamber);
            Anim.SetTrigger("Reload");
            IsReloading = true;
        });

        if (IsInADS)
        {
            // If we are aiming down sights, run this code after ads stops. Enqueueing this will automatically cause the gun to un-ads.
            todoAfterUnADS.Enqueue(a);
        }
        else
        {
            // If we are not aiming down sights, run this code immediately so that the animation triggers as fast as possible.
            a.Invoke();
        }
    }

    public void TriggerInspect()
    {
        if (!IsInADS)
            Anim.SetTrigger("Inspect");
        else
            todoAfterUnADS.Enqueue(() => { Anim.SetTrigger("Inspect"); });
    }

    public void TriggerMelee()
    {
        // Triggers immediately regardless of ads state. This just makes it a little more responsive, even if it doesn't look as smooth.
        Anim.SetTrigger("Melee");
    }

    public void SpawnCasing()
    {
        if (CasingSpawnPoint == null)
            return;

        if (CasingData == null)
            return;

        if (CasingData.Prefab == null)
            return;

        var spawned = PoolObject.Spawn(CasingData.Prefab);
        spawned.transform.position = CasingSpawnPoint.position;
        spawned.transform.forward = CasingSpawnPoint.forward;
        Vector3 localVel = new Vector3(CasingHorizontalVel.LerpFromRange(Random.value), CasingVerticalVel.LerpFromRange(Random.value), CasingForwardVel.LerpFromRange(Random.value));
        Vector3 worldVel = CasingSpawnPoint.TransformVector(localVel);
        spawned.Velocity = worldVel;
        spawned.AngularVelocity = CasingAngularVel;
        spawned.BounceVelocityMultiplier = CasingData.BounceCoefficient;
    }

    private void Rechamber()
    {
        if (MagazineBullets > 0)
        {
            MagazineBullets--;
            BulletInChamber = true;
        }
    }

    private void OnShoot()
    {
        if (!BulletInChamber)
        {
            Debug.LogWarning($"Gun {Item.Name} has no bullet in the chamber but the shoot animation played...");
            return;
        }

        // Remove the bullet from the chamber.
        BulletInChamber = false;

        // Auto re-chamber. For some weapons, such as bolt-action rifles, this is not desirable. This will be implemented later.
        Rechamber();

        // Spawn muzzle flash.
        if(Muzzle != null && MuzzleFlashPrefab != null)
        {
            var spawned = PoolObject.Spawn(MuzzleFlashPrefab);
            spawned.transform.SetParent(Muzzle);
            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = Quaternion.identity;
            spawned.transform.Rotate(0f, 0f, Random.Range(0f, 360f), Space.Self);
        }

        if (SpawnCasingOnShoot)
        {
            // Spawn bullet casing.
            SpawnCasing();
        }        
    }

    private void OnReload()
    {
        if (!IsReloading)
        {
            Debug.LogWarning($"Gun {Item.Name} ended reload animation, but IsReloading is false...");
            return;
        }

        // Fill that magazine back up.
        MagazineBullets = MagazineCapacity;

        IsReloading = false;
    }

    private void OnChamber()
    {
        if (MagazineBullets > 0 && !BulletInChamber)
            Rechamber();
        else
            Debug.LogWarning($"Gun {Item.Name} gave chamber callback from animation, but there are {MagazineBullets} bullets in magazine and there is {(BulletInChamber ? "" : "NOT ")}a bullet in the chamber.");
    }

    private void UponAnimationEvent(AnimationEvent e)
    {
        string s = e.stringParameter.ToLower().Trim();

        switch (s)
        {
            case "shoot":
                OnShoot();
                break;
            case "reload":
                OnReload();
                break;
            case "chamber":
                OnChamber();
                break;
            case "spawncasing":
            case "spawn casing":
            case "casing":
                SpawnCasing();
                break;
        }
    }

    private void OnDeactivate()
    {
        // Since the animator is left in an undefined state, reset all flags.
        IsReloading = false;
        ADSLerp = 0f;
    }
}

public enum FireMode
{
    Semi,
    Auto
}
