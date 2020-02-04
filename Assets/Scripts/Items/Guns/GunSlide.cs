
using UnityEngine;

[ExecuteInEditMode]
public class GunSlide : MonoBehaviour
{
    [Header("Animation")]
    [Range(0f, 2f)]
    public float Lerp = 0f;
    public bool AnimOverride = false;

    [Header("Settings")]
    public Transform Target;
    public Vector3 StartPos, EndPos;

    // If override is true, then the animation lerp value is not used UNLESS AnimOverride is true.
    public bool Override { get; set; } = false;
    public float OverrideLerp { get; set; } = 1f;

    private void LateUpdate()
    {
        float lerp = Lerp;
        if (Override && !AnimOverride)
            lerp = OverrideLerp;

        Target.localPosition = Vector3.LerpUnclamped(StartPos, EndPos, lerp);
    }
}
