using UnityEngine;

public class VectorsTest : MonoBehaviour
{
    public bool AutoNormal;

    public Vector3 IncomingVector;
    public Vector3 HitPoint;
    public Vector3 Normal;

    public float NormalAxisMultiplier = 0.5f;
    public float OtherAxisMultiplier = 1f;

    private void OnDrawGizmos()
    {
        if (AutoNormal)
        {
            Normal = transform.up;
            HitPoint = transform.position;
        }

        if (Normal.sqrMagnitude != 1f)
            Normal.Normalize();

        Gizmos.color = Color.green;
        Gizmos.DrawLine(HitPoint, HitPoint - IncomingVector);

        Gizmos.color = Color.grey;
        Gizmos.DrawLine(HitPoint, HitPoint + Normal * 0.5f);

        Vector3 reflectionRaw = IncomingVector - 2f * Normal * Vector3.Dot(Normal, IncomingVector);
        reflectionRaw *= OtherAxisMultiplier;

        Vector3 proj = ProjectOntoPlane(HitPoint + reflectionRaw, Normal, HitPoint);

        float finalHeight = GetAxisLength(reflectionRaw, Normal) * (NormalAxisMultiplier / OtherAxisMultiplier);
        Vector3 newPlanePoint = HitPoint + Normal * finalHeight;

        Vector3 proj2 = ProjectOntoPlane(proj, Normal, newPlanePoint);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(HitPoint, HitPoint + reflectionRaw);
        Gizmos.DrawWireCube(proj, Vector3.one * 0.02f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(proj2, Vector3.one * 0.03f);

        Vector3 finalVector = proj2 - HitPoint;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(HitPoint, HitPoint + finalVector);

        Debug.Assert(IncomingVector.ReflectAdvanced(Normal, NormalAxisMultiplier, OtherAxisMultiplier) == finalVector);
    }

    public Vector3 ProjectOntoPlane(Vector3 point, Vector3 planeNormal, Vector3 planePoint)
    {
        Vector3 v = point - planePoint;
        float dst = Vector3.Dot(v, planeNormal);
        Vector3 projected = point - dst * planeNormal;

        return projected;
    }

    public float GetAxisLength(Vector3 vector, Vector3 axis)
    {
        return Vector3.Dot(vector.normalized, axis.normalized) * vector.magnitude;
    }
}
