using Terrain.Camera;
using Terrain.Items;
using Terrain.Utils;
using UnityEngine;
using UnityEngine.UI;

public class UI_PickupInfo : GlobalUIElement
{
    public Item CurrentlyHighlightedItem { get; private set; }

    public TMPro.TMP_Text Text;
    public RectTransform CanvasRect;
    public RectTransform Box;
    public Image BoxImage;
    public float MaxWorldDistance = 6f;
    [Range(-1, 1f)]
    public float MinDotProduct = 0.5f;
    public float FadeTime = 0.8f;
    public AnimationCurve FadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float timer = 0f;
    private CanvasGroup group;

    protected override void Awake()
    {
        base.Awake();

        group = Box.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        CurrentlyHighlightedItem = null;
        float maxDst = MaxWorldDistance * MaxWorldDistance;
        Camera cam = CameraLook.Instance.Camera;
        Vector3 playerPos = cam.transform.position;
        Vector3 camForwards = cam.transform.forward;

        Item toDisplay = null;
        float clostest = -1f;
        Vector2 closestScreenPos = Vector2.zero;

        foreach (var item in Item.DroppedItems)
        {
            float sqrDst = (item.transform.position - playerPos).sqrMagnitude;
            if (sqrDst > maxDst)
                continue;            
    
            Vector3 toItem = item.transform.position - playerPos;
            float dot = Vector3.Dot(toItem.normalized, camForwards);
            if (dot < MinDotProduct)
                continue;
            
            if(dot > clostest)
            {
                Vector2 scrPos = cam.WorldToViewportPoint(item.transform.position) * CanvasRect.sizeDelta;
                clostest = dot;
                closestScreenPos = scrPos;
                toDisplay = item;
            }
        }

        float a = FadeCurve.Evaluate(Mathf.Clamp01((FadeTime - timer) / FadeTime));
        if(toDisplay != null)
        {
            CurrentlyHighlightedItem = toDisplay;
            timer -= Time.unscaledDeltaTime;
            float elevation = -50f + a * 150f;
            Box.anchoredPosition = closestScreenPos + Vector2.up * elevation;
            BoxImage.color = toDisplay.Rarity.GetColor();

            /*
             * Item Name - RARITY
             * Item quick details [gun, melee, crafting, quest etc.]
             * Item specific (from item) [10 uses left, broken, linked to door, disabled]
             */

            string primaryDetails = toDisplay.GetPrimaryDetails();
            string secondaryDetails = toDisplay.GetSecondaryDetails();

            Text.text = $"{toDisplay.Name.Trim()} - {toDisplay.Rarity.GetDisplayName(true)}\n{primaryDetails.Trim()}\n{secondaryDetails.Trim()}".Trim();
        }
        else
        {
            timer += Time.unscaledDeltaTime;
        }
        timer = Mathf.Clamp(timer, 0f, FadeTime);

        group.alpha = a;
        Box.localScale = Vector3.one * a;
    }
}
