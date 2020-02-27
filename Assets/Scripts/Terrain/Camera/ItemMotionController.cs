using System.Collections.Generic;
using MyBox;
using Terrain.Items;
using Terrain.Items.Guns;
using UnityEngine;

namespace Terrain.Camera
{
    [ExecuteInEditMode]
    public class ItemMotionController : MonoBehaviour
    {
        public static ItemMotionController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<ItemMotionController>();
                return _instance;
            }
        }
        private static ItemMotionController _instance;

        public ItemManager ItemManager;
        public Animator Animator;
        public Transform AnimTransform => Animator.transform;
        public bool RunInEditMode = true;

        [ReadOnly]
        public float AnimWeight = 1f;
        [ReadOnly]
        public Vector3 AnglePunch;
        public float AnglePunchRecovery = 0.9f;

        private const float ANGLE_VEL_CALC_FREQUENCY = 60f;
        private const float ANGLE_VEL_CALC_DELTA_TIME = 1f / ANGLE_VEL_CALC_FREQUENCY;
        private readonly List<AnglePunchItem> punches = new List<AnglePunchItem>();
        private struct AnglePunchItem
        {
            public Vector3 Velocity;
            public float Falloff;
        }

        private void Awake()
        {
            _instance = this;
            InvokeRepeating("UpdatePunches", 0f, ANGLE_VEL_CALC_DELTA_TIME);
        }

        private void Update()
        {
            if (!Application.isPlaying && !RunInEditMode)
                return;

            UpdateAnimator();

            AnimWeight = 1f;
            Gun gun = null;
            if (ItemManager.ActiveItem != null && ItemManager.ActiveItem.IsGun)
                gun = ItemManager.ActiveItem.Gun;
            if(gun != null)
                AnimWeight = 1f - gun.ADSLerp;


            Vector3 pos = Vector3.Lerp(Vector3.zero, AnimTransform.localPosition, AnimWeight);
            Vector3 rot = Quaternion.Lerp(Quaternion.identity, AnimTransform.localRotation, AnimWeight).eulerAngles;
            rot += AnglePunch;

            transform.localPosition = pos;
            transform.localEulerAngles = rot;
        }

        private void UpdateAnimator()
        {
            bool move = Player.Player.Instance.Movement.IsMoving;
            bool run = Player.Player.Instance.Movement.IsRunning;

            if (Animator.runtimeAnimatorController == null)
                return;

            Animator.SetBool("New Bool", run); // Animator panel is super bugged (2020.1a23) so can't change bool name.
            Animator.SetBool("Walk", move);
        }

        public void AddPunch(Vector3 anglesVel, float falloff)
        {
            AnglePunchItem a = new AnglePunchItem();
            anglesVel.x *= -1f;
            a.Velocity = anglesVel;
            a.Falloff = falloff;

            if(a.Falloff <= 0 || a.Falloff >= 1f)
            {
                Debug.LogWarning($"When adding aimpunch, a falloff value of {falloff} is normally NOT a good idea: it should be more than zero and less than one.");
            }

            punches.Add(a);
        }

        private void UpdatePunches()
        {
            const float TOLERANCE = 0.1f;
            const float TOL_SQR = TOLERANCE * TOLERANCE;
            for (int i = 0; i < punches.Count; i++)
            {
                var item = punches[i];

                // Apply this punch to the global offset.
                AnglePunch += item.Velocity * ANGLE_VEL_CALC_DELTA_TIME;

                // Reduce the velocity (magnitude) of this punch.
                item.Velocity *= item.Falloff;

                // Write item back into list.
                punches[i] = item;

                // Remove this velocity item if it's speed is less than the tolerance; basically if it has no effect any more.
                if(item.Velocity.sqrMagnitude <= TOL_SQR)
                {
                    punches.RemoveAt(i);
                }
            }

            // Apply global reduction to punch. This causes it to reset to zero over time.
            AnglePunch *= AnglePunchRecovery;
        }
    }
}
