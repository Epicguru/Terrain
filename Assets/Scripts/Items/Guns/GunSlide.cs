
using UnityEngine;

[ExecuteInEditMode]
public class GunSlide : MonoBehaviour
{
    [Header("Animation")]
    [Range(0f, 2f)]
    [SerializeField]
    private float Lerp = 0f;
    [SerializeField]
    private bool AnimOverride = false;

    [Header("Settings")]
    [SerializeField]
    private Transform Target;
    [SerializeField]
    private Vector3 StartPos, EndPos;

    // If override is true, then the animation lerp value is not used UNLESS AnimOverride is true.
    [HideInInspector]
    public bool Override = false;
    [HideInInspector]
    public float OverrideLerp = 1f;

    private void LateUpdate()
    {
        float lerp = Lerp;
        if (Override && !AnimOverride)
            lerp = OverrideLerp;

        Target.localPosition = Vector3.LerpUnclamped(StartPos, EndPos, lerp);
    }
}
