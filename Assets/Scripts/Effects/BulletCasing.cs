
using UnityEngine;

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
    public float BounceVelocityMultiplier = 0.8f;
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
            Vector3 newVel = Vector3.Reflect(Velocity, hit.normal) * BounceVelocityMultiplier;

            // Assign new velocity.
            Velocity = newVel;

            // Make sure that we move to collision point.
            next = hit.point + hit.normal * 0.001f;
        }

        transform.position = next;

        // Update rotation. Rotation does not add or influence collision or speed.
        float angularVelScale = Mathf.Clamp01(Velocity.magnitude / 0.1f); // Once the casing starts moving slower than 10cm/s, the rotation starts to slow down.        
        transform.localEulerAngles += AngularVelocity * angularVelScale * Time.deltaTime;

        // Add gravity.
        Velocity += Physics.gravity * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= Lifespan)
        {
            PoolObject.Despawn();
        }
    }
}