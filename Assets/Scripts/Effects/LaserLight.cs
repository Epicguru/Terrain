
using UnityEngine;

public class LaserLight : MonoBehaviour
{
    public enum Mode
    {
        Off,
        Laser,
        Light,
        Both
    }

    [Header("References")]
    public LineRenderer Renderer;
    public Light LaserTipLight;
    public Light SpotLight;

    [Header("Controls")]
    public Mode CurrentMode = LaserLight.Mode.Laser;
    public float DefaultDistance = 200f, MaxDistance = 200f;
    public float LightOffset = 0.05f;
    public LayerMask Mask;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            CurrentMode = (Mode)(((int)CurrentMode + 1) % 4);
        }

        SpotLight.enabled = CurrentMode == Mode.Light || CurrentMode == Mode.Both;
        LaserTipLight.enabled = CurrentMode == Mode.Laser || CurrentMode == Mode.Both;
        Renderer.enabled = CurrentMode == Mode.Laser || CurrentMode == Mode.Both;

        if (CurrentMode == Mode.Laser || CurrentMode == Mode.Both)
        {
            float dst = GetCollisionDistance(Mask, MaxDistance, DefaultDistance);
            Renderer.SetPosition(1, new Vector3(0f, 0f, dst));
            LaserTipLight.transform.localPosition = new Vector3(0f, 0f, dst - LightOffset);
            Renderer.material.SetFloat("_Length", dst);
        }        
    }

    private float GetCollisionDistance(LayerMask mask, float maxDistance, float defaultDistance)
    {
        bool didHit = Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit, maxDistance, mask);

        if (!didHit)
        {
            return defaultDistance;
        }

        float dst = (hit.point - transform.position).magnitude;
        return Mathf.Min(dst, maxDistance);
    }
}
