
using UnityEngine;

public class UI_Hotbar : GlobalUIElement
{
    [SerializeField]
    private UI_HotbarItem[] uis;

    public void UpdateIcons(ItemManager items)
    {
        for (int i = 0; i < Mathf.Min(items.MaxEquippedItems, uis.Length); i++)
        {
            var ui = uis[i];
            var item = items.GetEquippedItem(i);

            bool visible = item != null;
            if(ui.gameObject.activeSelf != visible)
            {
                ui.gameObject.SetActive(visible);
            }

            if (visible)
            {
                ui.UpdateIconAndSize(item);
            }
        }
    }
}
