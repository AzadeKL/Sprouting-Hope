using SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public struct InventoryItem
{
    public int count;
    public string name;
    public InventoryIcon InventoryIcon;

    public InventoryItem(string name, int count, InventoryIcon icon)
    {
        this.count = count;
        this.name = name;
        this.InventoryIcon = icon;
    }

    public void AddItem(int count)
    {
        this.count += count;
    }

    public void RemoveItem(int count)
    {
        this.count -= count;
        count = Math.Max(count, 0);
    }
}

public class PlayerInventory : MonoBehaviour, SaveSystem.ISaveable
{


    // label of item is currently equipped to hand
    public string handItem = "";

    // hotbar that can scroll through
    public List<string> inventoryIndex;

    // hotbar
    public List<GameObject> hotbar;
    public int hotbarIndex;

    // inventory
    // private Dictionary<string, int> inventory = new Dictionary<string, int>();
    public List<InventoryItem> inventory = new();
    // public Dictionary<string, GameObject> inventoryIcons = new Dictionary<string, GameObject>();

    // sell mode for when at restaurant
    public bool sellMode = false;

    public int money = 500;

    private Dictionary<Vector3Int, GameObject> grids;

    [SerializeField] private GameObject inventoryGrid;
    [SerializeField] private GameObject inventoryIcon;
    [SerializeField] private GameObject handIcon;
    [SerializeField] private PlayerTool playerTool;

    [SerializeField] private GameEvent handChanged;
    public void Save(GameData gameData)
    {
        var data = gameData.playerInventoryData;
        ISaveable.AddKey(data, "handItem", handItem);
        ISaveable.AddKey(data, "money", money);

        gameData.playerInventoryInventoryKeys = new List<string>();
        gameData.playerInventoryInventoryValues = new List<int>();
        foreach (var key_value in inventory)
        {
            //TODO
            //gameData.playerInventoryInventoryKeys.Add(key_value.Key);
            //gameData.playerInventoryInventoryValues.Add(key_value.Value);
        }
    }

    public bool Load(GameData gameData)
    {
        foreach (var key_value in gameData.playerInventoryData)
        {
            var parsed = ISaveable.ParseKey(key_value);
            switch (parsed[0])
            {
                case "handItem":
                handItem = parsed[1];
                break;
                case "money":
                money = Convert.ToInt32(parsed[1]);
                break;
                default:
                Debugger.Log("Invalid key for class (" + this.GetType().Name + "): " + key_value);
                break;
            }
        }

        inventoryIndex = new List<string>();
        inventory = new();
        //inventoryIcons = new Dictionary<string, GameObject>();
        if (gameData.playerInventoryInventoryKeys.Count != gameData.playerInventoryInventoryValues.Count)
        {
            handItem = "";
            return false;
        }
        for (var i = 0; i < gameData.playerInventoryInventoryKeys.Count; i++)
        {
            AddToInventory(gameData.playerInventoryInventoryKeys[i], gameData.playerInventoryInventoryValues[i]);
        }

        ChangeHandItem(handItem);

        return true;
    }

    public void AddToInventory(string Item, int Count = 1)
    {
        //Debug.Log(Item);
        if (InventoryContainsItem(Item))
        {
            InventoryAddItem(Item, Count);
            // InventoryItemGetIcon(Item).GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
        }
        else
        {
            int i = 0;
            //TODO This feels wrong
            while (inventoryGrid.transform.GetChild(i).transform.childCount != 0) i++;
            InventoryIcon newIcon = Instantiate(inventoryIcon, inventoryGrid.transform.GetChild(i).transform).GetComponent<InventoryIcon>();
            var newItem = new InventoryItem(Item, Count, newIcon);
            inventory.Add(newItem);

            inventoryIndex.Add(Item);




            StretchAndFill(newIcon.GetComponent<RectTransform>());
            newIcon.SetIcon(Item);
            newIcon.UpdateQuantity(newItem.count);
            //inventoryIcons.Add(Item, newIcon);
        }
        //Debug.Log(Item + ", " + inventory[Item]);
    }

    public void AddToInventory(InventoryIcon icon, string name, int Count = 1)
    {
        var newItem = new InventoryItem(name, Count, icon);
        inventory.Add(newItem);

    }

