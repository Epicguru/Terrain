
using Terrain.Items;
using UnityEngine;
using UnityEngine.UI;

public class UI_HotbarItem : MonoBehaviour
{
    public RawImage Image;
    public int MaxWidth = 300;
    public Vector2 Padding;

    public void UpdateIconAndSize(Item item)
    {
        if (item == null)
            return;

        if (item.IconTexture == null)
            item.RefreshIcon(false);

        Image.texture = item.IconTexture ?? item.DefaultIcon;

        Vector2 containerSize = (transform as RectTransform).sizeDelta - Padding;
        Vector2 iconSize = new Vector2(Image.texture.width, Image.texture.height);

        float containerRatio = containerSize.x / containerSize.y;
        float iconRatio = iconSize.x / iconSize.y;

        bool fitHeight = iconRatio < containerRatio;
        Vector2 finalSize = Vector2.zero;

        if (fitHeight)
        {
            finalSize.y = containerSize.y;
            finalSize.x = (finalSize.y / iconSize.y) * iconSize.x;
        }
        else
        {
            finalSize.x = containerSize.x;
            finalSize.y = (finalSize.x / iconSize.x) * iconSize.y;
        }

        Image.rectTransform.sizeDelta = finalSize;
    }
}
