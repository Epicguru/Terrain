using UnityEngine;

namespace Terrain.Utils
{
    [RequireComponent(typeof(Animator))]
    public class GenericAnimationCallback : MonoBehaviour
    {
        public void Event(AnimationEvent e)
        {
            SendMessageUpwards("UponAnimationEvent", e, SendMessageOptions.DontRequireReceiver);
        }
    }
}
