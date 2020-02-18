
using UnityEngine;

[RequireComponent(typeof(PoolObject))]
public class TempParticleEffect : MonoBehaviour
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

    public ParticleSystem ParticleSystem
    {
        get
        {
            if (_ps == null)
                _ps = GetComponent<ParticleSystem>();
            return _ps;
        }
    }
    private ParticleSystem _ps;

    public float LifeTime = 5f;

    private float timer = 0f;

    private void UponSpawn()
    {
        ParticleSystem.Play(true);
        timer = 0f;
    }

    private void UponDespawn()
    {
        ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= LifeTime)
            PoolObject.Despawn();
    }
}
