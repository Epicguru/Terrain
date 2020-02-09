
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    public Rigidbody Body
    {
        get
        {
            if (_body == null)
                _body = GetComponent<Rigidbody>();
            return _body;
        }
    }
    private Rigidbody _body;
    public Animator Animator
    {
        get
        {
            if (_anim == null)
                _anim = GetComponentInChildren<Animator>();
            return _anim;
        }
    }
    private Animator _anim;

    public ItemManager Manager { get; internal set; }

    [Header("Details")]
    public string Name;

    [Header("Equipped")]
    public Vector3 EquippedOffset;

    [Header("Animation")]
    [Tooltip("Enable to allow animation clips to be dynamically swapped at runtime. This has a small overhead, so only use if necessary. Most items with complex animation, such as weapons, will require this. This value has no effect if changed at runtime.")]
    public bool AllowAnimationInjection = false;
    [Tooltip("When set to true, animation overrides are cached to allow for faster injection with no GC allocation. However, memory usage may increase.")]
    public bool NonAllocInjection = true;

    [Header("Arms")]
    public Transform LeftHandPos;
    public Transform RightHandPos;

    public ItemState State
    {
        get
        {
            if (Manager == null)
                return ItemState.Dropped;

            if (Manager.ActiveItem == this)
                return ItemState.Active;
            else
                return ItemState.Equipped;
        }
    }

    private List<KeyValuePair<AnimationClip, AnimationClip>> clips;
    private bool allowInjection = false;
    private bool useNonAllocInjection = false;

    private void Awake()
    {        
        Body.interpolation = RigidbodyInterpolation.None;
        gameObject.layer = SortingLayer.NameToID("Items");

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

    /// <summary>
    /// Replaces an animation clip with another one. This only works when <see cref="AllowAnimationInjection"/> is enabled (it cannot be enabled at runtime).
    /// </summary>
    /// <param name="name">The name of the original animation clip. The name is the name of the clip, not the name of the state.</param>
    /// <param name="c">The new animation clip to replace it with. If null, the base animation is used. In most cases, this will not give good results, so avoid null unless you know what you are doing.</param>
    /// <returns>True if the operation was successful, false otherwise. See console for reasons why this might have failed.</returns>
    public bool InjectAnimation(string name, AnimationClip c)
    {
        if(Animator == null)
        {
            Debug.LogWarning($"Cannot inject to animation slot '{name}' because this item {Name} has no animation component.");
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
            Debug.LogWarning($"Cannot inject to animation slot '{name}' because this item {Name} has no animation component.");
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

    public void UponEquip()
    {
        Body.isKinematic = true;
        transform.localPosition = EquippedOffset;
        transform.localRotation = Quaternion.identity;
        BroadcastMessage("OnEquip",  SendMessageOptions.DontRequireReceiver);
    }

    public void UponDequip()
    {
        Body.isKinematic = false;
        BroadcastMessage("OnDequip", SendMessageOptions.DontRequireReceiver);
    }

    public void UponActivate()
    {
        Body.isKinematic = true;
        transform.localPosition = EquippedOffset;
        transform.localRotation = Quaternion.identity;
        BroadcastMessage("OnActivate", SendMessageOptions.DontRequireReceiver);
    }

    public void UponDeactivate()
    {
        Body.isKinematic = false;
        BroadcastMessage("OnDeactivate", SendMessageOptions.DontRequireReceiver);
    }

    private void Update()
    {
        // It is active when held by the player.
        if (State == ItemState.Active)
        {
            transform.localPosition = EquippedOffset;
            transform.localRotation = Quaternion.identity;
        }

        // It is dropped if not equipped on the player.
        Animator?.SetBool("Dropped", State == ItemState.Dropped);
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}

public enum ItemState
{
    Dropped,
    Equipped,
    Active
}
