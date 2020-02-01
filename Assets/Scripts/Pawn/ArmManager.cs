
using UnityEngine;

[ExecuteInEditMode]
public class ArmManager : MonoBehaviour
{
    public Pawn Pawn
    {
        get
        {
            if (_pawn == null)
                _pawn = GetComponent<Pawn>();
            return _pawn;
        }
    }
    private Pawn _pawn;

    public ArmIK LeftArm, RightArm;
    public Transform IdleLeft, IdleRight;

    [Header("Transition")]
    [Range(0f, 10f)]
    public float TransitionTime = 1f;
    public AnimationCurve TransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float timer;
    private (Vector3 pos, float rot) lastPosRight, lastPosLeft;

    private void Update()
    {
        var im = Pawn.ItemManager;
        var item = im.CurrentItem;

        if(item != null)
        {
            var left = item.LeftHandPos;
            var right = item.RightHandPos;

            Vector3 finalRight = (right == null || !right.gameObject.activeInHierarchy) ? RightArm.transform.position + Vector3.down * 2f : right.position;
            Vector3 finalLeft = (left == null || !left.gameObject.activeInHierarchy) ? LeftArm.transform.position + Vector3.down * 2f : left.position;

            float finalRotRight = right == null ? 0f : right.localEulerAngles.z;
            float finalRotLeft = left == null ? 0f : left.localEulerAngles.z;

            RightArm.TargetPosition = finalRight;
            RightArm.ElbowOffset = finalRotRight;
            lastPosRight = (finalRight, finalRotRight);

            LeftArm.TargetPosition = finalLeft;
            LeftArm.ElbowOffset = finalRotLeft;
            lastPosLeft = (finalLeft, finalRotLeft);

            timer = 0f;
        }
        else
        {
            timer += Time.deltaTime;
            if (timer > TransitionTime)
                timer = TransitionTime;

            float p = TransitionTime <= 0f ? 1f : (timer / TransitionTime);
            float x = TransitionCurve.Evaluate(p);

            // Where 0 is item, 1 is idle pos.
            Vector3 finalRight = Vector3.Lerp(lastPosRight.pos, IdleRight.position, x);
            float finalRotRight = Mathf.Lerp(lastPosRight.rot, IdleRight.localEulerAngles.z, x);

            Vector3 finalLeft = Vector3.Lerp(lastPosLeft.pos, IdleLeft.position, x);
            float finalRotLeft = Mathf.Lerp(lastPosLeft.rot, IdleLeft.localEulerAngles.z, x);

            RightArm.TargetPosition = finalRight;
            RightArm.ElbowOffset = finalRotRight;

            LeftArm.TargetPosition = finalLeft;
            LeftArm.ElbowOffset = finalRotLeft;
        }
    }
}