
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An item is an object that can be dropped in the world, picked up, stored in inventories, used, sold etc.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ItemAnimator))]
public partial class Item : MonoBehaviour
{
    public static float DroppedSpinSpeed { get; set; } = 90;

    public ItemAnimator Animation
    {
        get
        {
            if (_anim == null)
                _anim = GetComponent<ItemAnimator>();
            return _anim;
        }
    }
    private ItemAnimator _anim;
    public Gun Gun
    {
        get
        {
            if (_gun == null)
                _gun = GetComponent<Gun>();
            return _gun;
        }
    }
    private Gun _gun;
    public MeleeWeapon MeleeWeapon
    {
        get
        {
            if (_melee == null)
                _melee = GetComponent<MeleeWeapon>();
            return _melee;
        }
    }
    private MeleeWeapon _melee;

    public Texture2D IconTexture { get; private set; }
    public bool IsGun { get { return Gun != null; } }
    public bool IsMeleeWeapon { get { return MeleeWeapon != null; } }
    public ItemManager Manager { get; internal set; }

    [Header("Details")]
    public string Name;
    public Texture DefaultIcon;

    [Header("Equipped")]
    public Vector3 EquippedOffset;    

    [Header("Hands")]
    public Transform LeftHandPos;
    public Transform RightHandPos;

    public ItemState State
    {
        get
        {
            if (Manager == null)
                return ItemState.Dropped;

            if (Manager.ActiveItem == this)
                return ItemState.Active;
            else
                return ItemState.Equipped;
        }
    }
    
    private IconGen.Request iconRequest;

    private void Awake()
    {        
        gameObject.layer = SortingLayer.NameToID("Items");        
    }

    private void OnDestroy()
    {
        // Destroy icon to avoid memory leaks.
        if(IconTexture != null)
        {
            Destroy(IconTexture);
            IconTexture = null;
        }
        if(iconRequest != null)
        {
            iconRequest.IsDone = true;
            iconRequest.InputTexture = null;
        }
    }

    [MyBox.ButtonMethod]
    private bool RefreshIcon()
    {
        return RefreshIcon(false);
    }

    public bool RefreshIcon(bool forceNewRequest = false)
    {
        if (!forceNewRequest)
        {
            if (iconRequest != null)
            {
                //Debug.LogWarning($"Item icon is already being regenerated, please wait or call with forceNewRequest set to true.");
                return false;
            }
        }

        // Cancel prevoious request, if it exists.
        if(iconRequest != null)
        {
            iconRequest.IsDone = true;
            iconRequest.InputTexture = null;
            iconRequest = null;
        }

        // Create request.
        iconRequest = new IconGen.Request()
        {
            Item = this,
            InputTexture = IconTexture,
            OnComplete = newIcon =>
            {
                if(newIcon != IconTexture)
                {
                    Destroy(IconTexture);
                }
                IconTexture = newIcon;

                iconRequest = null;
            }
        };

        // Submit request.
        IconGen.RequestIcon(iconRequest);

        return true;
    }

    public void UponEquip()
    {
        transform.localPosition = EquippedOffset;
        transform.localRotation = Quaternion.identity;
        BroadcastMessage("OnEquip",  SendMessageOptions.DontRequireReceiver);
    }

    public void UponDequip()
    {
        BroadcastMessage("OnDequip", SendMessageOptions.DontRequireReceiver);
    }

    public void UponActivate()
    {
        transform.localPosition = EquippedOffset;
        transform.localRotation = Quaternion.identity;
        BroadcastMessage("OnActivate", SendMessageOptions.DontRequireReceiver);
    }

    public void UponDeactivate()
    {
        BroadcastMessage("OnDeactivate", SendMessageOptions.DontRequireReceiver);
    }

    private void Update()
    {
        // It is active when held by the player.
        if (State == ItemState.Active)
        {
            transform.localPosition = EquippedOffset;
            transform.localRotation = Quaternion.identity;
        }
        else if(State == ItemState.Dropped)
        {
            transform.localEulerAngles += new Vector3(0f, DroppedSpinSpeed, 0f) * Time.deltaTime;
        }

        // It is dropped if not equipped on the player.
        Animation.Animator?.SetBool("Dropped", State == ItemState.Dropped);
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}

public enum ItemState
{
    Dropped,
    Equipped,
    Active
}
