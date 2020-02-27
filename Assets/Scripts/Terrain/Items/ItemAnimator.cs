using System;
using System.Collections.Generic;
using Terrain.Utils;
using UnityEngine;

namespace Terrain.Items
{
    [DisallowMultipleComponent]
    public class ItemAnimator : AnimationInjector
    {
        public Item Item
        {
            get
            {
                if (_item == null)
                    _item = GetComponent<Item>();
                return _item;
            }
        }
        private Item _item;

        public int PendingActionCount { get { return actions.Count; } }

        [Header("General")]
        [Min(0.01f)]
        public float IdleSpeedMultiplier = 1f;

        private bool updateIdleSpeed = false;
        private readonly List<PendingAction> actions = new List<PendingAction>();

        protected override void Awake()
        {
            base.Awake();
        
            if (Animator != null && Animator.HasParameter("IdleSpeed", AnimatorControllerParameterType.Float))
                updateIdleSpeed = true;
        }

        private void Update()
        {
            UpdateIdleSpeed();
            UpdateActions();
        }

        private void UpdateIdleSpeed()
        {
            if (updateIdleSpeed)
                Animator.SetFloat("IdleSpeed", IdleSpeedMultiplier);
        }

        private void UpdateActions()
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action.Action == null)
                {
                    actions.RemoveAt(i);
                    i--;
                    continue;
                }

                bool allTrue = true;
                for (int j = 0; j < action.LayerIndex.Length; j++)
                {
                    bool conditionMet = action.LayerIndex[j] <= 0;
                    if (!conditionMet)
                    {
                        float layerWeight = Animator.GetLayerWeight(action.LayerIndex[j]);
                        switch (action.ComparisonType)
                        {
                            case ComparisonType.LessOrEqual:
                                conditionMet = layerWeight <= action.LayerWeight[j];
                                break;
                            case ComparisonType.GreaterOrEqual:
                                conditionMet = layerWeight >= action.LayerWeight[j];
                                break;
                        }
                    }

                    if (!conditionMet)
                    {
                        allTrue = false;
                        break;
                    }
                }

                if (allTrue)
                {
                    action.Action.Invoke();

                    actions.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }

        public void AddPendingAction(PendingAction action)
        {
            if (action.Action == null)
            {
                Debug.LogWarning("Invalid action: Action is null.");
                return;
            }
            if (action.LayerIndex == null || action.LayerWeight == null)
            {
                Debug.LogWarning("Null array(s?): layer index or weight.");
                return;
            }
            if (action.LayerIndex.Length != action.LayerWeight.Length)
            {
                Debug.LogWarning("Layer index / weight array length mismatch.");
                return;
            }
            for (int i = 0; i < action.LayerIndex.Length; i++)
            {
                if (action.LayerIndex[i] <= 0 || action.LayerIndex[i] >= Animator.layerCount)
                {
                    Debug.LogWarning($"Action for layer {action.LayerIndex} is invalid: this index is out of bounds. (min 1, max {Animator.layerCount - 1})");
                    return;
                }
                if (action.LayerWeight[i] < 0f || action.LayerWeight[i] >= 1f)
                {
                    Debug.LogWarning($"Action has invalid target weight: {action.LayerWeight}. This will be clamped to the 0-1 range.");
                    action.LayerWeight[i] = Mathf.Clamp01(action.LayerWeight[i]);
                }
            }

            actions.Add(action);
        }

        public bool HasPendingAction(int layerIndex)
        {
            if (layerIndex <= 0)
                return false;
            if (layerIndex >= Animator.layerCount)
                return false;

            foreach (var action in actions)
            {
                if (action.Action == null)
                    continue;

                for (int i = 0; i < action.LayerIndex.Length; i++)
                {
                    int index = action.LayerIndex[i];
                    if (index == layerIndex)
                        return true;
                }
            }

            return false;
        }

        public struct PendingAction
        {
            public int[] LayerIndex;
            public float[] LayerWeight;
            public ComparisonType ComparisonType;
            public Action Action;
        }
        public enum ComparisonType
        {
            LessOrEqual,
            GreaterOrEqual
        }
    }
}
