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

        public Animator LegAnimator;
        public Animator TopAnimator;

        public bool UseMeleeStance { get; set; }

        private float meleeStanceLerp;

        private void Update()
        {
            const float MELEE_STANCE_LERP_TIME = 0.2f;
            meleeStanceLerp = Mathf.Clamp01(meleeStanceLerp + Time.deltaTime / MELEE_STANCE_LERP_TIME * (UseMeleeStance ? 1f : -1f));

            LegAnimator.SetBool("Run", Enemy.Navigation.IsMoving);
            LegAnimator.SetLayerWeight(1, meleeStanceLerp);

        }
    }
}
