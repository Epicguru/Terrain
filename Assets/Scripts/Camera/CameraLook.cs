
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public static CameraLook Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<CameraLook>();
            return _instance;
        }
    }
    private static CameraLook _instance;

    [Header("References")]
    public Transform Yaw;
    public Transform Pitch;

    [Header("Controls")]
    public float MouseSensitivity = 1f;
    public float InternalSens = 1f;
    public bool CaptureMouse = true;

    [Header("Recoil")]
    [Range(0f, 1f)]
    public float VelocityReductionCoefficient = 0.7f;

    [Header("Recoil Recovery")]
    public bool RecoveryMode = false;
    public float RecoverySpeed = 60f;
    public float MinTimeSinceRecoil = 0.1f;

    [Header("Runtime")]
    public float HorizontalTurnDelta = 0f;
    public float VerticalTurnDelta = 0f;

    private float horizontalLook;
    private float verticalLook;

    private Vector2 recoilOffset;

    private const float RECOIL_FREQ = 60f;
    private const float RECOIL_DELTA_TIME = 1f / RECOIL_FREQ;
    private List<Vector2> recoils = new List<Vector2>();
    private float recoilTimer = 0f;

    private void Awake()
    {
        _instance = this;
        InvokeRepeating("UpdateRecoil", 0f, RECOIL_DELTA_TIME);
    }

    private void Update()
    {
        if (!RecoveryMode)
            recoilOffset = Vector2.zero;

        float oldX = horizontalLook;
        float oldY = verticalLook;

        horizontalLook += Input.GetAxisRaw("Mouse X") * MouseSensitivity * InternalSens;
        verticalLook -= Input.GetAxisRaw("Mouse Y") * MouseSensitivity * InternalSens;
        verticalLook = Mathf.Clamp(verticalLook, -90f, 90f);

        Yaw.localEulerAngles = new Vector3(0f, horizontalLook + recoilOffset.x, 0f);
        Pitch.localEulerAngles = new Vector3(verticalLook + recoilOffset.y, 0f, 0f);

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

        if (RecoveryMode)
        {
            if (recoils.Count != 0)
            {
                recoilTimer = 0f;
            }
            else
            {
                recoilTimer += Time.deltaTime;
            }
            if (recoils.Count == 0 && recoilTimer >= MinTimeSinceRecoil)
            {
                recoilOffset = Vector2.MoveTowards(recoilOffset, Vector2.zero, RecoverySpeed * Time.deltaTime);
            }
        }        
    }

    public void AddRecoil(Vector2 vel)
    {
        vel.y *= -1f;
        recoils.Add(vel);
    }

    private void UpdateRecoil()
    {
        for (int i = 0; i < recoils.Count; i++)
        {
            var item = recoils[i];

            if (RecoveryMode)
            {
                recoilOffset.x += item.x * RECOIL_DELTA_TIME;
                recoilOffset.y += item.y * RECOIL_DELTA_TIME;
            }
            else
            {
                horizontalLook += item.x * RECOIL_DELTA_TIME;
                verticalLook += item.y * RECOIL_DELTA_TIME;
            }

            item *= VelocityReductionCoefficient;

            recoils[i] = item;

            if(item.sqrMagnitude <= 0.1f * 0.1f)
            {
                recoils.RemoveAt(i);
            }
        }
    }
}
