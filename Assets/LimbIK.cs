using UnityEngine;

[ExecuteInEditMode]
public class LimbIK : MonoBehaviour
{
    [Header("References")]
    public Transform TopBone;
    public Transform LowerBone;
    public Transform BoneDir;

    [Header("Runtime")]
    public bool InvertedMode = false;
    public Vector3 TargetPosition;
    public float ElbowOffset;

    [Header("Settings")]
    public float TopBoneLength = 1f;
    public float LowerBoneLength = 1f;

    public float MaxReach { get { return TopBoneLength + LowerBoneLength; } }
    public float MinReach { get { return Mathf.Abs(LowerBoneLength - TopBoneLength); } }

    public bool IsOutOfBounds { get; private set; }
    public float CurrentExtension { get; private set; }

    private void LateUpdate()
    {
        Vector3 targetPos = TargetPosition;
        Vector3 basePosition = TopBone.position;
        Vector3 offset = (targetPos - basePosition);
        float minDistance = MinReach;
        float maxDistance = MaxReach;
        float offsetMag = offset.magnitude;
        IsOutOfBounds = false;
        if (offsetMag > maxDistance)
        {
            targetPos = basePosition + offset.normalized * (maxDistance - 0.0001f);
            IsOutOfBounds = true;
        }
        if(offsetMag < minDistance)
        {
            targetPos = basePosition + offset.normalized * (minDistance + 0.0001f);
            IsOutOfBounds = true;
        }

        float d = (targetPos - basePosition).magnitude;
        CurrentExtension = d;
        float x = TopBoneLength;
        float y = LowerBoneLength;
        float x2 = x * x;
        float y2 = y * y;
        float d2 = d * d;

        float alpha = Mathf.Acos((y2 - x2 - d2) / (2 * x * d)) * Mathf.Rad2Deg;
        float beta = Mathf.Acos((d2 - x2 - y2) / (2 * x * y)) * Mathf.Rad2Deg;

        // Rotate both joints to achieve target distance.
        TopBone.localEulerAngles = new Vector3((alpha - 180f) * (InvertedMode ? -1f : 1f), 0f, 0f);
        LowerBone.localEulerAngles = new Vector3(beta * (InvertedMode ? -1f : 1f), 0f, 0f);
        
        // Now the arm is extended to the correct length, point it towards the target.
        BoneDir.LookAt(targetPos, -transform.up);

        // Make up point forwards.
        BoneDir.Rotate(90f, 0f, 0f);

        if (InvertedMode)
        {
            // Make down point forwards (it makes no sense, just accept it).
            BoneDir.Rotate(180, 0f, 0f);
        }

        // Elbow offset (rotating arm so that elbow is below, out to the side etc.)
        BoneDir.RotateAround(BoneDir.position, (targetPos - BoneDir.position), ElbowOffset);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsOutOfBounds ? Color.red : Color.green;
        Gizmos.DrawCube(TargetPosition, Vector3.one * 0.03f);
    }
}
