using UnityEngine;

namespace Terrain.Enemies.AI
{
    public class RagdollController : MonoBehaviour
    {
        public Health Health;

        private void Awake()
        {
            if (Health == null)
            {
                Debug.LogError($"Health reference for this RagdollController is null (in awake, {gameObject.name})");
                return;
            }
            Health.UponDeath.AddListener(UponDeath);
        }

        private void UponDeath(Health health)
        {

        }
    }
}
