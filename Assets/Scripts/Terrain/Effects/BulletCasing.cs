
using Terrain.Utils;
using UnityEngine;

namespace Terrain.Effects
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PoolObject))]
    public class BulletCasing : MonoBehaviour
    {
        public PoolObject PoolObject
        {
            get
            {
                if (_po == null)
                    _po = GetComponent<PoolObject>();
                return _po;
            }
        }
        private PoolObject _po;

        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        [Range(0f, 1f)]
        public float BounceVelocityMultiplier = 0.5f;
        [Range(0f, 1f)]
        public float BounceFlatVelocityMultiplier = 0.8f;
        public float Lifespan = 10f;

        private float timer;

        private void UponSpawn()
        {
            timer = 0f;
        }

        private void Update()
        {
            Vector3 currentPos = transform.position;
            Vector3 next = currentPos + Velocity * Time.deltaTime;

            // Check collision between here and the next pos, if we are moving.

            const float MIN_SPEED = 0.005f; // 0.5cm/s
            if (Velocity.sqrMagnitude >= MIN_SPEED * MIN_SPEED && Physics.Linecast(currentPos, next, out RaycastHit hit))
            {
                Vector3 newVel = Velocity.ReflectAdvanced(hit.normal, BounceVelocityMultiplier, BounceFlatVelocityMultiplier);

                // Assign new velocity.
                Velocity = newVel;

                // Make sure that we move to collision point.
                next = hit.point + hit.normal * 0.001f;
            }

            transform.position = next;

            // Update rotation. Rotation does not add or influence collision or speed.
            transform.localEulerAngles += AngularVelocity * Time.deltaTime;

            // Add gravity.
            Velocity += Physics.gravity * Time.deltaTime;

            timer += Time.deltaTime;
            if (timer >= Lifespan)
            {
                PoolObject.Despawn();
            }
        }
    }
}