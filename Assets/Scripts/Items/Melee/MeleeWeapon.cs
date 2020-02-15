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
    public Animator Anim { get { return Item.Animator; } }

    [Header("Defence")]
    public bool Block;
    [Range(0, 2)]
    public int BlockDirection;

    [Header("Custom Animation")]
    public bool CustomAnimationAutoExit = true;
    public float CustomAnimationSpeed = 1f;

    [Header("Other Input")]
    public AnimationClip Clip;

    [Header("Runtime")]
    public bool IsInAttack;

    private void Awake()
    {
        SetupInput();
    }

    private void SetupInput()
    {
        Player.Input.actions["Shoot"].performed += ctx =>
        {
            if (Item.State == ItemState.Active)
                TriggerAttack(Random.Range(0, 3));
        };
        Player.Input.actions["Inspect"].performed += ctx =>
        {
            if (Item.State == ItemState.Active)
                TriggerInspect();
        };
        Player.Input.actions["Aim"].started += ctx =>
        {
            if (Item.State == ItemState.Active)
                Block = true;
        };
        Player.Input.actions["Aim"].canceled += ctx =>
        {
            if (Item.State == ItemState.Active)
                Block = false;
        };
    }

    private void Update()
    {
        if (Item.State != ItemState.Active)
            return;

        UpdateBlocking();

        Anim.SetInteger("BlockDirection", BlockDirection);

        Anim.SetBool("Block", Block);
        Anim.SetBool("CustomAutoExit", CustomAnimationAutoExit);

        Anim.SetFloat("CustomSpeed", CustomAnimationSpeed);

        // Update UI elements (such as block indicator
        UpdateUI();
    }

    public void TriggerCustom()
    {
        Anim.SetTrigger("Custom");
    }

    public void TriggerCustomExit()
    {
        Anim.SetTrigger("CustomExit");
    }

    public void TriggerAttack(int variant)
    {
        if (!Block)
        {
            Anim.SetInteger("AttackVariant", variant);
            Anim.SetTrigger("Attack");
        }
    }

    public void TriggerBlockHit(int direction)
    {
        Anim.SetInteger("BlockHitDirection", direction);
        Anim.SetTrigger("BlockHit");
    }

    public void TriggerThrow()
    {
        Anim.SetTrigger("Throw");
    }

    public void TriggerInspect()
    {
        Anim.SetTrigger("Inspect");
    }

    private void UpdateBlocking()
    {
        if (Block)
        {
            // Determine direction.
            Vector2 up = new Vector2(0f, 1f);
            Vector2 down = new Vector2(0f, -1f);
            Vector2 right = new Vector2(1f, 0f);
            Vector2 left = new Vector2(-1f, 0f);

            var look = CameraLook.Instance;
            Vector2 movement = new Vector2(look.HorizontalTurnDelta, -look.VerticalTurnDelta);
            Vector2 moveNormal = movement.normalized;

            if (movement.magnitude >= 1f)
            {
                float upAmount = Vector2.Dot(up, moveNormal);
                float downAmount = Vector2.Dot(down, moveNormal); // Not a valid block direction, but used to prevent weird blocking when aiming down.
                float rightAmount = Vector2.Dot(right, moveNormal);
                float leftAmount = Vector2.Dot(left, moveNormal);

                int dir = GetLargest(upAmount, rightAmount, leftAmount, downAmount);
                if (dir != 3)
                {
                    BlockDirection = dir;
                }
            }
        }
    }

    private void UpdateUI()
    {
        var block = GlobalUIElement.Get<UI_BlockIndicator>();
        block.Active = Block;
        block.BlockDirection = BlockDirection;
    }

    private void UIBlockHit()
    {
        var block = GlobalUIElement.Get<UI_BlockIndicator>();
        block.BlockHit();
    }

    private int GetLargest(params float[] args)
    {
        float record = float.MinValue;
        int index = -1;
        for (int i = 0; i < args.Length; i++)
        {
            float value = args[i];
            if (value > record)
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
    }

    private int NewRandom(int min, int max, int not)
    {
        if (Mathf.Abs(max - min) < 1)
            return min;

        int r = Random.Range(min, max);
        while (r == not)
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

            case "throw":
                float speed = e.floatParameter == 0f ? 15f : e.floatParameter;
                var graphics = transform.GetChild(0);

                Anim.SetBool("Dropped", true);
                //Item.Manager.Dequip(graphics.position + transform.forward * speed * Time.deltaTime, graphics.rotation, transform.forward * speed);
                Anim.Update(Time.deltaTime);

                break;

            default:
                break;
        }
    }

    private void OnDeactivate()
    {
        // Update UI to ensure that UI elements don't 'linger'.
        Block = false;
        IsInAttack = false;
        UpdateUI();
    }
}
