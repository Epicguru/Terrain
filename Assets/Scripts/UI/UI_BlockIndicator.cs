
using UnityEngine;
using UnityEngine.UI;

public class UI_BlockIndicator : GlobalUIElement
{
    [Header("Refs")]
    public Image Img;

    [Header("Controls")]
    public bool Active = true;
    public float LerpSpeedPixels = 30f;

    public int BlockDirection;
    public Vector2[] Positions;

    [Header("Colours")]
    public Color Normal = Color.white;
    public Color Block = Color.red;

    private float hitAmount = 0f;

    private void Update()
    {
        var c = Img.color;
        Color a = new Color(Normal.r, Normal.g, Normal.b, c.a);
        Color b = new Color(Block.r, Block.g, Block.b, c.a);
        c = Color.Lerp(a, b, hitAmount);
        c.a = Mathf.MoveTowards(c.a, Active ? 1f : 0f, Time.deltaTime * 8f);
        Img.color = c;

        if (BlockDirection < 0 || BlockDirection > 2)
            return;

        var rt = transform as RectTransform;

        Vector2 targetPos = Positions[BlockDirection];
        Vector2 currentPos = rt.anchoredPosition;

        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, LerpSpeedPixels * Time.deltaTime);

        rt.anchoredPosition = newPos;
        rt.localScale = Vector3.one + (Vector3.one * 0.3f * hitAmount);

        hitAmount -= Time.deltaTime * 5f;
        if (hitAmount < 0f)
            hitAmount = 0f;
    }

    public void BlockHit()
    {
        hitAmount = 1f;
    }
}
