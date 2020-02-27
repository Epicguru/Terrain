using System.Collections.Generic;
using Terrain.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Terrain.Camera
{
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

        [Header("Override")]
        public bool Override = false;
        public Vector3 OverrideTargetDirection = Vector3.forward;

        [Header("Batching")]
        public bool ForceSRPBatching = true;

        [Header("References")]
        public Transform Yaw;
        public Transform Pitch;
        public UnityEngine.Camera Camera;

        [Header("Code Controls")]
        public bool CaptureMouse = true;
        public bool UseRigidbodyYaw = true;

        [Header("Sensitivities")]
        public float MouseSensitivity = 0.1f;
        public float GamepadSensitivity = 0.5f;
        public float MouseADSSensitivityMultiplier = 1f;
        public float GamepadADSSensitivityMultiplier = 0.3f;

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
        private readonly List<Vector2> recoils = new List<Vector2>();
        private float recoilTimer = 0f;
        private Vector2 input;

        private void Awake()
        {
            _instance = this;
            InvokeRepeating("UpdateRecoil", 0f, RECOIL_DELTA_TIME);
            Player.Player.Input.actions["Look"].performed += Input_Look;
            Player.Player.Input.actions["Look"].canceled += Input_Look;
            Player.Player.Input.actions["Look"].started += Input_Look;

            if(ForceSRPBatching)
                Invoke("EnableBatcher", 2f); // Wait 2 seconds because Unity likes to reset this property. Thanks Unity :D
        }

        private void EnableBatcher()
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
            Debug.Log("Force enabled SRP batcher.");        
        }

        private void Input_Look(InputAction.CallbackContext context)
        {
            input = context.ReadValue<Vector2>();
        }

        private void Update()
        {
            if (Override)
            {
                HorizontalTurnDelta = 0f;
                VerticalTurnDelta = 0f;
                recoils.Clear();

                Vector3 dir = OverrideTargetDirection;
                if (dir.sqrMagnitude != 1f)
                    dir.Normalize();

                // Check for (0, 0, 0) vector.
                if (dir.sqrMagnitude <= 0.01f)
                    return;

                Quaternion rot = Quaternion.LookRotation(dir, -Physics.gravity);
                Vector3 angles = rot.eulerAngles;

                verticalLook = angles.x;
                horizontalLook = angles.y;

                if (!UseRigidbodyYaw)
                    Yaw.localEulerAngles = new Vector3(0f, horizontalLook, 0f);
                else
                    Yaw.GetComponent<Rigidbody>().rotation = Quaternion.Euler(0f, horizontalLook, 0f);
                Pitch.localEulerAngles = new Vector3(verticalLook, 0f, 0f);

                return;
            }

            if (!RecoveryMode)
                recoilOffset = Vector2.zero;

            float oldX = horizontalLook;
            float oldY = verticalLook;

            // Make framerate independent: mouse delta is naturally independent, but controller input
            // is just a normalized value, so need to be tied to the time between frames.
            float frameRateIndependent = Player.Player.Input.IsKeyboardAndMouse() ? 1f : Time.deltaTime;
            Vector2 delta = input * frameRateIndependent;

            delta *= CalculateCurrentSensitivity();                       

            horizontalLook += delta.x;
            verticalLook -= delta.y;
            verticalLook = Mathf.Clamp(verticalLook, -90f, 90f);

            if (!UseRigidbodyYaw)
                Yaw.localEulerAngles = new Vector3(0f, horizontalLook + recoilOffset.x, 0f);
            else
                Yaw.GetComponent<Rigidbody>().rotation = Quaternion.Euler(0f, horizontalLook + recoilOffset.x, 0f);
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

        public float CalculateCurrentSensitivity()
        {
            float sens = 1f;

            // Keyboard vs controller changes sens.
            sens *= Player.Player.Input.IsKeyboardAndMouse() ? MouseSensitivity : GamepadSensitivity;

            // FOV changes sens.
            sens *= CameraFOVController.FovMultiplier;

            // ADS changes sensitivity...
            float adsSens = Player.Player.Input.IsKeyboardAndMouse() ? MouseADSSensitivityMultiplier : GamepadADSSensitivityMultiplier;
            if (adsSens != 1f)
            {
                var im = Player.Player.Instance.ItemManager;
                if (im.ActiveItem != null)
                {
                    var gun = im.ActiveItem.Gun;
                    if (gun != null)
                    {
                        sens *= Mathf.Lerp(1f, adsSens, gun.ADSLerp);
                    }
                }
            }

            return sens;
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

        //private void OnGUI()
        //{
        //    GUILayout.Label($"Device name: {SystemInfo.deviceName}");
        //    GUILayout.Label($"Device name: {SystemInfo.deviceName}");
        //    GUILayout.Label($"Battery status: {SystemInfo.batteryStatus}");
        //    GUILayout.Label($"Battery level: {SystemInfo.batteryLevel*100f:F1}%");
        //}
    }
}
