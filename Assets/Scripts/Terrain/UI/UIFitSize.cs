
using UnityEngine;

[ExecuteInEditMode]
public class UIFitSize : MonoBehaviour
{
    public RectTransform ToMatch;
    public Vector2 SizeOffset;
    public Vector2 PositionOffset;

    public bool MatchPosition = true;

    private void Update()
    {
        if (ToMatch == null)
            return;

        var rt = transform as RectTransform;
        if (rt == null)
            return;

        if(MatchPosition)
            rt.anchoredPosition = ToMatch.anchoredPosition + PositionOffset;
        rt.sizeDelta = ToMatch.sizeDelta + SizeOffset;
    }
}

