
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NewPlayerMovement : MonoBehaviour
{
    public Pawn Pawn
    {
        get
        {
            if (_pawn == null)
                _pawn = GetComponent<Pawn>();
            return _pawn;
        }
    }
    private Pawn _pawn;
    public Rigidbody Body
    {
        get
        {
            if (_body == null)
                _body = GetComponent<Rigidbody>();
            return _body;
        }
    }
    private Rigidbody _body;

    public float GravityScale = 1f;
    public float AccelerateForce = 500f;
    public float DecelerateCoefficient = 50f;
    [Header("Grounding")]
    public bool IsGrounded = false;
    [Range(0f, 90f)]
    public float MaxGroundAngle = 50f;

    [Header("Jumping")]
    public float JumpVel = 8f;

    private Vector2 flatInputRaw;
    private bool jump = false;

    private void Update()
    {
        // Disable default gravity.
        Body.useGravity = false;        

        // Colect flat input.
        flatInputRaw = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            flatInputRaw.x -= 1f;
        if (Input.GetKey(KeyCode.D))
            flatInputRaw.x += 1f;
        if (Input.GetKey(KeyCode.S))
            flatInputRaw.y -= 1f;
        if (Input.GetKey(KeyCode.W))
            flatInputRaw.y += 1f;

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
            jump = true;
    }

    private void FixedUpdate()
    {
        // Add custom gravity.
        Body.AddForce(Physics.gravity * GravityScale * Body.mass); // F = ma

        // Add flat input.
        flatInputRaw.Normalize();
        Vector3 localFlat = new Vector3(flatInputRaw.x, 0f, flatInputRaw.y);
        Vector3 worldFlat = transform.TransformDirection(localFlat);

        // Add flat (WASD) movement force.
        Body.AddForce(worldFlat * AccelerateForce);

        // Counteract movement to reduce sliding.
        Vector3 dragVel = -Body.velocity;
        dragVel.y = 0f;
        Body.AddForce(dragVel * DecelerateCoefficient);

        if (jump)
        {
            jump = false;
            Body.AddForce(-Physics.gravity.normalized * JumpVel, ForceMode.VelocityChange);
        }

        IsGrounded = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        Vector3 up = -Physics.gravity.normalized;

        // Find out what value to compare our 'flatness' to. Evaluates to 1 when 0deg, 0 when 90deg.
        float minFlatness = 1f - Mathf.Sin(Mathf.Deg2Rad * MaxGroundAngle);

        for (int i = 0; i < collision.contactCount; i++)
        {
            var c = collision.GetContact(i);
            Debug.DrawLine(c.point, c.point + c.normal * 0.3f, Color.red);

            // Check if it is facing up, for grounding...
            float flatness = Vector3.Dot(up, c.normal);
            if (flatness < 0f)
                continue; // Means we collided with the ceiling or a downwards-facing ramp.

            if(flatness >= minFlatness)
            {
                // This is floor! We are grounded!
                IsGrounded = true;
            }
        }
    }
}