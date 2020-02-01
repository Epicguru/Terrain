using UnityEngine;

namespace ProjectB
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
