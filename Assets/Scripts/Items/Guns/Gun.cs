
using MyBox;
using System;
using System.Collections;
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
    public bool BulletInChamber = false;
    public int MagazineBullets = 0;
    [PositiveValueOnly]
    public int MagazineCapacity = 30;

    [Foldout("ADS", true)]
    public bool ADS = false;
    [PositiveValueOnly]
    public float ADSTime = 0.3f;

    public int TotalCurrentBullets { get { return MagazineBullets + (BulletInChamber ? 1 : 0); } }
    public bool IsReloading { get; protected set; }
    public bool CanADS { get { return (Anim.GetCurrentAnimatorStateInfo(0).IsTag("ADS") && !Anim.IsInTransition(0)) && todoAfterUnADS.Count == 0; } }

    public float ADSLerp { get; private set; }
    public bool IsInADS { get { return ADSLerp > 0f; } }

    private Queue<Action> todoAfterUnADS = new Queue<Action>();

    private void Start()
    {
        MagazineBullets = MagazineCapacity;
    }

    private void Update()
    {
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

        Anim.SetLayerWeight(1, ADSLerp);
    }

    private void UpdateInput()
    {
        ADS = Input.GetKey(KeyCode.Mouse1);
        if (Input.GetKeyDown(KeyCode.Mouse0))
            TriggerShoot();
        if (Input.GetKeyDown(KeyCode.R))
            TriggerReload();
        if (Input.GetKeyDown(KeyCode.Mouse3))
            TriggerMelee();
        if (Input.GetKeyDown(KeyCode.F))
            TriggerInspect();
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
        Action a = new Action(() =>
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
        }
    }
}
