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
        public Animator Anim
        {
            get
            {
                return Enemy.Animator.Animator;
            }
        }

        public Vector2 TimeBetweenSlashes = new Vector2(0.7f, 1.2f);

        private float slashDelay;
        private float timeSinceLastSlash;
        private int lastSlashVariant;

        private void Start()
        {
            slashDelay = Mathf.Lerp(TimeBetweenSlashes.x, TimeBetweenSlashes.y, Random.value);
        }

        private void Update()
        {
            timeSinceLastSlash += Time.deltaTime;
            if (timeSinceLastSlash >= slashDelay)
            {
                bool slashCondition = Enemy.Navigation.DistanceToTarget <= 0.5f;
                if (slashCondition)
                {
                    // Choose variant.
                    int variant = lastSlashVariant;
                    while (variant == lastSlashVariant)
                    {
                        variant = Random.Range(0, 3);
                    }

                    // Cause attack animation.
                    TriggerAttack(variant);

                    // Update variables.
                    timeSinceLastSlash = 0f;
                    lastSlashVariant = variant;
                    slashDelay = Mathf.Lerp(TimeBetweenSlashes.x, TimeBetweenSlashes.y, Random.value);
                }
            }
        }

        private void TriggerAttack(int variant)
        {
            Anim.SetInteger("Slash Variant", variant);
            Anim.SetTrigger("Slash");
        }
    }
}
