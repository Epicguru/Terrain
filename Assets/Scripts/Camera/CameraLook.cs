
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Header("References")]
    public Transform Yaw;
    public Transform Pitch;

    [Header("Controls")]
    public float MouseSensitivity = 1f;
    public float InternalSens = 1f;
    public bool CaptureMouse = true;

    [Header("Runtime")]
    public float HorizontalTurnDelta = 0f;
    public float VerticalTurnDelta = 0f;

    private float horizontalLook;
    private float verticalLook;

    private void Update()
    {
        float oldX = horizontalLook;
        float oldY = verticalLook;

        horizontalLook += Input.GetAxisRaw("Mouse X") * MouseSensitivity * InternalSens;
        verticalLook -= Input.GetAxisRaw("Mouse Y") * MouseSensitivity * InternalSens;
        verticalLook = Mathf.Clamp(verticalLook, -90f, 90f);

        Yaw.localEulerAngles = new Vector3(0f, horizontalLook, 0f);
        Pitch.localEulerAngles = new Vector3(verticalLook, 0f, 0f);

        if (CaptureMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        HorizontalTurnDelta = horizontalLook - oldX;
        VerticalTurnDelta = verticalLook - oldY;
    }
}
