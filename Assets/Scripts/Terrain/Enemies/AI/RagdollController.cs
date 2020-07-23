using Boo.Lang;
using UnityEngine;

namespace Terrain.Enemies.AI
{
    public class RagdollController : MonoBehaviour
    {
        public Rigidbody[] BodyParts;

        public List<Collider> RagdollColliders = new List<Collider>();
        public List<Collider> HitboxColliders = new List<Collider>();

        public Health Health;
        public Animator Anim;

        private void Awake()
        {
            if (Health == null)
            {
                Debug.LogError($"Health reference for this RagdollController is null (in awake, {gameObject.name})");
                return;
            }
            Health.UponDeath += UponDeath;

            BodyParts = GetComponentsInChildren<Rigidbody>(true);
            foreach (var part in BodyParts)
            {
                part.isKinematic = true;
                var col = part.GetComponent<Collider>();
                Debug.Assert(col != null);
                RagdollColliders.Add(col);
                col.enabled = false;
            }
            foreach(var col in transform.GetComponentsInChildren<Collider>(true))
            {
                if (!RagdollColliders.Contains(col))
                    HitboxColliders.Add(col);
            }

            Debug.Log($"This object has {RagdollColliders.Count} ragdoll colliders and {HitboxColliders.Count} hitbox colliders.");
        }

        private void UponDeath(Health health)
        {
            if(Anim != null)
                Anim.enabled = false;

            SetState(true);
        }

        private void SetState(bool ragdoll)
        {
            foreach (var part in BodyParts)
            {
                part.isKinematic = !ragdoll;
            }
            foreach (var col in HitboxColliders)
                col.enabled = !ragdoll;
            foreach (var col in RagdollColliders)
                col.enabled = ragdoll;
        }
    }
}
