
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class GlobalUIElement : MonoBehaviour
{
    private static Dictionary<Type, GlobalUIElement> all = new Dictionary<Type, GlobalUIElement>();

    public static T Get<T>() where T : GlobalUIElement
    {
        var t = typeof(T);
        if (all.ContainsKey(t))
            return all[t] as T;

        return default(T);
    }

    protected virtual void Awake()
    {
        var t = this.GetType();
        if (all.ContainsKey(t))
        {
            Debug.LogWarning($"Duplicate global UI element of type {t.FullName}. Only one component is expected per scene.");
            return;
        }

        all.Add(t, this);
        Debug.Log($"Registered global UI: '{t.FullName}'");
    }
}
