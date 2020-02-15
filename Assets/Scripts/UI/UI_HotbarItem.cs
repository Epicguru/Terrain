
using UnityEngine;
using UnityEngine.UI;

public class UI_HotbarItem : MonoBehaviour
{
    public RawImage Image;
    public int MaxWidth = 300;

    public void UpdateIconAndSize(Item item)
    {
        if (item == null)
            return;

        if (item.IconTexture == null)
            item.RefreshIcon(false);

        Image.texture = item.IconTexture ?? item.DefaultIcon;
        if(Image.texture != null)
            (transform as RectTransform).sizeDelta = new Vector2(Image.texture.width, Image.texture.height);
    }
}
