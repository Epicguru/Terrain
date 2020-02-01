
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
    public Item CurrentItem { get; private set; }

    public Item ToPickUp;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (CurrentItem == null)
                SetItem(ToPickUp);
            else
                DropCurrentItem();
        }
    }

    public void SetItem(Item i)
    {
        if (i == CurrentItem)
            return;

        DropCurrentItem();

        i.Manager = this;
        i.transform.SetParent(ItemParent);
        i.transform.localPosition = Vector3.zero;
        i.transform.localRotation = Quaternion.identity;
        CurrentItem = i;
    }

    public void DropCurrentItem()
    {
        if (CurrentItem == null)
            return;

        CurrentItem.Manager = null;
        CurrentItem.transform.SetParent(null);
    }
}
