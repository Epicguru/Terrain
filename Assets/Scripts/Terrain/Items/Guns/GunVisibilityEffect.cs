using System.Collections.Generic;
using UnityEngine;

namespace Terrain.Items.Guns
{
    public class GunVisibilityEffect : MonoBehaviour
    {
        public enum StackingMode
        {
            /// <summary>
            /// Object is visible if any of the conditions are met.
            /// </summary>
            Any,
            /// <summary>
            /// Object is visibile if exactly one of the conditions are met.
            /// </summary>
            AnySingle,
            /// <summary>
            /// Object is visible if all of the conditions are met.
            /// </summary>
            All
        }
        public enum MagazineCheckMode
        {
            /// <summary>
            /// Magazine comparison is from empty. For example, checking if it has at least 5 bullets
            /// would work exactly as expected, returning true when bullets in magazine >= 5.
            /// </summary>
            FromEmpty,
            /// <summary>
            /// Magazine comparison is from full. For example, checking it it has at least 5 bullets
            /// would check if magazine bullets is >= (Magazine Capacity - 5).
            /// </summary>
            FromFull
        }

        public Gun Gun
        {
            get
            {
                if (_gun == null)
                    _gun = GetComponentInParent<Gun>();
                return _gun;
            }
        }
        private Gun _gun;

        public Renderer Renderer;

        [Header("Master")]
        public bool MasterInvert = false;
        public StackingMode ConditionStackMode = StackingMode.Any;

        [Header("Conditions")]

        [MyBox.Separator("Empty State")]
        public bool VisibleIfEmpty = false;
        [MyBox.ConditionalField("VisibleIfEmpty")]
        public bool EmptyInvert = false;

        [MyBox.Separator("Magazine Bullets")]
        public bool VisibleIfMagazineBulletsMoreThan = false;
        [MyBox.ConditionalField("VisibleIfMagazineBulletsMoreThan")]
        public bool MagBulletsInvert = false;
        [Min(0)]
        public int MagBulletCompare = 5;
        public MagazineCheckMode MagCompareMode = MagazineCheckMode.FromEmpty;

        private static List<bool> flags;
        private void Update()
        {
            if (Gun == null || Renderer == null)
                return;
            if (flags == null)
                flags = new List<bool>();
            flags.Clear();

            // If empty...
            if (VisibleIfEmpty)
            {
                bool vis = !Gun.BulletInChamber;
                if (EmptyInvert)
                    vis = !vis;

                flags.Add(vis);
            }

            // Number of bullets in mag...
            if (VisibleIfMagazineBulletsMoreThan)
            {
                int bulletsInMag = Gun.MagazineBullets;
                int cap = Gun.MagazineCapacity;

                bool vis = false;
                if(MagCompareMode == MagazineCheckMode.FromEmpty)
                {
                    vis = bulletsInMag >= MagBulletCompare;
                }
                if(MagCompareMode == MagazineCheckMode.FromFull)
                {
                    vis = bulletsInMag >= cap - MagBulletCompare;
                }

                if (MagBulletsInvert)
                    vis = !vis;

                flags.Add(vis);
            }

            // Final.
            bool final = CompileFinal(ConditionStackMode, flags);
            if (MasterInvert)
                final = !final;

            // Apply.
            Renderer.enabled = final;
        }

        private bool CompileFinal(StackingMode mode, List<bool> bools)
        {
            bool hasTrue = false;
            bool defaultReturnVal = false;

            for (int i = 0; i < bools.Count; i++)
            {
                bool flag = bools[i];
                switch (mode)
                {
                    case StackingMode.Any:
                        if (flag)
                            return true;
                        break;

                    case StackingMode.AnySingle:
                        if(flag)
                        {
                            defaultReturnVal = true;
                            if (hasTrue)
                            {
                                return false;
                            }
                            hasTrue = true;
                        }
                        break;

                    case StackingMode.All:
                        if (!flag)
                            return true;
                        break;
                }
            }

            return defaultReturnVal;
        }
    }
}
