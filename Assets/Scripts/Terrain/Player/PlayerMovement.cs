using System;
using Terrain.Utils;
using UnityEngine;

namespace Terrain.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        public Player Player
        {
            get
            {
                if (_pawn == null)
                    _pawn = GetComponent<Player>();
                return _pawn;
            }
        }
        private Player _pawn;
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

        public bool Override { get; set; } = false;
        public Vector3 OverridePosition { get; set; }
        public bool IsMoving { get { return flatInput != Vector2.zero; } } // TODO fix to use actual body displacement (not velocity).
        public bool IsRunning { get { return CanRun && run; } }
        public bool CanRun
        {
            get
            {
                if (!IsMoving)
                    return false;

                if (!IsGrounded)
                    return false;

                var currentItem = Player.ItemManager.ActiveItem;
                if(currentItem != null)
                {
                    return currentItem.AllowRunning;
                }
                else
                {
                    return true;
                }
            }
        }
        public bool IsGrounded { get; private set; }

        public float GravityScale = 1f;
        public float AccelerateForce = 500f;
        public float AccelerateForceRun = 800f;
        public float DecelerateCoefficient = 50f;
        [Header("Grounding")]
        [Range(0f, 90f)]
        public float MaxGroundAngle = 50f;

        [Header("Jumping")]
        public float JumpVel = 8f;
        public Action OnJump;

        private Vector2 flatInput;
        private bool jump = false;
        private bool run = false;

        private void Awake()
        {
            Player.Input.actions["Jump"].performed += ctx =>
            {
                if (IsGrounded)
                    jump = true;
            };
            Player.Input.actions["Run"].started += ctx =>
            {
                if(Player.Input.IsKeyboardAndMouse())
                    run = true;
            };
            Player.Input.actions["Run"].canceled += ctx =>
            {
                if(Player.Input.IsKeyboardAndMouse())
                    run = false;
            };
            Player.Input.actions["Run"].performed += ctx =>
            {
                if (Player.Input.IsKeyboardAndMouse())
                    return;
                run = !run;
            };
        }

        private void Update()
        {
            // Disable default gravity.
            Body.useGravity = false;

            // Poll input.
            flatInput = Player.Input.actions["Move"].ReadValue<Vector2>();
            if (Player.Input.IsKeyboardAndMouse())
            {
                flatInput.Normalize();
            }
            else
            {
                if (!IsRunning)
                {
                    if (flatInput.sqrMagnitude > 1f)
                        flatInput.Normalize();
                }
                else
                {
                    flatInput.Normalize();
                }
            }
        }

        private void FixedAnimationUpdate()
        {
            IsGrounded = false;
            Body.position = OverridePosition;
        }

        private void FixedUpdate()
        {
            // Set kinematic state.
            bool kinematic = Override;
            if (kinematic != Body.isKinematic)
                Body.isKinematic = kinematic;

            // Go no further if kinematic (animation / cutscene control)
            if (kinematic)
            {
                FixedAnimationUpdate();
                return;
            }

            // Add custom gravity.
            Body.AddForce(Physics.gravity * GravityScale * Body.mass);

            // Add flat input.
            Vector3 localFlat = new Vector3(flatInput.x, 0f, flatInput.y);
            Vector3 worldFlat = transform.TransformVector(localFlat);

            // Add flat (WASD) movement force.
            Body.AddForce(worldFlat * (IsRunning ? AccelerateForceRun : AccelerateForce));

            // Counteract movement to reduce sliding.
            Vector3 dragVel = -Body.velocity;
            dragVel.y = 0f;
            Body.AddForce(dragVel * DecelerateCoefficient);

            // Jump!
            if (jump)
            {
                jump = false;
                Body.AddForce(-Physics.gravity.normalized * JumpVel, ForceMode.VelocityChange);
                if (OnJump != null)
                    OnJump.Invoke();
            }

            IsGrounded = false;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (Override)
                return;

            Vector3 up = -Physics.gravity.normalized;

            // Find out what value to compare our 'flatness' to. Evaluates to 1 when 0deg, 0 when 90deg.
            float minFlatness = 1f - Mathf.Sin(Mathf.Deg2Rad * MaxGroundAngle);

            for (int i = 0; i < collision.contactCount; i++)
            {
                var c = collision.GetContact(i);
                Debug.DrawLine(c.point, c.point + c.normal * 0.3f, Color.red);

                // Check if it is facing up, for grounding...
                float flatness = Vector3.Dot(up, c.normal);
                if (flatness < 0f)
                    continue; // Means we collided with the ceiling or a downwards-facing ramp.

                if(flatness >= minFlatness)
                {
                    // This is floor! We are grounded!
                    IsGrounded = true;
                }
            }
        }
    }
}