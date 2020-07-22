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
    }
}
