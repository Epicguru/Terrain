
using UnityEngine;

namespace Terrain.Items.Guns
{
    [ExecuteInEditMode]
    public class GunSlide : MonoBehaviour
    {
#pragma warning disable CS0649

        [Header("Animation")]
        [Range(0f, 2f)]
        [SerializeField]
        private float Lerp = 0f;
        [SerializeField]
        private bool AnimOverride = false;

        [Header("Settings")]
        [SerializeField]
        private Transform Target;
        [SerializeField]
        private Vector3 StartPos, EndPos;

        // If override is true, then the animation lerp value is not used UNLESS AnimOverride is true.
        [HideInInspector]
        public bool Override = false;
        [HideInInspector]
        public float OverrideLerp = 1f;
        [HideInInspector]
        public bool IsInTransition = false; // When the animator is in transition, AnimOverride is ignored since the animation lerp property is incorrectly interpolated by Mekanism.

#pragma warning restore CS0649

        private void LateUpdate()
        {
            float lerp = Lerp;
            //if (Override && !(AnimOverride && true))
            if (Override && !(AnimOverride && !IsInTransition))
                lerp = OverrideLerp;

            Target.localPosition = Vector3.LerpUnclamped(StartPos, EndPos, lerp);
        }
    }
}
