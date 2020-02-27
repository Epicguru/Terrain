using System.Collections.Generic;
using UnityEngine;

namespace Terrain.Utils
{
    public static class AnimationHelper
    {
        public static AnimatorOverrideController CreateClone(this RuntimeAnimatorController rc)
        {
            bool isBase = !(rc is AnimatorOverrideController);
            var baseToUse = !isBase ? (rc as AnimatorOverrideController).runtimeAnimatorController : rc;
            var created = new AnimatorOverrideController(baseToUse);

            // Copy over clips from base.
            var overrides = created.GetOverrides();
            created.name = rc.name + " (runtime clone)";

            for (int i = 0; i < overrides.Count; i++)
            {
                var current = overrides[i];
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(current.Key, current.Key);
                //Debug.Log($"{current.Key.name} -> {(current.Value == null ? "null" : current.Value.name)}");
            }

            created.ApplyOverrides(overrides);

            return created;
        }

        public static List<KeyValuePair<AnimationClip, AnimationClip>> GetOverrides(this AnimatorOverrideController rc)
        {
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            rc.GetOverrides(overrides);
            return overrides;
        }

        public static int GetClipIndex(this RuntimeAnimatorController rc, string name)
        {
            var clips = rc.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == name)
                    return i;
            }
            return -1;
        }

        public static bool ReplaceAnimation(this AnimatorOverrideController oc, string name, AnimationClip c)
        {
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            oc.GetOverrides(overrides);

            return oc.ReplaceAnimation(overrides, name, c);
        }

        public static bool ReplaceAnimation(this AnimatorOverrideController oc, int index, AnimationClip c)
        {
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            oc.GetOverrides(overrides);

            return oc.ReplaceAnimation(overrides, index, c);
        }

        public static bool ReplaceAnimation(this AnimatorOverrideController oc, List<KeyValuePair<AnimationClip, AnimationClip>> overrides, string name, AnimationClip c)
        {
            for (int i = 0; i < overrides.Count; i++)
            {
                var current = overrides[i];
                if (current.Key.name == name)
                {
                    return oc.ReplaceAnimation(overrides, i, c);
                }
            }
            Debug.LogWarning($"Failed to find animation clip for name {name} to be replaced. Check spelling?");
            return false;
        }

        public static bool ReplaceAnimation(this AnimatorOverrideController oc, List<KeyValuePair<AnimationClip, AnimationClip>> overrides, int index, AnimationClip c)
        {
            var current = overrides[index];
            overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(current.Key, c);
            oc.ApplyOverrides(overrides);
            return true;

        }
    }
}
