
using UnityEngine;

[RequireComponent(typeof(PoolObject))]
public class Projectile : MonoBehaviour
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

    public LayerMask CollisionMask;
    public Vector3 Velocity;
    public float GravityScale = 1f;
    public float MaxTime = 10f;

    private float timer;

    private void Update()
    {
        Step();
    }

    /// <summary>
    /// Called once per frame. Should be used to adjust the projectile's position and velocity.
    /// </summary>
    protected virtual void Step()
    {
        Vector3 start = transform.position;
        Vector3 end = start + Velocity * Time.deltaTime;
        Vector3 final = DetectCollision(start, end, out RaycastHit hit);

        transform.position = final;

        if(hit.collider != null)
        {
            // Hit a wall or something.
            PoolObject.Despawn();
            Debug.DrawLine(hit.point, hit.point + hit.normal * 0.5f, Color.green, 5f);
        }

        // Add gravity to projectile.
        Velocity += Physics.gravity * GravityScale * Time.deltaTime;

        // Time out the projectile if it has been travelling for too long.
        timer += Time.deltaTime;
        if(timer >= MaxTime)
        {
            PoolObject.Despawn();
        }
    }

    /// <summary>
    /// Should return a position along the start-end line if any collisions occur when travelling between the two.
    /// Default implementation is a linecast that ignores trigger colliders and obeys the <see cref="CollisionMask"/> value.
    /// </summary>
    /// <param name="start">The start position in world space.</param>
    /// <param name="end">The end position in world space.</param>
    /// <returns>A position along the start-end line, returning the end position if no collision occurs.</returns>
    protected virtual Vector3 DetectCollision(Vector3 start, Vector3 end, out RaycastHit hit)
    {
        hit = default;

        bool didHit = Physics.Linecast(start, end, out hit, CollisionMask, QueryTriggerInteraction.Ignore);
        if (didHit)
            return hit.point;
        else
            return end;
    }
}
