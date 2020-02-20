
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemAnimator : MonoBehaviour
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
    public Animator Animator
    {
        get
        {
            if(_anim == null)
                _anim = GetComponentInChildren<Animator>();
            return _anim;
        }
    }
    private Animator _anim;    

    public int PendingActionCount { get { return actions.Count; } }

    [Header("Animation Injection")]
    [Tooltip("Enable to allow animation clips to be dynamically swapped at runtime. This has a small overhead, so only use if necessary. Most items with complex animation, such as weapons, will require this. This value has no effect if changed at runtime.")]
    public bool AllowAnimationInjection = false;
    [Tooltip("When set to true, animation overrides are cached to allow for faster injection with no GC allocation. However, memory usage may increase.")]
    public bool NonAllocInjection = true;

    private List<KeyValuePair<AnimationClip, AnimationClip>> clips;
    private bool allowInjection = false;
    private bool useNonAllocInjection = false;
    private readonly List<PendingAction> actions = new List<PendingAction>();

    private void Awake()
    {
        if (AllowAnimationInjection && Animator != null)
        {
            // Clone the animator to allow for runtime clip replacing.
            Animator.runtimeAnimatorController = Animator.runtimeAnimatorController.CreateClone();
            allowInjection = true;

            if (NonAllocInjection)
            {
                clips = (Animator.runtimeAnimatorController as AnimatorOverrideController).GetOverrides();
                useNonAllocInjection = true;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            if(action.Action == null)
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

    /// <summary>
    /// Replaces an animation clip with another one. This only works when <see cref="AllowAnimationInjection"/> is enabled (it cannot be enabled at runtime).
    /// </summary>
    /// <param name="name">The name of the original animation clip. The name is the name of the clip, not the name of the state.</param>
    /// <param name="c">The new animation clip to replace it with. If null, the base animation is used. In most cases, this will not give good results, so avoid null unless you know what you are doing.</param>
    /// <returns>True if the operation was successful, false otherwise. See console for reasons why this might have failed.</returns>
    public bool InjectAnimation(string name, AnimationClip c)
    {
        if (Animator == null)
        {
            Debug.LogWarning($"Cannot inject to animation slot '{name}' because this item {Item.Name} has no animation component.");
            return false;
        }
        if (!allowInjection)
        {
            Debug.LogWarning($"Cannot inject to animation slot '{name}' because this item ({name}) does not support animation injection. In the inspector, enable 'Allow Animation Injection'. This value cannot be changed through code at runtime.");
            return false;
        }

        if (useNonAllocInjection)
        {
            // Faster, no GC waste generated. However, if there are many instances of this item in the world, it could increase memory usage.
            return (Animator.runtimeAnimatorController as AnimatorOverrideController).ReplaceAnimation(clips, name, c);
        }
        else
        {
            // Slower, and fairly high GC waste generated. Avoid without good reason.
            return (Animator.runtimeAnimatorController as AnimatorOverrideController).ReplaceAnimation(name, c);
        }
    }

    /// <summary>
    /// Replaces an animation clip with another one. This only works when <see cref="AllowAnimationInjection"/> is enabled (it cannot be enabled at runtime).
    /// </summary>
    /// <param name="index">The index of the clip to replace.</param>
    /// <param name="c">The new animation clip to replace it with. If null, the base animation is used. In most cases, this will not give good results, so avoid null unless you know what you are doing.</param>
    /// <returns>True if the operation was successful, false otherwise. See console for reasons why this might have failed.</returns>
    public bool InjectAnimation(int index, AnimationClip c)
    {
        if (Animator == null)
        {
            Debug.LogWarning($"Cannot inject to animation slot '{name}' because this item {Item.Name} has no animation component.");
            return false;
        }
        if (!allowInjection)
        {
            Debug.LogWarning($"Cannot inject to animation slot '{name}' because this item ({name}) does not support animation injection. In the inspector, enable 'Allow Animation Injection'. This value cannot be changed through code at runtime.");
            return false;
        }

        if (useNonAllocInjection)
        {
            // Faster, no GC waste generated. However, if there are many instances of this item in the world, it could increase memory usage.
            return (Animator.runtimeAnimatorController as AnimatorOverrideController).ReplaceAnimation(clips, name, c);
        }
        else
        {
            // Slower, and fairly high GC waste generated. Avoid without good reason.
            return (Animator.runtimeAnimatorController as AnimatorOverrideController).ReplaceAnimation(name, c);
        }
    }

    /// <summary>
    /// Gets the index corresponding to an animation clip. This can be used in <see cref="InjectAnimation(int, AnimationClip)"/> since it is faster than passing in a name.
    /// </summary>
    /// <param name="name">The name of the clip. Case sensitive and whitespace is not removed.</param>
    /// <returns>The index of the animation clip, or -1 if the clip was not found.</returns>
    public int GetAnimationClipIndex(string name)
    {
        return Animator.runtimeAnimatorController.GetClipIndex(name);
    }

    public void AddPendingAction(PendingAction action)
    {
        if(action.Action == null)
        {
            Debug.LogWarning("Invalid action: Action is null.");
            return;
        }
        if(action.LayerIndex == null || action.LayerWeight == null)
        {
            Debug.LogWarning("Null array(s?): layer index or weight.");
            return;
        }
        if(action.LayerIndex.Length != action.LayerWeight.Length)
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
