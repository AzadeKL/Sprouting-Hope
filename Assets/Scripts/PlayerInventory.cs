using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour, SaveSystem.ISaveable
{

    [SerializeField] private bool inTown;
    // label of item is currently equipped to hand
    public string handItem = "";

    // hotbar that can scroll through
    public List<string> inventoryIndex;

    // hotbar
    [SerializeField] private List<GameObject> hotbar;
    private int hotbarIndex;

    // inventory
    public Transform[] newInventory;


    // sell mode for when at restaurant
    public bool sellMode = false;

    // give mode for when at truck
    public bool giveMode = false;

    public int money = 500;


    public GameObject inventoryIcon;
    [SerializeField] private GameObject handIcon;
    [SerializeField] private PlayerTool playerTool;

    [SerializeField] private GameEvent handChanged;
    [SerializeField] private GameEvent inventoryChanged;

    private GameObject toolTip;//Ui tool tip
    public void Save(GameData gameData)
    {
        gameData.playerInventoryData = new List<string>();
        var data = gameData.playerInventoryData;
        ISaveable.AddKey(data, "handItem", handItem);
        ISaveable.AddKey(data, "money", money);

        gameData.playerInventoryInventoryItems = new List<string>();
        gameData.playerInventoryInventoryQuantities = new List<int>();
        gameData.playerInventoryInventorySlot = new List<int>();
        foreach (Transform gridslot in newInventory)
        {
            if (gridslot.childCount > 0)
            {
                InventoryIcon icon = gridslot.GetChild(0).gameObject.GetComponent<InventoryIcon>();
                // Debug.Log("saving " + icon.item);
                gameData.playerInventoryInventorySlot.Add(Array.IndexOf(newInventory, gridslot));
                gameData.playerInventoryInventoryItems.Add(icon.item);
                gameData.playerInventoryInventoryQuantities.Add(icon.quantity);
            }
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
        //inventory = new Dictionary<string, int>();
        //inventoryIcons = new Dictionary<string, GameObject>();
        if (gameData.playerInventoryInventoryItems.Count != gameData.playerInventoryInventoryQuantities.Count)
        {
            handItem = "";
            return false;
        }
        for (var i = 0; i < gameData.playerInventoryInventoryItems.Count; i++)
        {
            InventoryIcon newIcon = Instantiate(inventoryIcon, newInventory[gameData.playerInventoryInventorySlot[i]]).GetComponent<InventoryIcon>();
            newIcon.InitializeVariables();
            // Debug.Log("initialized");
            newIcon.SetIcon(gameData.playerInventoryInventoryItems[i]);
            newIcon.UpdateQuantity(gameData.playerInventoryInventoryQuantities[i]);
            inventoryChanged.TriggerEvent();
        }

        //SetHandItem(handItem);

        return true;
    }

    public void AddToInventory(string Item, int Count = 1)
    {
        foreach (Transform gridslot in newInventory)
        {
            if (gridslot.childCount > 0)
            {
                InventoryIcon icon = gridslot.GetChild(0).gameObject.GetComponent<InventoryIcon>();
                if (icon.item == Item)
                {
                    icon.UpdateQuantity(Count + icon.quantity);
                    inventoryChanged.TriggerEvent();
                    return;
                }
            }
        }
        foreach (Transform gridslot in newInventory)
        {
            if (gridslot.childCount == 0)
            {
                InventoryIcon newIcon = Instantiate(inventoryIcon, gridslot).GetComponent<InventoryIcon>();
                newIcon.InitializeVariables();
                // Debug.Log("initialized");
                newIcon.SetIcon(Item);
                newIcon.UpdateQuantity(Count);
                inventoryChanged.TriggerEvent();
                return;
            }
        }
    }

    public void RemoveFromInventory(string Item, int Count = 1)
    {
        foreach (Transform gridslot in newInventory)
        {
            if (gridslot.childCount > 0)
            {
                InventoryIcon icon = gridslot.GetChild(0).gameObject.GetComponent<InventoryIcon>();
                if (icon.item == Item)
                {
                    icon.UpdateQuantity(icon.quantity - Count);
                    inventoryChanged.TriggerEvent();
                    return;
                }
            }
        }
    }

    /*public void AddToInventory(string Item, int Count = 1)
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
            newIcon.GetComponent<InventoryIcon>().SetIcon(Item);
            newIcon.GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
            inventoryIcons.Add(Item, newIcon);
        }
        inventoryChanged.TriggerEvent();
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
        inventoryChanged.TriggerEvent();
    }

    // remove said item from the inventory for dragging and other inventories
    public void RemoveFromInventoryOnly(string Item, bool full)
    {
        Debugger.Log("Dragging", Debugger.PriorityLevel.LeastImportant);
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
            //Debug.Log(Mathf.Ceil(inventory[Item] / 2f));
            //Debug.Log(Mathf.Floor(inventory[Item] / 2f));

            if ((int) Mathf.Floor(inventory[Item] / 2f) == 0)
            {
                RemoveFromInventoryOnly(Item, true);
                return;
            }

            icon.GetComponent<InventoryIcon>().UpdateQuantity((int) Mathf.Ceil(inventory[Item] / 2f));
            inventory[Item] = (int) Mathf.Floor(inventory[Item] / 2f);
            inventoryIcons[Item] = Instantiate(inventoryIcon, icon.GetComponent<InventoryIcon>().lastParent);
            inventoryIcons[Item].GetComponent<InventoryIcon>().SetIcon(Item);
            inventoryIcons[Item].GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
        }
        inventoryChanged.TriggerEvent();
    }*/

    public int GetSelectedHotbarIndex()
    {
        return hotbarIndex;
    }

    public void SetHandItem(int hotbarIndex)
    {
        this.hotbarIndex = hotbarIndex;
        UpdateHandItemFromHotbarIndex();
    }

    public void SetHandItem(string Item)
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


        // change hand icon to item icon to display what is currently in hand
        handIcon.GetComponent<Image>().enabled = true;
        handIcon.GetComponent<Image>().sprite = hotbar[hotbarIndex].transform.GetChild(0).gameObject.GetComponent<Image>().sprite;
        playerTool.visual.enabled = true;
        playerTool.visual.sprite = hotbar[hotbarIndex].transform.GetChild(0).gameObject.GetComponent<Image>().sprite;
        handChanged.TriggerEvent(hotbar[hotbarIndex].transform.GetChild(0).gameObject);

        for (int i = 0; i < hotbar.Count; ++i)
        {
            if (hotbar[i].transform.childCount > 0 && hotbar[i].transform.GetChild(0).GetComponent<InventoryIcon>().item == handItem)
            {
                hotbarIndex = i;
                break;
            }
        }
    }

    /*public void ChangeHandItemToPrevItem()
    {
        if (handItem == "") return;

        string newItem = "";
        int handItemIndex = (inventoryIndex.IndexOf(handItem) - 1 + inventoryIndex.Count) % inventoryIndex.Count;
        if (handItem != inventoryIndex[handItemIndex]) newItem = inventoryIndex[handItemIndex];
        SetHandItem(newItem);
    }*/

    private void UpdateHandItemFromHotbarIndex()
    {
        string newItem = "";
        if ((hotbarIndex >= 0) && (hotbar[hotbarIndex].transform.childCount > 0)) newItem = hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item;

        SetHandItem(newItem);
    }

    void Start()
    {
        toolTip = FindObjectOfType<Tooltip>(true).gameObject;

        if (SaveSystem.DataManager.instance.Load(this) == false)
        {
            // set up starting inventory
            AddToInventory("Rusty Shovel");
            AddToInventory("Rusty Watering Can");
            AddToInventory("Rusty Hoe");

            if (!inTown) SetHandItem("Rusty Shovel");
        }

    }


    void Update()
    {
        if (!inTown)
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
    }



    public void FindNextItem(int direction, int tryCount = 0)
    {
        hotbarIndex = (hotbarIndex + direction + hotbar.Count) % hotbar.Count;
        if (hotbar[hotbarIndex].transform.childCount > 0) return;
        if (tryCount > hotbar.Count) return;
        FindNextItem(direction, tryCount + 1);
    }


    public List<InventoryIcon> GetHotbarItems()
    {
        var list = new List<InventoryIcon>();
        foreach (var item in hotbar)
        {
            if (item.transform.childCount > 0)
            {
                var result = item.transform.GetChild(0).GetComponent<InventoryIcon>();
                list.Add(result);

            }
            else
            {
                list.Add(null);
            }


        }
        return list;
    }
}
