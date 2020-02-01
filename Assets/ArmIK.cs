using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ArmIK : MonoBehaviour
{
    [Header("References")]
    public Transform TopBone, LowerBone;
    public Transform BoneDir;

    [Header("Runtime")]
    public Vector3 TargetPosition;
    public float ElbowOffset;

    [Header("Settings")]
    public float TopBoneLength = 1f;
    public float LowerBoneLength = 1f;

    private bool isOutOfBounds = false;

    private void Update()
    {
        Vector3 targetPos = TargetPosition;
        Vector3 basePosition = TopBone.position;
        Vector3 offset = (targetPos - basePosition);
        float minDistance = Mathf.Abs(LowerBoneLength - TopBoneLength);
        float maxDistance = TopBoneLength + LowerBoneLength;
        float offsetMag = offset.magnitude;
        isOutOfBounds = false;
        if (offsetMag > maxDistance)
        {
            targetPos = basePosition + offset.normalized * (maxDistance - 0.0001f);
            isOutOfBounds = true;
        }
        if(offsetMag < minDistance)
        {
            targetPos = basePosition + offset.normalized * (minDistance + 0.0001f);
            isOutOfBounds = true;
        }

        float d = (targetPos - basePosition).magnitude;
        float x = TopBoneLength;
        float y = LowerBoneLength;
        float x2 = x * x;
        float y2 = y * y;
        float d2 = d * d;

        float alpha = Mathf.Acos((y2 - x2 - d2) / (2 * x * d)) * Mathf.Rad2Deg;
        float beta = Mathf.Acos((d2 - x2 - y2) / (2 * x * y)) * Mathf.Rad2Deg;

        // Rotate both joints to achieve target distance.
        TopBone.localEulerAngles = new Vector3(alpha - 180f, 0f, 0f);
        LowerBone.localEulerAngles = new Vector3(beta, 0f, 0f);
        
        // Now the arm is extended to the correct length, point it towards the target.
        BoneDir.LookAt(targetPos, -transform.up);
        BoneDir.Rotate(90f, 0f, 0f);

        // Elbow offset (rotating arm so that elbow is below, out to the side etc.)
        BoneDir.RotateAround(BoneDir.position, (targetPos - BoneDir.position), ElbowOffset);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isOutOfBounds ? Color.red : Color.green;
        Gizmos.DrawCube(TargetPosition, Vector3.one * 0.03f);
    }
}
