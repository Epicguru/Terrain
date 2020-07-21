using UnityEngine;

namespace Terrain.Enemies.AI
{
    [RequireComponent(typeof(EnemyNavigation))]
    [RequireComponent(typeof(EnemyAnimator))]
    public class Enemy : MonoBehaviour
    {
        public EnemyNavigation Navigation
        {
            get
            {
                if (_nav == null)
                    _nav = GetComponent<EnemyNavigation>();
                return _nav;
            }
        }
        private EnemyNavigation _nav;
        public EnemyAnimator Animator
        {
            get
            {
                if (_anim == null)
                    _anim = GetComponent<EnemyAnimator>();
                return _anim;
            }
        }
        private EnemyAnimator _anim;
    }
}
