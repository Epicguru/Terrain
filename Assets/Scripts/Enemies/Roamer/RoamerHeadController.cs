
using UnityEngine;

public class RoamerHeadController : MonoBehaviour
{
    public Transform Head;
    public Transform Target;

    private void Update()
    {
        if (Head == null || Target == null)
            return;

        Vector3 offset = Head.position - Target.position;
        if (offset.sqrMagnitude < 0.001f)
            return;

        float angleYaw = Mathf.Atan2(offset.y, offset.x);
        float anglePitch = Mathf.Asin(offset.y / offset.magnitude);

        Head.eulerAngles = new Vector3(anglePitch, angleYaw, 0f) * Mathf.Rad2Deg;
    }
}
