using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    public Dictionary<string, GameObject> inventoryIcons = new Dictionary<string, GameObject>();

    // sell mode for when at restaurant
    public bool sellMode = false;

    // give mode for when at truck
    public bool giveMode = false;

    public int money = 500;

    private Dictionary<Vector3Int, GameObject> grids;

    [SerializeField] private GameObject inventoryGrid;
    [SerializeField] private GameObject inventoryIcon;
    [SerializeField] private GameObject handIcon;
    [SerializeField] private PlayerTool playerTool;

    [SerializeField] private GameEvent handChanged;

    private GameObject toolTip;//Ui tool tip
    public void Save(GameData gameData)
    {
        var data = gameData.playerInventoryData;
        ISaveable.AddKey(data, "handItem", handItem);
        ISaveable.AddKey(data, "money", money);

        gameData.playerInventoryInventoryKeys = new List<string>();
        gameData.playerInventoryInventoryValues = new List<int>();
        foreach (var key_value in inventory)
        {
            gameData.playerInventoryInventoryKeys.Add(key_value.Key);
            gameData.playerInventoryInventoryValues.Add(key_value.Value);
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
        inventory = new Dictionary<string, int>();
        inventoryIcons = new Dictionary<string, GameObject>();
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
        if (inventory.ContainsKey(Item))
        {
            inventory[Item] += Count;
            inventoryIcons[Item].GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
        }
        else
        {
            inventory.Add(Item, Count);
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
    public void RemoveFromInventory(string Item, int Count = 1)
    {
        // failsafe check in case inventory doesn't have requested item
        if (inventory.ContainsKey(Item))
        {
            // subtract quantity of item by 1
            inventory[Item] -= Count;
            inventoryIcons[Item].GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);

            // if item slot is empty, remove from the dicts and lists and ui panel
            if (inventory[Item] == 0)
            {
                toolTip.SetActive(false);
                GameObject icon = inventoryIcons[Item];
                inventoryIcons.Remove(Item);
                GameObject parent = icon.transform.parent.gameObject;
                //GameObject newParent = Instantiate(parent, parent.transform.parent);
                Destroy(parent.transform.GetChild(0).gameObject);
                //Destroy(parent);
                inventory.Remove(Item);
                inventoryIndex.Remove(Item);

                // update hand icon in case holding last item used (like using last seed)
                if (handItem == Item) { ChangeHandItemToPrevItem(); Debug.Log("changing hands"); }
            }
        }
    }

    // remove said item from the inventory for dragging and other inventories
    public void RemoveFromInventoryOnly(string Item, bool full)
    {
        // left click take all items from inventory, removing from dicts
        if (full)
        {
            if (handItem == Item) { ChangeHandItemToPrevItem(); Debug.Log("changing hands"); }
            toolTip.SetActive(false);
            GameObject icon = inventoryIcons[Item];
            inventoryIcons.Remove(Item);
            inventory.Remove(Item);
            inventoryIndex.Remove(Item);
        }
        // right click take (bigger) half items from inventory
        else
        {
            toolTip.SetActive(false);
            GameObject icon = inventoryIcons[Item];
            Debug.Log(Mathf.Ceil(inventory[Item] / 2f));
            Debug.Log(Mathf.Floor(inventory[Item] / 2f));
            icon.GetComponent<InventoryIcon>().UpdateQuantity((int) Mathf.Ceil(inventory[Item] / 2f));
            inventory[Item] = (int) Mathf.Floor(inventory[Item] / 2f);
            inventoryIcons[Item] = Instantiate(inventoryIcon, icon.GetComponent<InventoryIcon>().lastParent);
            inventoryIcons[Item].GetComponent<InventoryIcon>().SetIcon(Item);
            inventoryIcons[Item].GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
        }
    }

    public void ChangeHandItem(string Item)
    {
        handItem = Item;

        // if nothing in hand, do not show image
        if (Item == "")
        {
            handIcon.GetComponent<Image>().enabled = false;
            playerTool.visual.enabled = false;
            handChanged.TriggerEvent(null);
            return;
        }

        // if the item does not exist, clear the current item
        if (!inventoryIcons.ContainsKey(handItem))
        {
            //Debugger.Log("Invalid Hand Item: " + handItem);
            ChangeHandItem("");
            return;
        }

        // change hand icon to item icon to display what is currently in hand
        handIcon.GetComponent<Image>().enabled = true;
        handIcon.GetComponent<Image>().sprite = inventoryIcons[handItem].GetComponent<Image>().sprite;
        playerTool.visual.enabled = true;
        playerTool.visual.sprite = inventoryIcons[handItem].GetComponent<Image>().sprite;
        handChanged.TriggerEvent(inventoryIcons[Item]);

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
    }

    void Start()
    {
        toolTip = FindObjectOfType<Tooltip>(true).gameObject;

        if (SaveSystem.DataManager.instance.Load(this) == false)
        {
            // set up starting inventory
            AddToInventory("Rusty Shovel");
            AddToInventory("Wheat Seeds");
            AddToInventory("Rusty Watering Can");
            AddToInventory("Rusty Hoe");

            ChangeHandItem("Rusty Shovel");
        }

    }


    void Update()
    {


        // scroll wheel down the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            FindNextItem(-1);
            UpdateHandItemFromHotbarIndex();
        }
        // scroll wheel up the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            FindNextItem(1);
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

    private void FindNextItem(int direction, int tryCount = 0)
    {
        hotbarIndex = (hotbarIndex + direction + hotbar.Count) % hotbar.Count;
        if (hotbar[hotbarIndex].transform.childCount > 0) return;
        if (tryCount > hotbar.Count) return;
        FindNextItem(direction, tryCount + 1);
    }
}
