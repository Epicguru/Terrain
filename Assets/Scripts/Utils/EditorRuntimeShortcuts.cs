
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorRuntimeShortcuts : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("Editor runtime shortcuts ready!");
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.BackQuote))
        {
            Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
        }
        else if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Time.timeScale = Time.timeScale == 0.2f ? 1f : 0.2f;
        }
    }
#endif
}
