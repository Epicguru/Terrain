
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    public Rigidbody Body
    {
        get
        {
            if (_body == null)
                _body = GetComponent<Rigidbody>();
            return _body;
        }
    }
    private Rigidbody _body;
    public ItemManager Manager { get; internal set; }

    [Header("Details")]
    public string Name;

    [Header("Equipped")]
    public Vector3 EquippedOffset;

    [Header("Arms")]
    public Transform LeftHandPos;
    public Transform RightHandPos;

    public bool IsEquipped { get { return Manager != null && Manager.CurrentItem == this; } }

    private void Awake()
    {        
        Body.interpolation = RigidbodyInterpolation.None;
    }

    public void UponEquip()
    {
        Body.isKinematic = true;
        transform.localPosition = EquippedOffset;
        transform.localRotation = Quaternion.identity;
        BroadcastMessage("OnEquip",  SendMessageOptions.DontRequireReceiver);
    }

    public void UponDequip()
    {
        Body.isKinematic = false;
        BroadcastMessage("OnDequip", SendMessageOptions.DontRequireReceiver);
    }

    private void Update()
    {
        if (IsEquipped)
        {
            transform.localPosition = EquippedOffset;
            transform.localRotation = Quaternion.identity;
        }
    }
}
