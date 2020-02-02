
using UnityEngine;

[ExecuteInEditMode]
public class FollowTarget : MonoBehaviour
{
    public Transform Target;

    [Header("Settings")]
    public UpdateMode Mode = UpdateMode.LateUpdate;
    public Vector3 Offset;
    public bool MatchRotation = false;
    public bool RunInEditMode = false;

    private bool isArm;
    private LimbIK arm;

    public enum UpdateMode
    {
        Update,
        LateUpdate,
        FixedUpdate
    }

    private void Awake()
    {
        CheckArm();
    }

    private void CheckArm()
    {
        if (arm != null)
            return;

        arm = GetComponentInParent<LimbIK>();
        isArm = arm != null;
    }

    private void Update()
    {
        if (Mode == UpdateMode.Update)
            MoveToTarget();
    }

    private void LateUpdate()
    {
        if (Mode == UpdateMode.LateUpdate)
            MoveToTarget();
    }

    private void FixedUpdate()
    {
        if (Mode == UpdateMode.FixedUpdate)
            MoveToTarget();
    }

    private void MoveToTarget()
    {
        if (Target == null)
            return;
        if (!Application.isPlaying && !RunInEditMode)
            return;
        if (!Application.isPlaying)
            CheckArm();

        Vector3 finalPos = Target.position + Offset;

        if (isArm)
        {
            if (Target.gameObject.activeInHierarchy)
            {
                arm.ElbowOffset = Target.localEulerAngles.z;
            }
            else
            {
                finalPos = arm.transform.position + Vector3.down * 2f;
                arm.ElbowOffset = 0f;
            }
        }

        transform.position = finalPos;
        if (MatchRotation)
        {
            transform.rotation = Target.rotation;
        }
    }
}

