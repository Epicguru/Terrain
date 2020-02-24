
using UnityEngine;

public class InteractionManager : AnimationInjector
{
    public ItemManager ItemManager;
    public CameraLook CameraLook;
    public PlayerMovement Movement;
    public Animator HandAnim;
    public Transform CameraRotationAnim;

    public AnimationClip Clip;
    public InteractionAnchour Anchour;
    public AnimationCurve LerpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Min(0f)]
    public float LerpTime = 0.5f;

    public bool IsInInteraction { get; private set; }
    public InteractionAnchour CurrentAnchour { get; private set; }
    public float OverrideLerp { get; private set; }

    private Vector3 startPos;
    private Quaternion startRot;
    private float lerpTimer;
    private bool lerpEnded;
    private int lastEquippedItemIndex;

    protected override void Awake()
    {
        base.Animator = HandAnim;
        base.Awake();
    }

    private void OnLerpEnd()
    {
        // Play the actual animation.
        base.Animator.SetTrigger("New Trigger 1");
    }

    private void Update()
    {
        if(IsInInteraction && Anchour == null)
        {
            OnEndInteraction(true);
            return;
        }

        if (IsInInteraction)
        {
            if (LerpTime <= 0f)
            {
                OverrideLerp = 1f;
                if (!lerpEnded)
                {
                    lerpEnded = true;
                    OnLerpEnd();
                }
            }
            else
            {
                lerpTimer += Time.deltaTime;
                OverrideLerp = Mathf.Clamp01(lerpTimer / LerpTime);
                if (OverrideLerp == 1f && !lerpEnded)
                {
                    lerpEnded = true;
                    OnLerpEnd();
                }
            }
        }        

        CameraLook.Override = IsInInteraction;
        if (IsInInteraction)
        {
            Quaternion start = startRot;
            Quaternion end = Quaternion.LookRotation(Anchour.transform.TransformDirection(CameraRotationAnim.parent.InverseTransformDirection(CameraRotationAnim.forward)), -Physics.gravity);
            Quaternion lerped = Quaternion.Lerp(start, end, LerpCurve.Evaluate(OverrideLerp));

            CameraLook.OverrideTargetDirection = lerped * Vector3.forward;
        }
        
        Movement.Override = IsInInteraction;
        if (IsInInteraction)
        {
            Vector3 start = startPos;
            Vector3 end = Anchour.GetPlayerPosition();
            Vector3 lerped = Vector3.Lerp(start, end, LerpCurve.Evaluate(OverrideLerp));

            Movement.OverridePosition = lerped;
        }    
    }

    [MyBox.ButtonMethod]
    private void TestInteraction()
    {
        StartInteraction(Anchour, Clip, 1f);
    }

    public void StartInteraction(InteractionAnchour anchour, AnimationClip clip, float speed = 1f, bool autoExit = true)
    {
        if(clip == null)
        {
            Debug.LogError("Clip is null, interaction will not be started.");
            return;
        }
        if (IsInInteraction)
        {
            Debug.LogWarning("Already in interaction, please wait until starting new one.");
            return;
        }

        IsInInteraction = true;

        // Remove item from hands and don't allow the player to put an item back into their hands.
        lastEquippedItemIndex = ItemManager.ActiveItemIndex;
        ItemManager.SetActiveItem(-1);
        ItemManager.AllowSwapping = false;

        // Record starting position and camera rotation.
        startPos = Movement.transform.position;
        startRot = CameraLook.Camera.transform.rotation;

        // Reset lerp timer.
        lerpTimer = 0f;
        lerpEnded = false;

        // URGTODO fix animator parameter names.
        base.InjectAnimation("Custom", clip);
        base.Animator.ResetTrigger("New Trigger 0");
        base.Animator.SetFloat("New Float", speed);
        base.Animator.SetBool("New Bool 0", autoExit);

        // Note that the 'play' trigger is not set: this is done once the lerp is complete. See method OnLerpEnd.

        Debug.Log($"Interaction started: {clip.name} on [{anchour.gameObject.name.Trim()}]{anchour.name}, speed {speed:F2}x, auto exit: {autoExit}");

        Update();
    }

    private void CancelInteraction()
    {
        if (!IsInInteraction)
        {
            Debug.LogWarning("Currently not in interaction, nothing to interrupt!");
            return;
        }

        OnEndInteraction(true);
    }

    private void OnEndInteraction(bool cancelled)
    {
        Debug.Log($"Interaction ended ({(cancelled ? "cancelled" : "regular")})");
        IsInInteraction = false;
        ItemManager.AllowSwapping = true;
        CameraLook.Override = false;
        Movement.Override = false;
        CurrentAnchour = null;
        lerpTimer = 0f;

        if (cancelled)
        {
            base.Animator.SetTrigger("New Trigger 0");
        }

        // Re-equip last item.
        ItemManager.SetActiveItem(lastEquippedItemIndex);
    }

    private void UponAnimationEvent(AnimationEvent e)
    {
        string s = e.stringParameter.Trim().ToLower();
        switch (s)
        {
            case "end":
            case "customend":
            case "custom end":
            case "endcustom":
            case "end custom":
                OnEndInteraction(false);
                break;
        }
    }
}
