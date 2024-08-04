using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotBarManager : MonoBehaviour
{
    [SerializeField] private GameEvent handChanged;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerInventory playerInventory;

    [SerializeField] private List<Image> hotbarSelectedImages;


    private void Start()
    {
        GetIcons();
        foreach (var item in hotbarSelectedImages)
        {
            item.enabled = false;
        }
    }

    private void GetIcons()
    {
        List<InventoryIcon> icons = playerInventory.GetHotBarItems();
        for (int i = 0; i < icons.Count; i++)
        {

            if (icons[i] == null)
            {
                var childImage = hotbarSelectedImages[i].transform.GetChild(0).GetComponentInChildren<Image>(true);
                childImage.enabled = false;
                childImage.overrideSprite = null;
            }
            else
            {
                var childImage = hotbarSelectedImages[i].transform.GetChild(0).GetComponentInChildren<Image>(true);
                childImage.enabled = true;
                childImage.overrideSprite = icons[i].GetComponent<Image>().sprite;
            }
        }
    }




    public void ChangeHandItem(int index)
    {
        playerInventory.hotbarIndex = index;
        playerInventory.UpdateHandItemFromHotbarIndex();
    }

    public void OnHandChanged(GameObject hand)
    {

        foreach (var item in hotbarSelectedImages)
        {
            item.enabled = false;
        }

        GetIcons();

        if (hand == null) return;

        var result = hand.GetComponent<InventoryIcon>();
        var sprite = result.GetComponent<Image>().sprite;

        foreach (var item in hotbarSelectedImages)
        {
            var childImage = item.transform.GetChild(0).GetComponentInChildren<Image>(true);
            if (childImage.overrideSprite == sprite)
            {
                item.enabled = true;
            }
        }

    }

    public void OnInventoryChanged()
    {
        GetIcons();

        if (playerInventory.hotbar[playerInventory.hotbarIndex].transform.childCount == 0)
        {
            foreach (var item in hotbarSelectedImages)
            {
                item.enabled = false;
            }
        }

    }

}
