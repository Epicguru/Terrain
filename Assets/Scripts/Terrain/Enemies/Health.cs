using MyBox;
using System;
using UnityEngine;

namespace Terrain.Enemies
{
    public class Health : MonoBehaviour
    {
        public int CurrentHealth = 100, MaxHealth = 100;
        public event Action<Health, int> UponHealthChange;
        public event Action<Health> UponDeath;

        public float HealthPercentage
        {
            get
            {
                return Mathf.Clamp01((float)CurrentHealth / MaxHealth);
            }
        }
        public bool IsDead
        {
            get
            {
                return CurrentHealth <= 0;
            }
        }

        public void ChangeHealth(int change)
        {
            if (change == 0)
                return;

            if (IsDead)
            {
                // Do not allow health change after death.
                return;
            }

            CurrentHealth = Mathf.Clamp(CurrentHealth + change, 0, MaxHealth);
            UponHealthChange?.Invoke(this, change);
            if (IsDead)
            {
                UponDeath?.Invoke(this);
            }
        }

        [ButtonMethod]
        private void Editor_Kill()
        {
            ChangeHealth(-CurrentHealth);
        }
    }
}
