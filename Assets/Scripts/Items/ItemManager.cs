using MyBox;
using System.Collections;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public Player Pawn
    {
        get
        {
            if (_pawn == null)
                _pawn = GetComponent<Player>();
            return _pawn;
        }
    }
    private Player _pawn;

    public Transform ItemParent;
    public Item ActiveItem { get { return currentIndex == -1 ? null : equippedItems[currentIndex]; } }

    [SerializeField]
    private Item[] equippedItems = new Item[3];
    public int MaxEquippedItems { get { return equippedItems.Length; } }
    [SearchableEnum]
    public KeyCode[] Keys = new KeyCode[3] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

    [SerializeField]
    [ReadOnly]
    private int currentIndex = -1;
    private UI_Hotbar hotbar;

    /*
     * Items can be in 3 states:
     * 1. Dropped. Dropped items have physics, they are always in the 'Dropped' animation state.
     * 2. Equipped. Equipped items are parented to the holding transform, but are not visible or animated.
     * 3. Active. Active items are equipped and also held in the hands, visible and animated.
     */

    private void Awake()
    {
        // Make sure equipped items have correct initial state.
        for (int i = 0; i < MaxEquippedItems; i++)
        {
            var item = equippedItems[i];
            if (item != null)
            {
                item.gameObject.SetActive(true);
                item.Animation.gameObject.SetActive(false);
                //StartCoroutine(Disable(item));
                item.Manager = this;
            }            
        }

        // Setup input.
        Player.Input.actions["Drop"].performed += ctx =>
        {
            Dequip(currentIndex);
        };
        Player.Input.actions["Next Item"].performed += ctx =>
        {
            bool found = false;
            int newIndex = currentIndex + 1;
            for (int i = 0; i < MaxEquippedItems; i++)
            {
                if (newIndex >= MaxEquippedItems)
                    newIndex = 0;
                if (newIndex < 0)
                    newIndex = 0;

                if (equippedItems[newIndex] != null)
                {
                    found = true;
                    break;
                }
                else
                {
                    newIndex++;
                }
            }
            if(found)
                SetActiveItem(newIndex);
        };
        Player.Input.actions["Previous Item"].performed += ctx =>
        {
            bool found = false;
            int newIndex = currentIndex - 1;
            for (int i = 0; i < MaxEquippedItems; i++)
            {
                if (newIndex >= MaxEquippedItems)
                    newIndex = 0;
                if (newIndex < 0)
                    newIndex = MaxEquippedItems - 1;

                if (equippedItems[newIndex] != null)
                {
                    found = true;
                    break;
                }
                else
                {
                    newIndex--;
                }
            }
            if (found)
                SetActiveItem(newIndex);
        };
        Player.Input.actions["Empty Hands"].performed += ctx =>
        {
            SetActiveItem(-1);
        };
    }

    private void Update()
    {
        for (int i = 0; i < Keys.Length; i++)
        {
            if (Input.GetKeyDown(Keys[i]))
            {
                // Try to equip the i'th item.
                int found = 0;
                for (int j = 0; j < equippedItems.Length; j++)
                {
                    if (equippedItems[j] == null)
                        continue;

                    if(found == i && found != currentIndex)
                    {
                        SetActiveItem(j);
                    }
                    found++;
                }
                break;
            }
        }

        if (hotbar == null)
            hotbar = GlobalUIElement.Get<UI_Hotbar>();

        if (hotbar != null)
            hotbar.UpdateIcons(this);
    }

    public void SetActiveItem(int index)
    {
        if (index < 0 || index >= MaxEquippedItems)
            index = -1;
        if (index != -1 && equippedItems[index] == null)
            index = -1;

        if (currentIndex == index)
            return;

        Item i = index == -1 ? null : equippedItems[index];
        Item currentItem = currentIndex == -1 ? null : equippedItems[currentIndex];

        // Dequip current.
        if (currentItem != null)
        {
            Disable(currentItem);
        }

        // Equip new.
        if (i != null)
        {
            Enable(i);
        }

        // Update the current index.
        currentIndex = index;
    }

    public int Equip(Item i)
    {
        if (i == null)
        {
            Debug.LogWarning("Cannot equip null item!");
            return -1;
        }

        if (i.Manager != null)
        {
            Debug.LogWarning($"Item {i} is already equipped.");
            return -1;
        }

        int index = -1;
        for (int j = 0; j < MaxEquippedItems; j++)
        {
            if (equippedItems[j] == null)
            {
                index = j;
                break;
            }
        }

        if (index == -1)
        {
            Debug.LogWarning($"There is no space left to equip item {i}.");
            return -1;
        }

        i.Manager = this;
        i.transform.SetParent(ItemParent);
        i.UponEquip();

        return index;
    }

    public Item Dequip(int index)
    {
        if (index < 0 || index >= MaxEquippedItems)
        {
            Debug.LogWarning($"Cannot dequip item at index {index} because that index is out of range.");
            return null;
        }

        Item item = equippedItems[index];
        if (item == null)
            return null;

        bool isActive = index == currentIndex;

        if (!isActive)
        {
            // Items that are not active need to have their animator re-enabled.
            Enable(item, false); // Enable it but don't send the 'UponActivate' message. This basically just enables the animator and graphics.
        }
        else
        {
            // This item is currently active. Send the deactivate message first.
            item.UponDeactivate();
            currentIndex = -1;
        }

        // Give the item the upon dequip message.
        item.Manager = null;
        item.transform.localPosition = item.EquippedOffset;
        item.transform.localRotation = Quaternion.identity;
        item.transform.SetParent(null, true);
        item.UponDequip();
        equippedItems[index] = null;

        return item;
    }

    public Item Dequip(int index, Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        var i = Dequip(index);
        if (i != null)
        {
            i.transform.position = pos;
            i.transform.rotation = rotation;
            // TODO reimplement velocity.
        }

        return i;
    }

    public int GetIndex(Item i)
    {
        if (i == null || i.Manager == null)
            return -1;

        for (int j = 0; j < MaxEquippedItems; j++)
        {
            if (equippedItems[j] == i)
                return j;
        }

        return -1;
    }

    public Item GetEquippedItem(int index)
    {
        if (index < 0 || index >= MaxEquippedItems)
            return null;

        return equippedItems[index];
    }

    private void Enable(Item i, bool sendMessage = true)
    {
        // Check if already active.
        if (i.Animation.gameObject.activeSelf)
            return;

        i.Animation.gameObject.SetActive(true);
        i.transform.localPosition = i.EquippedOffset;
        i.transform.localRotation = Quaternion.identity;

        if (sendMessage)
            i.UponActivate();
    }

    private void Disable(Item i)
    {
        // Check if already disabled.
        if (!i.Animation.gameObject.activeSelf)
            return;

        i.UponDeactivate();

        //c.Anim.Anim.enabled = false;
        i.Animation.Animator.Play("Idle", 0);
        for (int j = 0; j < i.Animation.Animator.layerCount; j++)
        {
            i.Animation.Animator.SetLayerWeight(j, 0);
        }
        i.Animation.Animator.Update(0f);

        i.Animation.gameObject.SetActive(false);
    }
}
