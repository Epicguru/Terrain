
using UnityEngine;

[ExecuteInEditMode]
public class AutoIK : MonoBehaviour
{
    public LimbIK IK;
    public bool ExecuteInEditor = true;

    [Header("Raycasting")]
    public bool RaycastMode = false;
    [Range(0f, 10f)]
    public float RaycastDistance = 0.3f;
    public Vector3 RaycastStartoffset = new Vector3(0f, 0f, 0f);
    public Vector3 RaycastDirection = new Vector3(0f, -1f, 0f);

    private bool raycastDidHit;

    private void Update()
    {
        if (IK == null)
            return;

        if (!ExecuteInEditor && !Application.isPlaying)
            return;

        Vector3 offset = Vector3.zero;
        if (RaycastMode)
        {
            raycastDidHit = Physics.Raycast(new Ray(transform.TransformPoint(RaycastStartoffset), transform.TransformDirection(RaycastDirection)), out RaycastHit hit, (RaycastDistance <= 0f ? float.MaxValue : RaycastDistance));
            if (raycastDidHit)
            {
                float height = hit.point.y;
                float diff = height - transform.position.y;

                offset.y = diff;
            }
        }

        IK.TargetPosition = transform.position + offset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = raycastDidHit ? Color.green : Color.red;
        Gizmos.DrawLine(transform.TransformPoint(RaycastStartoffset), transform.TransformPoint(RaycastStartoffset) + transform.TransformDirection(RaycastDirection) * (RaycastDistance <= 0f ? 1f : RaycastDistance));
    }
}
