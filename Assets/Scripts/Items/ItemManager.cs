using MyBox;
using System.Collections;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public Pawn Pawn
    {
        get
        {
            if (_pawn == null)
                _pawn = GetComponent<Pawn>();
            return _pawn;
        }
    }
    private Pawn _pawn;

    public Transform ItemParent;
    public Item ActiveItem { get { return currentIndex == -1 ? null : EquippedItems[currentIndex]; } }

    public Item[] EquippedItems = new Item[3];
    public int MaxEquippedItems { get { return EquippedItems.Length; } }
    [SearchableEnum]
    public KeyCode[] Keys = new KeyCode[3] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

    [SerializeField]
    [ReadOnly]
    private int currentIndex = -1;

    /*
     * Items can be in 3 states:
     * 1. Dropped. Dropped items have physics, they are always in the 'Dropped' animation state.
     * 2. Equipped. Equipped items are parented to the holding transform, but are not visible or animated.
     * 3. Active. Active items are equipped and also held in the hands, visible and animated.
     */

    private void Start()
    {
        for (int i = 0; i < MaxEquippedItems; i++)
        {
            var item = EquippedItems[i];
            if (item != null)
            {
                item.gameObject.SetActive(true);
                StartCoroutine(Disable(item));
                item.Manager = this;
            }            
        }
    }

    private void Update()
    {
        for (int i = 0; i < Keys.Length; i++)
        {
            if (Input.GetKeyDown(Keys[i]))
            {
                SetActiveItem(i);
            }
        }
    }

    public void SetActiveItem(int index)
    {
        if (index < 0 || index >= MaxEquippedItems)
            index = -1;
        if (index != -1 && EquippedItems[index] == null)
            index = -1;

        if (currentIndex == index)
            return;

        Item i = index == -1 ? null : EquippedItems[index];
        Item currentItem = currentIndex == -1 ? null : EquippedItems[currentIndex];

        // Dequip current.
        if (currentItem != null)
        {
            StartCoroutine(Disable(currentItem));
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
            if (EquippedItems[j] == null)
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

        Item item = EquippedItems[index];
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
        }

        // Give the item the upon dequip message.
        item.Manager = null;
        item.transform.localPosition = item.EquippedOffset;
        item.transform.localRotation = Quaternion.identity;
        item.transform.SetParent(null, true);
        item.UponDequip();

        return item;
    }

    public Item Dequip(int index, Vector3 pos, Quaternion rotation, Vector3 velocity)
    {
        var i = Dequip(index);
        if (i != null)
        {
            i.transform.position = pos;
            i.transform.rotation = rotation;
            if (i.Body != null)
                i.Body.velocity = velocity;
        }

        return i;
    }

    public int GetIndex(Item i)
    {
        if (i == null || i.Manager == null)
            return -1;

        for (int j = 0; j < MaxEquippedItems; j++)
        {
            if (EquippedItems[j] == i)
                return j;
        }

        return -1;
    }

    private void Enable(Item i, bool sendMessage = true)
    {
        // Check if already active.
        if (i.Animator.gameObject.activeSelf)
            return;

        i.Animator.gameObject.SetActive(true);
        i.transform.localPosition = i.EquippedOffset;
        i.transform.localRotation = Quaternion.identity;

        if (sendMessage)
            i.UponActivate();
    }

    private IEnumerator Disable(Item i)
    {
        // Check if already disabled.
        if (!i.Animator.gameObject.activeSelf)
            yield return null;

        i.UponDeactivate();

        //c.Anim.Anim.enabled = false;
        i.Animator.Play("Idle", 0);
        for (int j = 0; j < i.Animator.layerCount; j++)
        {
            i.Animator.SetLayerWeight(j, 0);
        }

        // Out of sight, out of mind. There is a single frame where the gun appears in the idle pose, which is correct, but also looks lame.
        // So I teleport it far away so that it can render the pose without it flickering over the screen.
        i.transform.position = Vector3.one * 1000f;

        // Wait for end of frame for that to have been applied.
        yield return new WaitForEndOfFrame();

        i.Animator.gameObject.SetActive(false);

        yield return null;
    }
}