    // remove one of said item from the inventory, if last item was removed, remove the spot from the grid
    public void RemoveFromInventory(string Item, int Count = 1)
    {
        // failsafe check in case inventory doesn't have requested item
        if (InventoryContainsItem(Item))
        {
            InventoryRemoveItem(Item, Count);

            if (InventoryGetItemCount(Item) == 0)
            {
                // update hand icon in case holding last item used (like using last seed)
                ChangeHandItemToPrevItem();

                inventoryGrid.transform.parent.parent.parent.GetChild(4).gameObject.SetActive(false);

                InventoryIcon icon = InventoryItemGetIcon(Item);
                GameObject parent = icon.transform.parent.gameObject;
                Destroy(parent.transform.GetChild(0).gameObject);
                InventoryRemove(Item);
                inventoryIndex.Remove(Item);
            }
        }
    }

    // remove said item from the inventory for dragging and other inventories
    public void RemoveFromInventoryOnly(string Item, bool full)
    {
        // left click take all items from inventory, removing from dicts
        if (full)
        {
            ChangeHandItemToPrevItem();
            inventoryGrid.transform.parent.parent.parent.GetChild(4).gameObject.SetActive(false);
            InventoryIcon icon = InventoryItemGetIcon(Item);
            InventoryRemove(Item);
            Debugger.Log(InventoryContainsItem(Item), Debugger.PriorityLevel.Medium);
            inventoryIndex.Remove(Item);
        }
        // right click take (bigger) half items from inventory
        else
        {
            //inventoryGrid.transform.parent.parent.parent.GetChild(4).gameObject.SetActive(false);
            //GameObject icon = inventoryIcons[Item];
            //icon.GetComponent<InventoryIcon>().UpdateQuantity((int) Mathf.Ceil(inventory[Item] / 2f));
            //inventory[Item] = (int) Mathf.Floor(inventory[Item] / 2f);
            //inventoryIcons[Item] = Instantiate(inventoryIcon, icon.GetComponent<InventoryIcon>().lastParent);
            //inventoryIcons[Item].GetComponent<InventoryIcon>().SetIcon(Item);
            //inventoryIcons[Item].GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
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
        if (InventoryContainsItem(handItem) == false)
        {
            //Debugger.Log("Invalid Hand Item: " + handItem);
            ChangeHandItem("");
            return;
        }

        // change hand icon to item icon to display what is currently in hand
        handIcon.GetComponent<Image>().enabled = true;
        handIcon.GetComponent<Image>().sprite = InventoryItemGetIcon(handItem).GetComponent<Image>().sprite;
        playerTool.visual.sprite = InventoryItemGetIcon(handItem).GetComponent<Image>().sprite;
        handChanged.TriggerEvent(InventoryItemGetIcon(handItem).gameObject);

        for (int i = 0; i < hotbar.Count; ++i)
        {
            if (hotbar[i].transform.childCount > 0 && hotbar[i].transform.GetChild(0).GetComponent<InventoryIcon>().item == handItem)
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
        //Debug.Log(hotbarIndex);
    }

    void Awake()
    {


    }
    private void Start()
    {
        if (SaveSystem.DataManager.instance.Load(this) == false)
        {
            // set up starting inventory
            AddToInventory("Rusty Hoe");
            AddToInventory("Wheat Seeds");
            AddToInventory("Rusty Watering Can");

            ChangeHandItem("Rusty Hoe");
        }
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

    public bool InventoryContainsItem(string item)
    {
        return inventory.Any(pair => pair.name == item);
    }

    private void InventoryRemove(string item)
    {
        var result = inventory.First(l => l.name == item);
        inventory.Remove(result);
        Destroy(result.InventoryIcon);
    }
    private void InventoryAddItem(string item, int count = 1)
    {
        var result = inventory.First(l => l.name == item);
        result.AddItem(count);
        result.InventoryIcon.UpdateQuantity(result.count);
    }
    private void InventoryRemoveItem(string item, int count = 1)
    {
        var result = inventory.First(l => l.name == item);
        result.RemoveItem(count);
        result.InventoryIcon.UpdateQuantity(result.count);
    }

    private int InventoryGetItemCount(string item)
    {
        var result = inventory.First(l => l.name == item);
        return result.count;
    }

    private InventoryIcon InventoryItemGetIcon(string item)
    {
        var result = inventory.First(l => l.name == item);
        return result.InventoryIcon;
    }

}
