using MyBox;
using UnityEngine;

namespace Terrain.Items.Guns
{
    public class RevolverBulletManager : MonoBehaviour
    {
        public Gun Gun;

        [MinValue(0)]
        public int CasingsToShow;
        [MinValue(0)]
        public int BulletsToShow;

        public MeshRenderer[] Casings;
        public MeshRenderer[] Bullets;

        private void Awake()
        {
            if (Casings.Length != Bullets.Length)
                Debug.LogError($"Expected bullets array length to be same size as casings length, but there are {Casings.Length} casings and {Bullets.Length} bullets.");
        }

        private void Update()
        {
            if (Gun != null)
            {
                CasingsToShow = Gun.MagazineCapacity;
                BulletsToShow = Gun.MagazineBullets;
            }

            for (int i = 0; i < Casings.Length; i++)
            {
                var casing = Casings[i];
                if (casing == null)
                    continue;

                bool show = i < CasingsToShow;
                casing.enabled = show;
            }

            for (int i = 0; i < Bullets.Length; i++)
            {
                var bullet = Bullets[i];
                if (bullet == null)
                    continue;

                bool show = i < BulletsToShow;
                bullet.enabled = show;
            }
        }
    }
}
