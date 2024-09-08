using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour, SaveSystem.ISaveable
{
    [FormerlySerializedAs("inTown")]
    [SerializeField] private bool disableHandItemAndHotbar = false;
    // label of item is currently equipped to hand
    public string handItem = "";

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

            if (!disableHandItemAndHotbar) SetHandItem("Rusty Shovel");
        }

    }

    void Update()
    {
        if (!disableHandItemAndHotbar)
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
                var icon = item.transform.GetChild(0).GetComponent<InventoryIcon>();
                if (icon.quantity <= 0)
                {
                    list.Add(null);
                }
                else
                {
                    list.Add(icon);
                }
            }
            else
            {
                list.Add(null);
            }

        }
        return list;
    }
}
