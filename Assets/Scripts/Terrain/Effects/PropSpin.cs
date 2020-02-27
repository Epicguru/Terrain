
using UnityEngine;

namespace Terrain.Effects
{
    public class PropSpin : MonoBehaviour
    {
        public float Scale = 1f;
        public Vector3 LocalAngles = new Vector3(0f, 0f, 180f);

        private void Update()
        {
            transform.localEulerAngles += LocalAngles * Scale * Time.deltaTime;
        }
    }
}
