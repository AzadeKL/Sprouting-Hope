using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour, SaveSystem.ISaveable
{
    [FormerlySerializedAs("inTown")]
    [SerializeField] private bool disableHandItemAndHotbar = false;

    // hotbar
    [SerializeField] private int hotbarIndex = -1; // Index of current hand item
    [SerializeField] private List<GameObject> hotbar;

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

    [FormerlySerializedAs("handChanged")]
    [SerializeField] private GameEvent handItemChanged;
    [SerializeField] private GameEvent inventoryChanged;

    private GameObject toolTip;//Ui tool tip
    public void Save(GameData gameData)
    {
        gameData.playerInventoryMoney = money;
        if (!disableHandItemAndHotbar) gameData.playerInventoryHotbarIndex = hotbarIndex;

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
        money = gameData.playerInventoryMoney ;
        if (!disableHandItemAndHotbar) hotbarIndex = gameData.playerInventoryHotbarIndex;

        if (gameData.playerInventoryInventoryItems.Count != gameData.playerInventoryInventoryQuantities.Count)
        {
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

        SetHandItem(hotbarIndex);

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

    private string GetHotbarItemName(int hotbarIndex)
    {
        var icon = GetHotbarIcon(hotbarIndex);
        return (icon != null) ? icon.item : "";
    }

    public string GetHandItemName()
    {
        return GetHotbarItemName(hotbarIndex);
    }

    public int GetSelectedHotbarIndex()
    {
        return hotbarIndex;
    }

    public void SetHandItem(int hotbarIndex)
    {
        if (disableHandItemAndHotbar) return;

        this.hotbarIndex = (hotbarIndex >= hotbar.Count) ? -1 : hotbarIndex;

        var icon = GetHotbarIcon(this.hotbarIndex);
        if (icon == null)
        {
            handIcon.GetComponent<Image>().enabled = false;
            handIcon.GetComponent<Image>().sprite = null;
            playerTool.visual.enabled = false;
            playerTool.visual.sprite = null;
        }
        else
        {
            var sprite = icon.GetComponent<Image>().sprite;
            handIcon.GetComponent<Image>().enabled = true;
            handIcon.GetComponent<Image>().sprite = sprite;
            playerTool.visual.enabled = true;
            playerTool.visual.sprite = sprite;
        }

        handItemChanged.TriggerEvent();
    }

    public void SetHandItem(string item)
    {
        if (disableHandItemAndHotbar) return;

        int index = -1;

        if (item != "")
        {
            for (int i = 0; i < hotbar.Count; ++i)
            {
                if (hotbar[i].transform.childCount <= 0) continue;
                var icon = hotbar[i].transform.GetChild(0).GetComponent<InventoryIcon>();
                if ((icon == null) || (icon.quantity <= 0)) continue;
                if (icon.item == item)
                {
                    index = i;
                    break;
                }
            }
        }

        SetHandItem(index);
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
        if (disableHandItemAndHotbar) return;

        // scroll wheel down the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            SetNextHotbarItem(-1);
        }
        // scroll wheel up the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            SetNextHotbarItem(1);
        }
        // number keys for specific hotbar slots
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            SetHandItem(0);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            SetHandItem(1);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            SetHandItem(2);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            SetHandItem(3);
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            SetHandItem(4);
        }
    }

    public void SetNextHotbarItem(int direction)
    {
        for (int i = 1; i < hotbar.Count; i++)
        {
            int index = (hotbarIndex + hotbar.Count + i * direction) % hotbar.Count;
            var icon = GetHotbarIcon(index);
            if (icon != null)
            {
                SetHandItem(index);
                return;
            }
        }
    }

    private InventoryIcon GetHotbarIcon(int index)
    {
        if ((index < 0) || (index >= hotbar.Count)) return null;
        if (hotbar[index].transform.childCount <= 0) return null;
        var icon = hotbar[index].transform.GetChild(0).GetComponent<InventoryIcon>();
        return (icon.quantity > 0)  ? icon : null;
    }

    public List<InventoryIcon> GetHotbarItems()
    {
        var list = new List<InventoryIcon>(hotbar.Count);
        for (var i = 0; i < hotbar.Count; i++) list.Add(GetHotbarIcon(i));
        return list;
    }
}
