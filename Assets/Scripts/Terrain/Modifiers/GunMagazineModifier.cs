
using Terrain.Items.Guns;
using UnityEngine;

namespace Terrain.Modifiers
{
    [RequireComponent(typeof(Gun))]
    public class GunMagazineModifier : MonoBehaviour, IModifier<Gun>
    {
        public bool RequiresReapplication { get; private set; }
        public float MagazineSizeChange = 2f;

        [MyBox.ButtonMethod]
        public void ApplyMod()
        {
            ApplyMod(GetComponent<Gun>());
        }

        public void ApplyMod(Gun target)
        {
            target.MagazineCapacity = Mathf.CeilToInt(target.MagazineCapacity * MagazineSizeChange);
        }
    }
}