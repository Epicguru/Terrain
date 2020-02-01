
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

    private void Update()
    {
        // TODO possibly disable (trigger) colliders to avoid weird interaction with world rigidbodies.
        Body.isKinematic = IsEquipped;
        Body.interpolation = RigidbodyInterpolation.None;

        if (IsEquipped)
        {
            transform.localPosition = EquippedOffset;
            transform.localRotation = Quaternion.identity;
        }
    }
}
