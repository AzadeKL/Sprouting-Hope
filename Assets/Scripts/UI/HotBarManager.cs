using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR;

// TODO: Correct "HotBar" to "Hotbar"
public class HotBarManager : MonoBehaviour
{
    [FormerlySerializedAs("handChanged")]
    [SerializeField] private GameEvent selectedItemChanged;
    // TODO: Replace PlayerInventory with HotbarUser (interface)
    [SerializeField] private PlayerInventory hotbarUser;

    [FormerlySerializedAs("hotbarSelectedImages")]
    [SerializeField] private List<Image> itemImages;
    private int selectedIndex = -1;

    private void Start()
    {
        UpdateAvailableItems();
        DeselectAllItems();
        GetNewSelection();
    }

    private void DeselectAllItems()
    {
        foreach (var item in itemImages)
        {
            item.enabled = false;
        }
    }

    private void UpdateAvailableItems()
    {
        List<InventoryIcon> icons = hotbarUser.GetHotbarItems();

        for (int i = 0; i < Math.Min(icons.Count, itemImages.Count); i++)
        {

            if (icons[i] == null)
            {
                var childImage = itemImages[i].transform.GetChild(0).GetComponentInChildren<Image>(true);
                childImage.enabled = false;
                childImage.overrideSprite = null;
            }
            else
            {
                var childImage = itemImages[i].transform.GetChild(0).GetComponentInChildren<Image>(true);
                childImage.enabled = true;
                childImage.overrideSprite = icons[i].GetComponent<Image>().sprite;
            }

        }

        for (int i = icons.Count; i < itemImages.Count; i++)
        {
            var childImage = itemImages[i].transform.GetChild(0).GetComponentInChildren<Image>(true);
            childImage.enabled = false;
            childImage.overrideSprite = null;
        }
    }

    private void UpdateSelection(int newIndex)
    {
        int lastSelectedIndex = selectedIndex;
        selectedIndex = newIndex;
        if (selectedIndex >= itemImages.Count) selectedIndex = -1;

        if (lastSelectedIndex == selectedIndex) return;
        if (lastSelectedIndex >= 0) itemImages[lastSelectedIndex].enabled = false;
        if (selectedIndex >= 0) itemImages[selectedIndex].enabled = true;
    }

    private int GetNewSelection()
    {
        return hotbarUser.GetSelectedHotbarIndex();
    }

    private int GetNewSelection(GameObject selectedItem)
    {
        if (selectedItem == null) return -1;

        var sprite = selectedItem.GetComponent<InventoryIcon>().GetComponent<Image>().sprite;
        for (int i = 0; i < itemImages.Count; i++)
        {
            var childImage = itemImages[i].transform.GetChild(0).GetComponentInChildren<Image>(true);
            if (childImage.overrideSprite == sprite) return i;
        }

        return -1;
    }

    public void SelectItem(int hotbarIndex)
    {
        // TODO: Configure this as a Component or Trigger
        hotbarUser.SetHandItem(hotbarIndex);
    }

    public void OnSelectionChanged()
    {
        UpdateSelection(GetNewSelection());
    }

    public void OnSelectionChanged(GameObject item)
    {
        UpdateSelection(GetNewSelection(item));
    }

    public void OnAvailableItemsChanged()
    {
        UpdateAvailableItems();
    }

}
