using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{

    public GameObject inventoryUI;

    // index variable labeling what is currently equipped to hand
    public int handIndex = -1;

    // hotbar that can scroll through
    public List<string> inventoryIndex;

    // inventory
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    private Dictionary<string, GameObject> inventoryIcons = new Dictionary<string, GameObject>();



    [SerializeField] private GameObject inventoryGrid;
    [SerializeField] private GameObject inventoryIcon;
    [SerializeField] private GameObject handIcon;



    public void AddToInventory(string Item)
    {
        //Debug.Log(Item);
        if (inventory.ContainsKey(Item))
        {
            inventory[Item]++;
            inventoryIcons[Item].GetComponent<InventoryIcon>().UpdateQuantity(inventory[Item]);
        }
        else
        {
            inventory.Add(Item, 1);
            inventoryIndex.Add(Item);
            GameObject newIcon = Instantiate(inventoryIcon, inventoryGrid.transform.GetChild(inventoryIndex.Count - 1).transform);
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
                if (handIndex == inventoryIndex.IndexOf(Item))
                {
                    if (handIndex == 0) 
                    {
                        handIndex = -1;
                        ChangeHand("");
                    }
                    else ChangeHand(inventoryIndex[inventoryIndex.IndexOf(Item) - 1]);
                }
                GameObject icon = inventoryIcons[Item];
                inventoryIcons.Remove(Item);
                GameObject parent = icon.transform.parent.gameObject;
                Destroy(icon);
                Instantiate(parent, parent.transform.parent);
                Destroy(parent);
                inventory.Remove(Item);
                inventoryIndex.Remove(Item);
            }
        }
    }

    public void ChangeHand(string Item)
    {
        // if nothing in hand, do not show image
        if (Item == "")
        {
            handIcon.GetComponent<Image>().enabled = false;
        }
        // change hand icon to item icon to display what is currently in hand
        else
        {
            handIcon.GetComponent<Image>().enabled = true;
            handIcon.GetComponent<Image>().sprite = inventoryIcons[Item].GetComponent<Image>().sprite;
        }
    }

    void Awake()
    {
        // set up starting inventory
        AddToInventory("Hoe");
        AddToInventory("Wheat Seeds");
        AddToInventory("Watering Can");
    }

    void Update()
    {
        // enable/disable inventory window
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }

        /*
        // scroll wheel down the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (handIndex == 0) handIndex = inventory.Count - 1;
            else handIndex--;
            Debug.Log(handIndex);
        }
        // scroll wheel up the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            handIndex = (handIndex + 1) % inventory.Count;
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
        }*/
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
