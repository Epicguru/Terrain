
using UnityEngine;

public static class Extensions
{
    #region Vectors

    public static Vector3 ReflectAdvanced(this Vector3 inputVector, Vector3 normal, float normalAxisScalar, float otherAxisScalar)
    {
        Vector3 HitPoint = Vector3.zero;

        Vector3 reflectionRaw = inputVector - 2f * normal * Vector3.Dot(normal, inputVector);
        reflectionRaw *= otherAxisScalar;

        Vector3 proj = ProjectOntoPlane(HitPoint + reflectionRaw, normal, HitPoint);

        float finalHeight = reflectionRaw.GetAxisLength(normal) * (normalAxisScalar / otherAxisScalar);
        Vector3 newPlanePoint = HitPoint + normal * finalHeight;

        Vector3 proj2 = proj.ProjectOntoPlane(normal, newPlanePoint);

        Vector3 finalVector = proj2 - HitPoint;

        return finalVector;
    }

    public static Vector3 ProjectOntoPlane(this Vector3 point, Vector3 planeNormal, Vector3 planePoint)
    {
        Vector3 v = point - planePoint;
        float dst = Vector3.Dot(v, planeNormal);
        Vector3 projected = point - dst * planeNormal;

        return projected;
    }

    public static float GetAxisLength(this Vector3 vector, Vector3 axis)
    {
        return Vector3.Dot(vector.normalized, axis.normalized) * vector.magnitude;
    }

    #endregion
}
