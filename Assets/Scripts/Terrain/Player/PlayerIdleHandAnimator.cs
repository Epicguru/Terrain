
using UnityEngine;

namespace Terrain.Player
{
    public class PlayerIdleHandAnimator : MonoBehaviour
    {
        // URGTODO fix animation parameter names since Unity is buggy shit.
        public PlayerMovement Movement;
        public Transform CameraOffset;
        public Transform PlayerOffset;

        public Animator Anim;

        private void Awake()
        {
            Movement.OnJump += () => { Anim.SetTrigger("New Trigger"); };
        }

        private void Update()
        {
            Anim.SetBool("New Bool", Movement.IsRunning);        
        }
    }
}