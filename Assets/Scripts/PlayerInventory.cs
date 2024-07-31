using SaveSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{


    // label of item is currently equipped to hand
    public string handItem = "";

    // hotbar that can scroll through
    public List<string> inventoryIndex;

    // hotbar
    public List<GameObject> hotbar;
    public int hotbarIndex;

    // inventory
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    private Dictionary<string, GameObject> inventoryIcons = new Dictionary<string, GameObject>();

    // sell mode for when at restaurant
    public bool sellMode = false;

    public int money = 500;

    [SerializeField] private GameObject inventoryGrid;
    [SerializeField] private GameObject inventoryIcon;
    [SerializeField] private GameObject handIcon;
    [SerializeField] private PlayerTool playerTool;
  
    [SerializeField] private GameEvent handChanged;

    public void AddToInventory(string Item)
    {
        //Debug.Log(Item);
        if (inventory.ContainsKey(Item))
        {
            inventory[Item]++;
        }
        else
        {
            inventory.Add(Item, 1);
            inventoryIndex.Add(Item);
            int i = 0;
            while (inventoryGrid.transform.GetChild(i).transform.childCount != 0) i++;
            GameObject newIcon = Instantiate(inventoryIcon, inventoryGrid.transform.GetChild(i).transform);
            StretchAndFill(newIcon.GetComponent<RectTransform>());
            newIcon.GetComponent<InventoryIcon>().SetIcon(Item);
            newIcon.GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
            inventoryIcons.Add(Item, newIcon);
        }
        //Debug.Log(Item + ", " + inventory[Item]);
    }

    // remove one of said item from the inventory, if last item was removed, remove the spot from the grid
    public void RemoveFromInventory(string Item)
    {
        // failsafe check in case inventory doesn't have requested item
        if (inventory.ContainsKey(Item))
        {
            // subtract quantity of item by 1
            inventory[Item]--;
            inventoryIcons[Item].GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);

            // if item slot is empty, remove from the dicts and lists and ui panel
            if (inventory[Item] == 0)
            {
                // update hand icon in case holding last item used (like using last seed)
                ChangeHandItemToPrevItem();

                inventoryGrid.transform.parent.parent.parent.GetChild(4).gameObject.SetActive(false);
                GameObject icon = inventoryIcons[Item];
                inventoryIcons.Remove(Item);
                GameObject parent = icon.transform.parent.gameObject;
                //GameObject newParent = Instantiate(parent, parent.transform.parent);
                Destroy(parent.transform.GetChild(0).gameObject);
                //Destroy(parent);
                inventory.Remove(Item);
                inventoryIndex.Remove(Item);
            }
        }
    }

    public void ChangeHandItem(string Item)
    {
        GameObject result = null;
        handItem = Item;

        // if nothing in hand, do not show image
        if (Item == "")
        {
            handIcon.GetComponent<Image>().enabled = false;
            handChanged.TriggerEvent(null);
            return;
        }

        // if the item does not exist, clear the current item
        if (!inventoryIcons.ContainsKey(handItem))
        {
            Debugger.Log("Invalid Hand Item: " + handItem);
            ChangeHandItem("");
            return;
        }

        // change hand icon to item icon to display what is currently in hand
        handIcon.GetComponent<Image>().enabled = true;
        handIcon.GetComponent<Image>().sprite = inventoryIcons[handItem].GetComponent<Image>().sprite;
        playerTool.visual.sprite = inventoryIcons[handItem].GetComponent<Image>().sprite;
        handChanged.TriggerEvent(inventoryIcons[Item]);

        for (int i = 0; i < hotbar.Count; ++i)
        {
            if (hotbar[i].transform.GetChild(0).GetComponent<InventoryIcon>().item == handItem)
            {
                hotbarIndex = i;
                break;
            }
        }
    }

public void ChangeHandItemToPrevItem()
    {
        if (handItem == "") return;

        string newItem = "";
        int handItemIndex = (inventoryIndex.IndexOf(handItem) - 1 + inventoryIndex.Count) % inventoryIndex.Count;
        if (handItem != inventoryIndex[handItemIndex]) newItem = inventoryIndex[handItemIndex];
        ChangeHandItem(newItem);
    }

    private void UpdateHandItemFromHotbarIndex()
    {
        string newItem = "";
        if ((hotbarIndex >= 0) && (hotbar[hotbarIndex].transform.childCount > 0)) newItem = hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item;

        ChangeHandItem(newItem);
        Debug.Log(hotbarIndex);
    }

    void Awake()
    {
        // set up starting inventory
        AddToInventory("Rusty Hoe");
        AddToInventory("Wheat Seeds");
        AddToInventory("Rusty Watering Can");

        ChangeHandItem("Rusty Hoe");
    }

    void Update()
    {


        // scroll wheel down the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            hotbarIndex = (hotbarIndex - 1 + hotbar.Count) % hotbar.Count;
            UpdateHandItemFromHotbarIndex();
        }
        // scroll wheel up the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            hotbarIndex = (hotbarIndex + 1) % hotbar.Count;
            UpdateHandItemFromHotbarIndex();
        }
        // number keys for specific hotbar slots
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            hotbarIndex = 0;
            UpdateHandItemFromHotbarIndex();
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            hotbarIndex = 1;
            UpdateHandItemFromHotbarIndex();
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            hotbarIndex = 2;
            UpdateHandItemFromHotbarIndex();
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            hotbarIndex = 3;
            UpdateHandItemFromHotbarIndex();
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            hotbarIndex = 4;
            UpdateHandItemFromHotbarIndex();
        }
    }


    public void StretchAndFill(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform is null!");
            return;
        }

        // Set anchors to stretch in all directions
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Bottom-left corner
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f); // Top-right corner      

        // Reset offsets
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        rectTransform.sizeDelta = new Vector2(100, 200);
    }
}
