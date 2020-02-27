
using UnityEngine;

// Handles the physics of items when they are dropped in the world.
namespace Terrain.Items
{
    public partial class Item
    {
        public static float WallCollisionResolveForce = 2f;
        private static SphereCollider Collider
        {
            get
            {
                if(_coll == null)
                {
                    var go = new GameObject("Item Physics Collider");
                    go.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector;
                    _coll = go.AddComponent<SphereCollider>();
                    _coll.isTrigger = true;
                }
                return _coll;
            }
        }
        private static SphereCollider _coll;
        private static Collider[] _colliders = new Collider[32];

        public PhysicsSettings Physics = new PhysicsSettings()
        {
            Radius = 0.5f,
            IsSleeping = false,
            Velocity = Vector3.zero,
            LayerMask = 1,
            SleepVelocityThreashold = 0.07f,
            DrawDebug = false
        };

        [System.Serializable]
        public struct PhysicsSettings
        {
            [Range(0.1f, 2f)]
            public float Radius;
            public bool DrawDebug;
            [MyBox.ReadOnly]
            public bool IsSleeping;
            public Vector3 Velocity;
            public LayerMask LayerMask;
            public float SleepVelocityThreashold;
            [MyBox.ReadOnly]
            public Vector3 SleepPosition;
            [HideInInspector]
            public float sleepTimer;

            public void SetIsSleeping(bool sleepFlag) { IsSleeping = sleepFlag; }
            public bool ShouldBeSleeping() { return Velocity.sqrMagnitude <= SleepVelocityThreashold * SleepVelocityThreashold; }
        }

        private void LateUpdate()
        {
            bool shouldSleep = Physics.ShouldBeSleeping() && IsGrounded();
            if (shouldSleep)
            {
                Physics.sleepTimer += Time.unscaledDeltaTime;
                if (Physics.sleepTimer < 0.5f)
                    shouldSleep = false;
            }
            if(shouldSleep != Physics.IsSleeping)
            {
                if (shouldSleep)
                    Physics.SleepPosition = transform.position;
                Physics.IsSleeping = shouldSleep;
            }
            if(shouldSleep)
            {
                if(Physics.SleepPosition != transform.position)
                {
                    Physics.sleepTimer = 0f;
                }                
            }

            // Only update physics if not sleeping, and dropped.
            if (Physics.IsSleeping || State != ItemState.Dropped)
                return;

            // Add gravity to velocity.
            Physics.Velocity += UnityEngine.Physics.gravity * Time.deltaTime;

            Vector3 start = transform.position;
            Vector3 end = start + Physics.Velocity * Time.deltaTime;
            Vector3 direction = end - start;
            float distance = direction.magnitude;

            int count = UnityEngine.Physics.OverlapSphereNonAlloc(transform.position, Physics.Radius, _colliders, Physics.LayerMask, QueryTriggerInteraction.Ignore);
            if(count != 0)
            {
                // NOTE: I should actually resolve each and every collision, but hopefully solving the first one alone will do.
                var collider = _colliders[0];
                Collider.radius = Physics.Radius;
                bool didReallyIntersect = UnityEngine.Physics.ComputePenetration(Collider, transform.position, Quaternion.identity, collider, collider.transform.position, collider.transform.rotation, out Vector3 solveDir, out float solveLength);
                if (didReallyIntersect)
                {
                    transform.position += solveDir * (solveLength + 0.05f);
                    Physics.Velocity = WallCollisionResolveForce * solveLength * solveDir;
                    return;
                }
            }

            // Perform a sphere cast from current position to the position we will be in next frame.
            bool didHit = UnityEngine.Physics.SphereCast(new Ray(start, direction), Physics.Radius, out RaycastHit hit, distance, Physics.LayerMask, QueryTriggerInteraction.Ignore);

            // If we didn't hit anything with the sphere cast, then just move to the new position and stop there.
            if (!didHit)
            {
                transform.position = end;
                return;
            }

            // We did hit something with out sphere cast.
            Debug.DrawLine(hit.point, hit.point + hit.normal * 0.5f, Color.red);

            // Work out new final position: the position at which we collide with the surface without penetrating it.
            Vector3 finalPos = start + direction.normalized * (hit.distance - 0.01f);

            // Write position.
            transform.position = finalPos;

            // Also need to change velocity, reflect around the collision normal.
            Vector3 incoming = Physics.Velocity;
            Vector3 outgoing = Vector3.Reflect(incoming, hit.normal) * 0.7f; // This preserves magnitude, so it bounces at the same speed it came in.

            // Write back velocity.
            Physics.Velocity = outgoing;
        }

        private bool IsGrounded()
        {
            return UnityEngine.Physics.Raycast(new Ray(transform.position, UnityEngine.Physics.gravity), Physics.Radius + 0.05f, Physics.LayerMask, QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmos()
        {
            if (Physics.DrawDebug)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, Physics.Radius);
            }
        }
    }
}
