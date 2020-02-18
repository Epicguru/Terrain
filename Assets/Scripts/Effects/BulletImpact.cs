
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(PoolObject))]
public class BulletImpact : MonoBehaviour
{
    public PoolObject PoolObject
    {
        get
        {
            if (_po == null)
                _po = GetComponent<PoolObject>();
            return _po;
        }
    }
    private PoolObject _po;

    public bool CheapMode = true;
    public Renderer Renderer;
    public float GlowFadeTime = 2f;
    public float DespawnTime = 4f;

    [ColorUsage(true, true)]
    public Color GlowColor, NormalColor;

    private float timer = 0f;

    private void UponSpawn()
    {
        timer = 0f;
        if(!CheapMode)
            Renderer.material.SetColor("_EmissiveColor", GlowColor);

    }

    private void Update()
    {
        timer += Time.deltaTime;
        float glow = Mathf.Clamp01(timer / GlowFadeTime);
        bool despawn = timer >= DespawnTime;

        if (despawn)
        {
            PoolObject.Despawn();
            return;
        }


        if(!CheapMode)
            Renderer.material.SetColor("_EmissiveColor", Color.Lerp(GlowColor, NormalColor, glow));
    }
}
