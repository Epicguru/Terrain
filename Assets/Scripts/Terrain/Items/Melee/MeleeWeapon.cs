using Terrain.Camera;
using Terrain.Enemies;
using UnityEngine;

namespace Terrain.Items.Melee
{
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
        public Animator Anim => Item.Animation.Animator;

        [Header("Attack")]
        public Transform[] SweepPoints;
        public float SweepPointLength = 0.3f;

        [Header("Defence")]
        public bool Block;

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
            Player.Player.Input.actions["Shoot"].performed += ctx =>
            {
                if (Item.State == ItemState.Active)
                    TriggerAttack(Random.Range(0, 3));
            };
            Player.Player.Input.actions["Inspect"].performed += ctx =>
            {
                if (Item.State == ItemState.Active)
                    TriggerInspect();
            };
            Player.Player.Input.actions["Aim"].started += ctx =>
            {
                if (Item.State == ItemState.Active)
                    Block = true;
            };
            Player.Player.Input.actions["Aim"].canceled += ctx =>
            {
                if (Item.State == ItemState.Active)
                    Block = false;
            };
        }

        private void Update()
        {
            if (Item.State != ItemState.Active)
                return;

            // Update item run flag: melee weapons don't allow running while blocking.
            Item.AllowRunning = !Block;

            Anim.SetBool("Block", Block);
            Anim.SetBool("CustomAutoExit", CustomAnimationAutoExit);

            Anim.SetFloat("CustomSpeed", CustomAnimationSpeed);

            // Update UI elements (such as block indicator
            UpdateUI();

            UpdateHitting();
        }

        private void UpdateHitting()
        {
            if (!IsInAttack)
                return;

            foreach(var sweep in SweepPoints)
            {
                Vector3 start = sweep.position;
                Vector3 end = sweep.position + sweep.forward * SweepPointLength * sweep.localScale.z;

                bool didHit = Physics.Linecast(start, end, out var hitInfo);
                Debug.DrawLine(start, end, didHit ? Color.red : Color.yellow, 10f);

                if (didHit)
                {
                    var health = hitInfo.collider.GetComponentInParent<Health>();
                    if(health != null)
                    {
                        health.ChangeHealth(-10);
                    }
                }
            }
        }

        public void TriggerCustom()
        {
            System.Action a = () => { Anim.SetTrigger("Custom"); };
            Item.Animation.AddPendingAction(new ItemAnimator.PendingAction()
            {
                Action = a,
                LayerIndex = new int[] { 1 },
                LayerWeight = new float[] { 0f },
                ComparisonType = ItemAnimator.ComparisonType.LessOrEqual
            });
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

        private void UpdateUI()
        {
            var block = GlobalUIElement.Get<UI_BlockIndicator>();
            if (block == null)
                return;

            block.Active = Block;
            block.BlockDirection = 1;
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            foreach(var point in SweepPoints)
            {
                Gizmos.DrawLine(point.position, point.position + point.forward * SweepPointLength * point.localScale.z);
            }
        }
    }
}
