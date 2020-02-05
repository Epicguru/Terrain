
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
                _gunSlide = GetComponent<GunSlide>();
            return _gunSlide;
        }
    }
    private GunSlide _gunSlide;
    public Animator Anim { get { return Item.Animator; } }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
            Anim.SetTrigger("Shoot");
    }
}
