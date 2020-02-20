
using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

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
    public Animator Anim { get { return Item.Animation.Animator; } }    

    [Foldout("Shooting", true)]
    public Transform Muzzle;
    public Projectile BulletPrefab;
    public FireMode FireMode = FireMode.Semi;
    [MinMaxRange(0f, 360f)]
    public MinMaxFloat BulletRandomAngle = new MinMaxFloat(0f, 0f);
    [Range(1f, 64f)]
    public int BulletsPerShot = 1;
    [PositiveValueOnly]
    public float MaxRPM = 300f;
    public bool BulletInChamber = false;
    public int MagazineBullets = 0;
    [PositiveValueOnly]
    public int MagazineCapacity = 30;

    [Foldout("Reloading", true)]
    [Tooltip("Does this gun use a shotgun-like reload? (Loading individual shots)")]
    public bool ShotgunReload = false;
    [Tooltip("If shotgun-like reload is enabled, the is the user allowed to interrupt the animation before all shells/bullets are loaded? Recommended true for consistency, only disable for a valid gameplay to technical reason.")]
    public bool CanInterruptShotgunReload = true;

    [Foldout("Recoil", true)]
    [MyBox.Separator("Kick (Gun)")]
    public Vector2 HorizontalKick = new Vector2(-25f, 50f);
    public Vector2 VerticalKick = new Vector2(40f, 80f);
    public float KickFalloff = 0.75f;
    [MyBox.Separator("Recoil (Camera)")]
    public Vector2 HorizontalRecoil = new Vector2(-50f, 50f);
    public Vector2 VerticalRecoil = new Vector2(40f, 50f);

    [Foldout("ADS", true)]
    public bool ADS = false;
    [PositiveValueOnly]
    public float ADSTime = 0.3f;
    public AnimationCurve ADSCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float ADS_FOV_Multiplier = 0.8f;

    [Foldout("Bullet Casings", true)]
    public Transform CasingSpawnPoint;
    public bool SpawnCasingOnShoot = false;
    [MinMaxRange(-1f, 5f)]
    public RangedFloat CasingVerticalVel;
    [MinMaxRange(-1f, 5f)]
    public RangedFloat CasingHorizontalVel;
    [MinMaxRange(-5f, 5f)]
    public RangedFloat CasingForwardVel;
    public Vector3 CasingAngularVel = new Vector3(6000f, 500f, 100f);
    [DisplayInspector]
    public BulletCasingData CasingData;

    [Foldout("Other effects", true)]
    public MuzzleFlash MuzzleFlashPrefab;

    public int TotalCurrentBullets { get { return MagazineBullets + (BulletInChamber ? 1 : 0); } }
    public bool IsReloading { get; protected set; }
    public bool CanADS { get { return (Anim.GetCurrentAnimatorStateInfo(0).IsTag("ADS") || Anim.GetCurrentAnimatorStateInfo(0).IsTag("ADS Run")) && !Anim.IsInTransition(0) && Item.Animation.PendingActionCount == 0; } }
    public bool CanRun { get { return !IsInADS && (Anim.GetCurrentAnimatorStateInfo(0).IsTag("Run") || Anim.GetCurrentAnimatorStateInfo(0).IsTag("ADS Run")) && !Anim.IsInTransition(0) && Item.Animation.PendingActionCount == 0; } }

    public float ADSLerp { get; private set; }
    public bool IsInADS { get { return ADSLerp > 0f; } }
    public float RunLerp { get; private set; }
    public bool IsInRun { get { return RunLerp > 0f; } }

    private float fireTimer;
    private bool shootQueued = false;
    private bool shootPressed = false;

    private void Awake()
    {
        SetupInput();        
    }

    private void Start()
    {
        MagazineBullets = MagazineCapacity;
    }

    private void SetupInput()
    {
        var input = Player.Input;
        input.actions["Aim"].started += ctx => ADS = true;
        input.actions["Aim"].canceled += ctx => ADS = false;
        input.actions["Shoot"].started += ctx =>
        {
            if(Item.State == ItemState.Active)
            {
                shootPressed = true;
                shootQueued = true;

                // Interrupt recursive reload on shotguns.
                TriggerReloadInterrupt();
            }            
        };
        input.actions["Shoot"].canceled += ctx => shootPressed = false;

        input.actions["Reload"].performed += ctx =>
        {
            if (Item.State != ItemState.Active)
                return;

            TriggerReload();
        };
        input.actions["Melee"].performed += ctx =>
        {
            if (Item.State != ItemState.Active)
                return;

            TriggerMelee();
        };
        input.actions["Inspect"].performed += ctx =>
        {
            if (Item.State != ItemState.Active)
                return;

            TriggerInspect();
        };
    }

    private void Update()
    {
        if (Item.State != ItemState.Active)
            return;

        UpdateShooting();
        UpdateADS();
        UpdateRun();

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
        }
        ADSLerp = Mathf.Clamp01(ADSLerp);

        float finalForAnimation = Mathf.Clamp01(ADSCurve.Evaluate(ADSLerp));
        Anim.SetLayerWeight(1, finalForAnimation);
    }

    private void UpdateRun()
    {
        bool run = Player.Instance.Movement.IsRunning;
        const float RUN_LERP_TIME = 0.25f;
        const float RUN_LERP_SPEED = 1f / RUN_LERP_TIME;

        if(run && CanRun)
        {
            RunLerp += Time.deltaTime * RUN_LERP_SPEED;
        }
        else
        {
            RunLerp -= Time.deltaTime * RUN_LERP_SPEED;
        }
        RunLerp = Mathf.Clamp01(RunLerp);
        Anim.SetLayerWeight(2, RunLerp);
    }

    private void UpdateShooting()
    {
        if (FireMode != FireMode.Semi)
            shootQueued = false;

        fireTimer += Time.deltaTime;
        float minInterval = 1f / (MaxRPM / 60f);

        switch (FireMode)
        {
            case FireMode.Semi:

                if(fireTimer >= minInterval && shootQueued)
                {
                    if (TriggerShoot())
                    {
                        fireTimer = 0f;
                    }
                    shootQueued = false;
                }
                break;
            case FireMode.Auto:
                if (shootPressed && fireTimer >= minInterval)
                {
                    if (TriggerShoot())
                    {
                        fireTimer = 0f;
                    }
                }
                break;
        }
    }

    public bool TriggerShoot()
    {
        bool canShoot = CanShoot();
        if (canShoot)
        {
            Anim.SetTrigger("Shoot");
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanShoot()
    {
        if (!BulletInChamber)
            return false;

        if (IsReloading)
            return false;

        return true;
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

            // Set trigger or bool based on animator type.
            if (ShotgunReload)
                Anim.SetBool("Reload", true);
            else
                Anim.SetTrigger("Reload");

            IsReloading = true;
        });

        if (IsInADS || IsInRun)
        {
            // If we are aiming down sights, run this code after ads stops. Enqueueing this will automatically cause the gun to un-ads.
            EnqueueOnceRegular(a);
        }
        else
        {
            // If we are not aiming down sights, run this code immediately so that the animation triggers as fast as possible.
            a.Invoke();
        }
    }

    private void EnqueueOnceRegular(System.Action a)
    {
        Item.Animation.AddPendingAction(new ItemAnimator.PendingAction()
        {
            Action = a,
            LayerIndex = new int[] { 1, 2 },
            LayerWeight = new float[] { 0, 0 },
            ComparisonType = ItemAnimator.ComparisonType.LessOrEqual
        });
    }

    /// <summary>
    /// Interrupt shotgun-like reloads. Has no effect on regular guns that have 'simple' reload animations.
    /// </summary>
    public void TriggerReloadInterrupt()
    {
        if (!ShotgunReload)
            return;

        Anim.SetBool("Reload", false);
    }

    public void TriggerInspect()
    {
        if (!IsInADS && !IsInRun)
            Anim.SetTrigger("Inspect");
        else
            EnqueueOnceRegular(() => { Anim.SetTrigger("Inspect"); });
    }

    public void TriggerMelee()
    {
        // Triggers immediately regardless of ads state. This just makes it a little more responsive, even if it doesn't look as smooth.
        Anim.SetTrigger("Melee");

        // If reloading, we aren't any more.
        if (IsReloading)
        {
            IsReloading = false;

            // Cancel shotgun reload or risk bugging out the weapon.
            TriggerReloadInterrupt();
        }
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

    public bool ShootNow(bool immediate = false)
    {
        if (!BulletInChamber)
        {
            Debug.LogWarning($"Gun {Item.Name} has no bullet in the chamber but the shoot animation played...");
            return false;
        }

        if (immediate)
        {
            return InternalShootRegular();
        }
        else
        {
            var method = InternalShootCoroutine();
            StartCoroutine(method);
        }        

        return true;
    }

    public void AddRecoil()
    {
        Vector3 angles = new Vector3();
        angles.x = Mathf.Lerp(VerticalKick.x, VerticalKick.y, Random.value);
        angles.y = Mathf.Lerp(HorizontalKick.x, HorizontalKick.y, Random.value);

        ItemMotionController.Instance.AddPunch(angles, KickFalloff);

        Vector2 recoil = new Vector2();
        recoil.x = Mathf.Lerp(HorizontalRecoil.x, HorizontalRecoil.y, Random.value);
        recoil.y = Mathf.Lerp(VerticalRecoil.x, VerticalRecoil.y, Random.value);

        CameraLook.Instance.AddRecoil(recoil);
    }

    private IEnumerator InternalShootCoroutine()
    {
        yield return new WaitForEndOfFrame();

        InternalShootRegular();

        yield return true;
    }

    private bool InternalShootRegular()
    {
        if (!BulletInChamber)
        {
            Debug.LogWarning($"Gun {Item.Name} has no bullet in the chamber but the shoot animation played...");
            return false;
        }

        // Remove the bullet from the chamber.
        BulletInChamber = false;

        // Spawn the bullet.
        if (BulletPrefab != null && Muzzle != null)
        {
            for (int i = 0; i < BulletsPerShot; i++)
            {
                var spawned = PoolObject.Spawn(BulletPrefab);
                spawned.transform.position = Muzzle.position;

                // Velocity is calculated based on random angle - most rifles have zero random angle.
                // Note that velocity magnitude is always maintained.
                const float MUZZLE_VELOCITY = 350f;
                if(BulletRandomAngle.Min == 0f && BulletRandomAngle.Max == 0)
                {
                    spawned.Velocity = Muzzle.forward * MUZZLE_VELOCITY;
                }
                else
                {
                    float angle = BulletRandomAngle.Lerp(Random.value);
                    spawned.Velocity = GenerateConeDirection(angle, Muzzle) * MUZZLE_VELOCITY;
                    Debug.DrawLine(Muzzle.position, Muzzle.position + spawned.Velocity.normalized, Color.red, 10f);
                }
            }            
        }

        // Auto re-chamber. For some weapons, such as bolt-action rifles, this is not desirable. This will be implemented later.
        Rechamber();

        // Spawn muzzle flash.
        if (Muzzle != null && MuzzleFlashPrefab != null)
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

        // Add kick and recoil.
        AddRecoil();

        return true;
    }

    public static Vector3 GenerateConeDirection(float angleDeg, Transform forwards)
    {
        angleDeg = Mathf.Clamp(angleDeg, 0f, 179.5f);
        if (angleDeg <= 0.01f)
            return forwards.forward;

        const float DST = 1f;
        float r = Mathf.Tan((angleDeg * 0.5f) * Mathf.Deg2Rad) / DST;
        Vector2 onCircle = Random.insideUnitCircle.normalized * r;

        Vector3 localSpace = new Vector3(onCircle.x, onCircle.y, DST);
        Vector3 worldSpace = forwards.TransformVector(localSpace);

        //Debug.Log($"Angle: {angleDeg * 0.5f}, r: {r}");

        return worldSpace.normalized;
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
        ShootNow();
    }

    private void OnReload(int shells)
    {
        if (!IsReloading)
        {
            Debug.LogWarning($"Gun {Item.Name} ended reload animation, but IsReloading is false...");
            return;
        }

        if (!ShotgunReload)
        {
            // Regular reload, just refil the magazine.

            // Fill that magazine back up.
            MagazineBullets = MagazineCapacity;

            // Reload is finished.
            IsReloading = false;
        }
        else
        {
            // Shotgun-like reload.
            MagazineBullets += shells;
            MagazineBullets = Mathf.Clamp(MagazineBullets, 0, MagazineCapacity);

            // If full, stop reloading.
            if (MagazineBullets == MagazineCapacity)
                TriggerReloadInterrupt();
        }       
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
                int shells = 1;
                if (e.intParameter != 0)
                    shells = e.intParameter;
                OnReload(shells);
                break;
            case "reloadend":
            case "reload end":
            case "endreload":
            case "end reload":
                // This should only be called from shotgun-like weapons that reload recursively.
                if (!ShotgunReload)
                {
                    Debug.LogError($"Gun {Item.Name} invoked callback 'Reload End' from animation, but it is not using ShotgunReload. Use 'Reload' callback instead!");
                }
                IsReloading = false;
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
        shootQueued = false;
        shootPressed = false;

        if(ShotgunReload)
            Anim.SetBool("Reload", false);
    }
}

public enum FireMode
{
    Semi,
    Auto
}
