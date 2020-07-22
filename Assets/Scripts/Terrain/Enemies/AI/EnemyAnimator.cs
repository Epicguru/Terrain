using UnityEngine;

namespace Terrain.Enemies.AI
{
    public class EnemyAnimator : MonoBehaviour
    {
        public Enemy Enemy
        {
            get
            {
                if (_enemy == null)
                    _enemy = GetComponent<Enemy>();
                return _enemy;
            }
        }
        private Enemy _enemy;

        public Animator Animator
        {
            get
            {
                if (_anim == null)
                    _anim = GetComponent<Animator>();
                return _anim;
            }
        }
        private Animator _anim;

        private void Update()
        {
            Vector3 rawVel = Enemy.Navigation.Velocity;
            Vector3 vel = rawVel.normalized;

            Vector2 flatMovement = new Vector2(vel.x, vel.z).normalized;

            Vector2 flatFacingDir = new Vector2(transform.forward.x, transform.forward.z).normalized;
            Vector2 flatRightDir = new Vector2(transform.right.x, transform.right.z).normalized;

            float forwardsAmount = Vector2.Dot(flatMovement, flatFacingDir); // A dot of 1 means forwards, -1 means backwards.
            float rightAmount = Vector2.Dot(flatMovement, flatRightDir); // A dot of 1 means right, -1 means left.

            float weight = Mathf.Clamp01(rawVel.magnitude / 0.4f);

            Animator.SetLayerWeight(1, weight);
            Animator.SetFloat("MoveX", rightAmount);
            Animator.SetFloat("MoveY", forwardsAmount);

            Debug.DrawLine(transform.position, transform.position + new Vector3(flatMovement.x, 0f, flatMovement.y), Color.green);
            Debug.DrawLine(transform.position, transform.position + new Vector3(flatFacingDir.x, 0f, flatFacingDir.y), Color.blue);
            Debug.DrawLine(transform.position, transform.position + new Vector3(flatRightDir.x, 0f, flatRightDir.y), Color.red);
        }
    }
}
