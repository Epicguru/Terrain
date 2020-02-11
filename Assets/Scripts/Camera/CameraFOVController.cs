﻿
using UnityEngine;

public class CameraFOVController : MonoBehaviour
{
    public Camera Camera;
    public ItemManager ItemManager;

    public float NormalFOV = 70f;
    public float MaxFOVChangeSpeed = 100f;

    private float realFOV;

    private void Awake()
    {
        realFOV = NormalFOV;
    }

    private void Update()
    {
        float target = NormalFOV;

        // Find out if the current item is a gun, and apply ADS FOV reduction.
        var item = ItemManager.ActiveItem;
        Gun gun = null;
        if (item != null && item.IsGun)
            gun = item.Gun;

        if (gun != null)
            target = Mathf.Lerp(NormalFOV, NormalFOV * gun.ADS_FOV_Multiplier, gun.ADSLerp);

        realFOV = Mathf.MoveTowards(realFOV, target, MaxFOVChangeSpeed * Time.deltaTime);

        Camera.fieldOfView = realFOV;
    }
}
