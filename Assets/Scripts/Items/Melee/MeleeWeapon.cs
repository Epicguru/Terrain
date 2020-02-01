using UnityEngine;

[RequireComponent(typeof(Item))]
public class MeleeWeapon : MonoBehaviour
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
    public Animator Anim
    {
        get
        {
            if (_anim == null)
                _anim = GetComponentInChildren<Animator>();
            return _anim;
        }
    }
    private Animator _anim;

    [Header("References")]
    public CameraLook Look;

    [Header("Defence")]
    public bool Block;
    [Range(0, 2)]
    public int BlockDirection;
    public bool BlockHit;
    [Range(0, 2)]
    public int BlockHitDirection;

    [Header("Attack")]
    public bool Attack;
    public int AttackVariant;
    public float ComboTimeout = 0.3f;
    public bool Throw;

    [Header("Runtime")]
    public bool IsInAttack;

    private void Update()
    {
        UpdateInput();

        Anim.SetBool("Dropped", !Item.IsEquipped);

        Anim.SetInteger("BlockDirection", BlockDirection);
        Anim.SetBool("Block", Block);

        Anim.SetInteger("AttackVariant", AttackVariant);
        if (Attack)
        {
            Attack = false;
            if (!Block)
            {
                Anim.SetTrigger("Attack");
            }
        }

        Anim.SetInteger("BlockHitDirection", BlockHitDirection);
        if (BlockHit)
        {
            BlockHit = false;
            Anim.SetTrigger("BlockHit");
        }

        if (Throw)
        {
            Throw = false;
            Anim.SetTrigger("Throw");
        }
    }

    private void UpdateInput()
    {
        if (!Item.IsEquipped)
            return;

        Block = Input.GetKey(KeyCode.Mouse1);
        if (Block)
        {
            // Determine direction.
            Vector2 up = new Vector2(0f, 1f);
            Vector2 down = new Vector2(0f, -1f);
            Vector2 right = new Vector2(1f, 0f);
            Vector2 left = new Vector2(-1f, 0f);

            Vector2 movement = new Vector2(Look.HorizontalTurnDelta, -Look.VerticalTurnDelta);
            Vector2 moveNormal = movement.normalized;

            if(movement.magnitude >= 1f)
            {
                float upAmount = Vector2.Dot(up, moveNormal);
                float downAmount = Vector2.Dot(down, moveNormal); // Not a valid block direction, but used to prevent weird blocking when aiming down.
                float rightAmount = Vector2.Dot(right, moveNormal);
                float leftAmount = Vector2.Dot(left, moveNormal);

                int dir = GetLargest(upAmount, rightAmount, leftAmount, downAmount);
                if(dir != 3)
                {
                    BlockDirection = dir;
                }
            }           
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack = true;
        }

        Throw = Input.GetKeyDown(KeyCode.E);

        if(Input.GetKeyDown(KeyCode.F) && Block)
        {
            BlockHit = true;
            BlockHitDirection = BlockDirection;
        }
    }

    private int GetLargest(params float[] args)
    {
        float record = float.MinValue;
        int index = -1;
        for (int i = 0; i < args.Length; i++)
        {
            float value = args[i];
            if(value > record)
            {
                record = value;
                index = i;
            }
        }

        return index;
    }

    private void OnAttackStart()
    {
        IsInAttack = true;
    }

    private void OnAttackEnd()
    {
        IsInAttack = false;
        AttackVariant = NewRandom(0, 3, AttackVariant);
    }

    private int NewRandom(int min, int max, int not)
    {
        if (Mathf.Abs(max - min) < 1)
            return min;

        int r = Random.Range(min, max);
        while(r == not)
        {
            r = Random.Range(min, max);
        }
        return r;
    }

    private void UponAnimationEvent(AnimationEvent e)
    {
        string s = e.stringParameter.Trim().ToLower();

        switch (s)
        {
            case "attack start":
            case "attackstart":
                OnAttackStart();
                break;

            case "attack end":
            case "attackend":
                OnAttackEnd();
                break;

            default:
                break;
        }
    }
}
