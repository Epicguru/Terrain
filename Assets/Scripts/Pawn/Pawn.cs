
using UnityEngine;

[RequireComponent(typeof(ItemManager))]
public class Pawn : MonoBehaviour
{
    public ItemManager ItemManager
    {
        get
        {
            if (_items == null)
                _items = GetComponent<ItemManager>();
            return _items;
        }
    }
    private ItemManager _items;
    public PawnController Controller
    {
        get
        {
            if (_pc == null)
                _pc = GetComponent<PawnController>();
            return _pc;
        }
    }
    private PawnController _pc;
    public ArmManager ArmManager
    {
        get
        {
            if (_arms == null)
                _arms = GetComponent<ArmManager>();
            return _arms;
        }
    }
    private ArmManager _arms;

    public string Name = "Bob";
}