using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{

    // TEMP string for hand
    public string hand = "hoe";


    [SerializeField] private GameObject inventoryUI;

    // index variable labeling what is currently equipped to hand
    public int handIndex = 0;

    // hotbar that can scroll through
    public List<string> hotBar;

    // inventory
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    [SerializeField] private GameObject inventoryGrid;
    [SerializeField] private GameObject inventoryIcon;

    private List<GameObject> inventoryIcons = new List<GameObject>();


    public void AddToInventory(string Item)
    {
        Debug.Log(Item);
        if (inventory.ContainsKey(Item))
        {
            inventory[Item] = inventory[Item] + 1;
            foreach (var icon in inventoryIcons)
            {
                if (icon.GetComponent<InventoryIcon>().item == Item)
                {
                    icon.GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
                    break;
                }
            }
        }
        else
        {
            inventory.Add(Item, 1);
            GameObject newIcon = Instantiate(inventoryIcon, inventoryGrid.transform.GetChild(0).transform);
            StretchAndFill(newIcon.GetComponent<RectTransform>());
            newIcon.GetComponent<InventoryIcon>().SetIcon(Item);
            newIcon.GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
            inventoryIcons.Add(newIcon);
        }
        Debug.Log(Item + ", " + inventory[Item]);
    }

    void Awake()
    {
        // set up starting inventory
        hotBar = new List<string> { "hoe", "watering can", "wheat seeds", "", "" };
    }

    void Update()
    {
        // enable/disable inventory window
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }

        // scroll wheel down the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (handIndex == 0) handIndex = 4;
            else handIndex--;
            Debug.Log(handIndex);
        }
        // scroll wheel up the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            handIndex = (handIndex + 1) % 5;
            Debug.Log(handIndex);
        }
        // number keys for specific hotbar slots
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            handIndex = 0;
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            handIndex = 1;
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            handIndex = 2;
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            handIndex = 3;
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            handIndex = 4;
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
        rectTransform.anchorMin = new Vector2(0, 0); // Bottom-left corner
        rectTransform.anchorMax = new Vector2(1, 1); // Top-right corner

        // Reset offsets
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
