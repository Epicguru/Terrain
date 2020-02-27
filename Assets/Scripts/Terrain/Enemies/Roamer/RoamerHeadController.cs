
using UnityEngine;

namespace Terrain.Enemies.Roamer
{
    public class RoamerHeadController : MonoBehaviour
    {
        [Header("References")]
        public Transform Head;

        [Header("Settings")]
        public float MaxTurnSpeed = 360f;

        [Header("Control")]
        public Transform Target;
        public bool LookAtTarget = true;

        private void Update()
        {
            Quaternion target = (Target == null || !LookAtTarget) ? Quaternion.identity : Quaternion.LookRotation((Target.position - Head.position), Vector3.up);

            Quaternion final = Quaternion.RotateTowards(Head.rotation, target, MaxTurnSpeed * Time.deltaTime);
            Head.rotation = final;
        }
    }
}
