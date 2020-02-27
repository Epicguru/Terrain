
using UnityEngine;

namespace Terrain.Items.Guns.Instances
{
    public class Minigun : Gun
    {
        [MyBox.Foldout("Minigun", true)]
        public Transform Barrels;
        public float SpinSpeed = 800f;
        public float SpinUpSpeed = 500f;
        public float SpinDownSpeed = 900f;

        public float CurrentSpinSpeed { get; private set; } = 0f;

        public override bool CanShoot()
        {
            return CurrentSpinSpeed == SpinSpeed && base.CanShoot();
        }

        protected override void Update()
        {
            if(Item.State != ItemState.Active)
            {
                CurrentSpinSpeed = 0f;
            }
            else
            {
                bool canSpinUp = base.CanADS;
                bool spinUp = base.ShootPressed || base.IsInADS;

                if (spinUp && canSpinUp)
                    CurrentSpinSpeed += Time.deltaTime * SpinUpSpeed;
                else
                    CurrentSpinSpeed -= Time.deltaTime * SpinDownSpeed;

                CurrentSpinSpeed = Mathf.Clamp(CurrentSpinSpeed, 0f, SpinSpeed);
            }

            Barrels.localEulerAngles += new Vector3(0f, CurrentSpinSpeed * Time.deltaTime, 0f);

            base.Update();
        }
    }
}
