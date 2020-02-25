
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

    #region Animation

    public static bool HasParameter(this Animator anim, string name, AnimatorControllerParameterType type)
    {
        foreach (var param in anim.parameters)
        {
            if (param.name.Equals(name) && param.type == type)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Item Rarity

    public static Color GetColor(this ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return new Color(0.6132076f, 0.6132076f, 0.6132076f, 1f);

            case ItemRarity.Uncommon:
                return new Color(0.3224012f, 0.990566f, 0.3663174f, 1f);

            case ItemRarity.Rare:
                return new Color(0.2548505f, 0.3988582f, 0.7830189f, 1f);

            case ItemRarity.UltraRare:
                return new Color(1f, 0.3537736f, 0.8362778f, 1f);

            case ItemRarity.Legendary:
                return new Color(0.9736655f, 1f, 0.0990566f, 1f);

            default:
                return Color.white;                
        }
    }

    #endregion
}
