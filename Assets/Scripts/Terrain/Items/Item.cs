using System.Collections.Generic;
using Terrain.Items.Guns;
using Terrain.Items.Melee;
using UnityEngine;

namespace Terrain.Items
{
    /// <summary>
    /// An item is an object that can be dropped in the world, picked up, stored in inventories, used, sold etc.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ItemAnimator))]
    public partial class Item : MonoBehaviour
    {
        public static float DroppedSpinSpeed { get; set; } = 90;
        public static IReadOnlyList<Item> DroppedItems { get { return _dropped; } }
        private static List<Item> _dropped = new List<Item>();
        private static System.Text.StringBuilder str = new System.Text.StringBuilder();

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
        public bool AllowRunning { get; set; } = true;

        [Header("Details")]
        public string Name;
        public ItemRarity Rarity = ItemRarity.Common;
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
            _dropped.Add(this);
        }

        private void OnDestroy()
        {
            // Destroy icon to avoid memory leaks.
            if (IconTexture != null)
            {
                Destroy(IconTexture);
                IconTexture = null;
            }
            if (iconRequest != null)
            {
                iconRequest.IsDone = true;
                iconRequest.InputTexture = null;
            }
            if (_dropped.Contains(this))
                _dropped.Remove(this);
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
            if (iconRequest != null)
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
                    if (newIcon != IconTexture)
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
            BroadcastMessage("OnEquip", SendMessageOptions.DontRequireReceiver);

            if (_dropped.Contains(this))
                _dropped.Remove(this);
        }

        public void UponDequip()
        {
            BroadcastMessage("OnDequip", SendMessageOptions.DontRequireReceiver);

            if (!_dropped.Contains(this))
                _dropped.Add(this);
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
            else if (State == ItemState.Dropped)
            {
                transform.localEulerAngles += new Vector3(0f, DroppedSpinSpeed, 0f) * Time.deltaTime;
            }

            // It is dropped if not equipped on the player.
            Animation.Animator?.SetBool("Dropped", State == ItemState.Dropped);
        }

        /// <summary>
        /// Gets a user-readable list of 'primary' details: important details about an item that are
        /// unlikely to change within the lifespan of the item. For example [Gun, Melee Weapon, Broken or Quest Item].
        /// </summary>
        public string GetPrimaryDetails()
        {
            str.Clear();
            int count = 0;
            if (IsGun)
            {
                if (count++ != 0)
                    str.Append(", ");
                str.Append("Gun");
            }
            if (IsMeleeWeapon)
            {
                if (count++ != 0)
                    str.Append(", ");
                str.Append("Melee Weapon ");
            }

            return str.ToString();
        }

        /// <summary>
        /// Gets a user-readable list of 'secondary' details: details that are not crucial to know about the item, but might be
        /// useful for more in-depth players. Should also be used for things that might change often. For example, [Ammo type: 5.56x45, Linked to: Door].
        /// </summary>
        public string GetSecondaryDetails()
        {
            str.Clear();
            return str.ToString();
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
}