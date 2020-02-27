
using UnityEngine;

namespace Terrain.Player.Interactions
{
    [DisallowMultipleComponent]
    public class InteractionAnchour : MonoBehaviour
    {
        public Vector3 Offset;
    
        public Vector3 GetPlayerPosition()
        {
            return transform.TransformPoint(Offset);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(GetPlayerPosition(), Vector3.one * 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(GetPlayerPosition(), GetPlayerPosition() + transform.forward * 0.2f);
        }
    }
}