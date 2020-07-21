using UnityEngine;

namespace Terrain.Enemies.AI.Melee
{
    public class EnemyMelee : MonoBehaviour
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

        public Animator TopAnim
        {
            get
            {
                return Enemy.Animator.TopAnimator;
            }
        }
        public Animator LegAnim
        {
            get
            {
                return Enemy.Animator.LegAnimator;
            }
        }

        public float TimeSinceLastAttack { get; private set; }

        private void Update()
        {
            Enemy.Animator.UseMeleeStance = true;

            if (!Enemy.Navigation.IsMoving && TimeSinceLastAttack >= 1.5f)
            {
                TriggerAttack(Random.Range(0, 2));
            }

            TimeSinceLastAttack += Time.deltaTime;
        }

        private void TriggerAttack(int index)
        {
            TopAnim.SetInteger("AttackInt", index);
            TopAnim.SetTrigger("Attack");
            TimeSinceLastAttack = 0f;
        }

        private void UponAnimationEvent(AnimationEvent e)
        {
            switch (e.stringParameter.ToLowerInvariant().Trim())
            {
                case "lunge":
                    LegAnim.SetTrigger("Melee Lunge");
                    break;
            }
        }
    }
}
