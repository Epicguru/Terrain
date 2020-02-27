using MyBox;
using Terrain.Effects;
using Terrain.Items.Guns;
using Terrain.Projectiles;
using Terrain.Utils;
using UnityEngine;

namespace Terrain.Enemies.Roamer
{
    public class RoamerGun : MonoBehaviour
    {
        [Header("Controls")]
        public bool Shoot = false;

        [Header("Settings")]
        public float RPM = 800f;
        public float SpinSpeed = 720f;
        public float SpinDownSpeed = 360f;
        public float MuzzleVelocity = 250f;

        [Header("Shooting")]
        public Transform Muzzle;
        public Projectile BulletPrefab;
        [MinMaxRange(0f, 180f)]
        public RangedFloat BulletDeviation = new RangedFloat(0f, 0f);

        [Header("Effects")]
        public MuzzleFlash FlashPrefab;

        private float timer;
        private float spinSpeed;

        private void Update()
        {
            if (Shoot)
                spinSpeed = SpinSpeed;
            else
                spinSpeed -= Time.deltaTime * SpinDownSpeed;

            if (spinSpeed < 0f)
                spinSpeed = 0f;

            timer += Time.deltaTime;
            if(timer >= 1f / (RPM / 60f))
            {
                if (Shoot)
                {
                    ShootImmediate();
                    timer = 0f;
                }
            }

            transform.localEulerAngles += new Vector3(0f, spinSpeed, 0f) * Time.deltaTime;
        }

        public void ShootImmediate()
        {
            if (Muzzle == null || BulletPrefab == null)
                return;

            float angle = BulletDeviation.LerpFromRange(Random.value);
            Vector3 dir = Gun.GenerateConeDirection(angle, Muzzle);
            Vector3 vel = dir * MuzzleVelocity;

            var proj = PoolObject.Spawn(BulletPrefab);
            proj.Velocity = vel;
            proj.transform.position = Muzzle.position;

            if(FlashPrefab != null)
            {
                var flash = PoolObject.Spawn(FlashPrefab);
                flash.transform.SetParent(Muzzle);
                flash.transform.localPosition = Vector3.zero;
                flash.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
