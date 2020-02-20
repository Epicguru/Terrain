
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;

[RequireComponent(typeof(Camera))]
public class IconGen : MonoBehaviour
{
    public static IconGen Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<IconGen>();
            return _instance;
        }
    }
    private static IconGen _instance;
    public static int PendingRequests { get { return Instance.requests.Count; } }

    [System.Serializable]
    public class Request
    {
        /// <summary>
        /// The item to generate an icon for. Must NOT be a prefab item.
        /// </summary>
        public Item Item;
        /// <summary>
        /// Optional input texture. If the texture size matches the new icon, this texture is recycled and returned in
        /// <see cref="OnComplete"/>. If the sizes do not match, a new texture is created and the old texture must be destroyed.
        /// To check if the texture was recycled simply check:
        /// <code>request.OnComplete += newIcon =>
        /// {
        ///     if (newIcon != myOldIcon)
        ///     {
        ///         Destroy(myOldIcon);
        ///     }
        /// }</code>
        /// </summary>
        public Texture2D InputTexture;
        public bool IsDone;
        public Action<Texture2D> OnComplete;
    }

    public Camera Camera
    {
        get
        {
            if (_cam == null)
                _cam = GetComponent<Camera>();
            return _cam;
        }
    }
    private Camera _cam;

    public float DistanceZ = 500f;
    [Tooltip("Meters of padding on each side of the item.")]
    public float Padding = 0.05f;
    public bool DebugBounds = false;
    public float DebugIconScale = 0.5f;

    public Item TestItem;
    public Texture ItemTexture;

    private List<Bounds> toDraw = new List<Bounds>();
    private Queue<Request> requests = new Queue<Request>();

    public static void RequestIcon(Request r)
    {
        if (r == null)
        {
            Debug.LogError("Null item icon request!");
            return;
        }
        if(r.Item == null)
        {
            Debug.LogError("Cannot request icon for null item!");
            return;
        }
        if(r.OnComplete == null)
        {
            Debug.LogError($"OnComplete action is null for item icon request. Item: {r.Item}.");
            return;
        }

        var reqs = Instance.requests;
        if (!reqs.Contains(r))
            reqs.Enqueue(r);
        else
            Debug.LogError("Request has allready been submitted! Stop spamming :(");
    }

    [MyBox.ButtonMethod]
    private void RenderItem()
    {
        if (ItemTexture != null)
            Destroy(ItemTexture);

        ItemTexture = Gen(TestItem, null);
    }

    private void Awake()
    {
        _instance = this;
    }

    private void OnGUI()
    {
        if(ItemTexture != null)
            GUI.DrawTexture(new Rect(10, 10, ItemTexture.width * DebugIconScale, ItemTexture.height * DebugIconScale), ItemTexture);
    }

    private void Update()
    {
        ProcessRequests(1);
    }

    private float GetFOVForAxis(float targetHeight)
    {
        return Mathf.Atan(targetHeight / DistanceZ) * Mathf.Rad2Deg;
    }

    private void ProcessRequests(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if(requests.Count > 0)
            {
                var req = requests.Dequeue();
                if(req.Item != null && !req.IsDone && req.OnComplete != null)
                {
                    var tex = Gen(req.Item, req.InputTexture);
                    req.IsDone = true;
                    if (req.OnComplete != null)
                        req.OnComplete.Invoke(tex);
                }
            }
            else
            {
                break;
            }
        }
    }

    private Texture2D Gen(Item item, Texture2D recycleMe)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot generate icon, item is null!");
            return null;
        }

        var watch = new System.Diagnostics.Stopwatch();
        Profiler.BeginSample($"Icon Render ({item})");
        watch.Start();

        RenderTexture rt = Camera.targetTexture;

        // Place the item in the cool-zone.
        transform.position = new Vector3(0f, -100f, -DistanceZ);

        // Record item state before taking the picture.
        bool wasEnabled = item.gameObject.activeSelf;
        bool graphicsWasEnabled = item.Animation.gameObject.activeSelf;
        Vector3 oldPosition = item.transform.position;
        Quaternion oldRotation = item.transform.rotation;

        // Enable item graphics if they were disabled.
        item.gameObject.SetActive(true);
        item.Animation.gameObject.SetActive(true);

        // Record current animation state. The current system assumes that a 'Dropped' state exists.
        // TODO Allow for items that can be held but don't have animations.
        var currentState = item.Animation.Animator.GetCurrentAnimatorStateInfo(0);

        // Play the dropped animation, place the item in the target position.
        item.Animation.Animator.Play("Dropped", 0, 0f);
        item.Animation.Animator.Update(0);
        item.transform.position = new Vector3(0f, -100f, 0f);
        item.transform.rotation = transform.rotation;
        item.transform.Rotate(0f, 90f, 0f);

        // Find item graphics bounds, and place camera 'over' the center of the item bounds.
        var bounds = CalculateBounds(item.transform);
        transform.position = bounds.center - new Vector3(0f, 0f, DistanceZ);

        // Adjust camera FOV based on the bounds of the item. This ensures no space is wasted.
        float iWidth = bounds.size.x + Padding * 2;
        float iHeight = bounds.size.y + Padding * 2;
        bool isWiderThanTaller = iWidth >= iHeight;
        Camera.fieldOfView = GetFOVForAxis(isWiderThanTaller ? iWidth : iHeight);

        // Using the FOV information, determine which parts of the texture actually contain 'item'
        // and which parts are just empty space.
        float widthToHeightRatio = iWidth / iHeight;
        int iconWidth = isWiderThanTaller ? rt.width : Mathf.FloorToInt(rt.height * widthToHeightRatio);
        int iconHeight = !isWiderThanTaller ? rt.height : Mathf.FloorToInt(rt.width / widthToHeightRatio);
        if (widthToHeightRatio == 1f)
        {
            iconWidth = rt.width;
            iconHeight = rt.height;
        }
        int srcX = Mathf.FloorToInt((rt.width - iconWidth) / 2f);
        int srcY = Mathf.FloorToInt((rt.height - iconHeight) / 2f);

        // Snap!
        Camera.Render();

        // Put item back in old state, including animation.
        item.transform.position = oldPosition;
        item.transform.rotation = oldRotation;
        item.Animation.Animator.Play(currentState.fullPathHash, 0, currentState.normalizedTime);
        item.Animation.Animator.Update(0);

        // Reset item and item graphics if necessary.
        // When disabling item animator, careful to not reset the default pose.
        if (!graphicsWasEnabled)
        {
            item.Animation.Animator.Play("Idle", 0);
            for (int j = 0; j < item.Animation.Animator.layerCount; j++)
            {
                item.Animation.Animator.SetLayerWeight(j, 0);
            }
            item.Animation.Animator.Update(0f);
        }
        item.Animation.gameObject.SetActive(graphicsWasEnabled);
        item.gameObject.SetActive(wasEnabled);

        // Create the new texture, if necessary. Otherwise the recycled texture is used.
        Texture2D texture = null;
        if (recycleMe != null && recycleMe.width == iconWidth && recycleMe.height == iconHeight)
            texture = recycleMe;
        else
            texture = new Texture2D(iconWidth, iconHeight, rt.graphicsFormat, 0, TextureCreationFlags.None);

        texture.alphaIsTransparency = true;
        texture.hideFlags = HideFlags.HideAndDontSave;

        // Copy over the render texture to the new texture.
        Graphics.CopyTexture(rt, 0, 0, srcX, srcY, iconWidth, iconHeight, texture, 0, 0, 0, 0);

        // Done!
        watch.Stop();
        Profiler.EndSample();
        Debug.Log($"Took {watch.Elapsed.TotalMilliseconds:F1} ms to render icon for {item}.");

        return texture;
    }

    public Bounds CalculateBounds(Transform item)
    {
        if(DebugBounds)
            toDraw.Clear();

        var renderers = item.GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds();
        int count = 0;
        foreach (var renderer in renderers)
        {
            if (renderer.CompareTag("IgnoreForIcon"))
                continue;

            if(count == 0)
            {
                b = renderer.bounds;
            }

            count++;
            var rb = renderer.bounds;

            if(DebugBounds)
                toDraw.Add(rb);
            b.Encapsulate(rb);
        }

        //Debug.Log($"Took {renderers.Length} renderers into account to find bounds: {b}");
        if(DebugBounds)
            toDraw.Add(b);
        return b;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < toDraw.Count; i++)
        {
            if (i != toDraw.Count - 1)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            var b = toDraw[i];
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
}
