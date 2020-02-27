using Terrain.IK;
using Terrain.Items;
using UnityEngine;

namespace Terrain.Player
{
    [ExecuteInEditMode]
    public class PlayerArmManager : MonoBehaviour
    {
        public Player Pawn
        {
            get
            {
                if (_pawn == null)
                    _pawn = GetComponent<Player>();
                return _pawn;
            }
        }
        private Player _pawn;

        public LimbIK LeftArm, RightArm;
        public Transform IdleLeft, IdleRight;
        public Transform View;

        [Header("Transition")]
        [Range(0f, 10f)]
        public float TransitionTime = 1f;
        public AnimationCurve TransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Creator Mode")]
        public Item EditorItem;
        public bool EquipEditorItemOnStart = true;
        public bool GiveDebugWarnings = true;

        private float timer;
        private (Vector3 pos, float rot) lastPosRight, lastPosLeft;

        private void Start()
        {
            if(Application.isEditor && EditorItem != null && EquipEditorItemOnStart && Application.isPlaying)
            {
                int itemIndex = Pawn.ItemManager.Equip(EditorItem);
                if (itemIndex == -1)
                    itemIndex = Pawn.ItemManager.GetIndex(EditorItem);

                Pawn.ItemManager.SetActiveItem(itemIndex);

                EditorItem = null;
            }
        }

        private void LateUpdate()
        {
            var im = Pawn.ItemManager;
            var item = im.ActiveItem;

            if(!Application.isPlaying && EditorItem != null)
            {
                item = EditorItem;
                if (GiveDebugWarnings)
                {
                    if(item.transform.parent != Pawn.ItemManager.ItemParent)
                    {
                        Debug.LogWarning($"In order to animate and debug item {item.Name}, it should be a child of the player's {Pawn.ItemManager.ItemParent.name} gameobject.");
                    }
                    else
                    {
                        if(item.transform.localPosition != item.EquippedOffset)
                        {
                            Debug.LogWarning($"Current editing item {item.Name} does not have a local position of {item.EquippedOffset}! This can cause issues when animating.");
                        }
                        if (item.transform.localRotation != Quaternion.identity)
                        {
                            Debug.LogWarning($"Current editing item {item.Name} does not have a local rotation of (0, 0, 0)! This can cause issues when animating.");
                        }
                    }
                }
            }

            if(item != null)
            {
                var left = item.LeftHandPos;
                var right = item.RightHandPos;

                Vector3 finalRight = (right == null || !right.gameObject.activeInHierarchy) ? RightArm.transform.position - RightArm.transform.up * 2f : right.position;
                Vector3 finalLeft = (left == null || !left.gameObject.activeInHierarchy) ? LeftArm.transform.position - LeftArm.transform.up * 2f : left.position;

                float finalRotRight = right == null ? 0f : right.localEulerAngles.z;
                float finalRotLeft = left == null ? 0f : left.localEulerAngles.z;

                RightArm.TargetPosition = finalRight;
                RightArm.ElbowOffset = finalRotRight;
                lastPosRight = (View.InverseTransformPoint(finalRight), finalRotRight);

                LeftArm.TargetPosition = finalLeft;
                LeftArm.ElbowOffset = finalRotLeft;
                lastPosLeft = (View.InverseTransformPoint(finalLeft), finalRotLeft);

                timer = 0f;
            }
            else
            {
                timer += Time.deltaTime;
                if (timer > TransitionTime)
                    timer = TransitionTime;

                float p = TransitionTime <= 0f ? 1f : (timer / TransitionTime);
                float x = TransitionCurve.Evaluate(p);

                Vector3 worldRight = View.TransformPoint(lastPosRight.pos);
                Vector3 worldLeft = View.TransformPoint(lastPosLeft.pos);

                // Where 0 is item, 1 is idle pos.
                Vector3 finalRight = Vector3.Lerp(worldRight, IdleRight.position, x);
                float finalRotRight = Mathf.Lerp(lastPosRight.rot, IdleRight.localEulerAngles.z, x);

                Vector3 finalLeft = Vector3.Lerp(worldLeft, IdleLeft.position, x);
                float finalRotLeft = Mathf.Lerp(lastPosLeft.rot, IdleLeft.localEulerAngles.z, x);

                RightArm.TargetPosition = finalRight;
                RightArm.ElbowOffset = finalRotRight;

                LeftArm.TargetPosition = finalLeft;
                LeftArm.ElbowOffset = finalRotLeft;
            }

            LeftArm.DoUpdate();
            RightArm.DoUpdate();
        }
    }
}