
using Terrain.Utils;
using UnityEngine;

namespace Terrain.Effects
{
    [RequireComponent(typeof(PoolObject))]
    public class MuzzleFlash : MonoBehaviour
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

        public float LifeTime = 0.03f;
        public Renderer[] Renderers;
        [Range(0f, 1f)]
        public float BaseAlpha = 0.9f;

        private float timer;

        private void UponSpawn()
        {
            timer = 0f;
        }

        private void Update()
        {
            float alpha = BaseAlpha * (1f - (timer / LifeTime));
            foreach (var renderer in Renderers)
            {
                var c = renderer.material.GetColor("_UnlitColor");
                c.a = alpha;
                renderer.material.SetColor("_UnlitColor", c);
            }

            timer += Time.deltaTime;
            if(timer >= LifeTime)
            {
                PoolObject.Despawn();
            }
        }
    }
}