using SaveSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{


    // index variable labeling what is currently equipped to hand
    public int handIndex = -1;

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
                if (handIndex == inventoryIndex.IndexOf(Item))
                {
                    if (handIndex == 0)
                    {
                        handIndex = -1;
                        ChangeHand("");
                    }
                    else ChangeHand(inventoryIndex[inventoryIndex.IndexOf(Item) - 1]);
                }
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

    public void ChangeHand(string Item)
    {
        GameObject result = null;
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
            playerTool.visual.sprite = inventoryIcons[Item].GetComponent<Image>().sprite;
            result = inventoryIcons[Item];
        }
        handChanged.TriggerEvent(result);
    }

    void Awake()
    {
        // set up starting inventory
        AddToInventory("Rusty Hoe");
        AddToInventory("Wheat Seeds");
        AddToInventory("Rusty Watering Can");
    }

    void Update()
    {


        // scroll wheel down the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (hotbarIndex == 0) hotbarIndex = hotbar.Count - 1;
            else hotbarIndex--;
            if (hotbar[hotbarIndex].transform.childCount == 0) ChangeHand("");
            else
            {
                ChangeHand(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
                handIndex = inventoryIndex.IndexOf(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
            }
            //Debug.Log(hotbarIndex);
        }
        // scroll wheel up the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            hotbarIndex = (hotbarIndex + 1) % hotbar.Count;
            if (hotbar[hotbarIndex].transform.childCount == 0) ChangeHand("");
            else
            {
                ChangeHand(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
                handIndex = inventoryIndex.IndexOf(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
            }
            //Debug.Log(hotbarIndex);
        }
        // number keys for specific hotbar slots
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            hotbarIndex = 0;
            if (hotbar[hotbarIndex].transform.childCount == 0) ChangeHand("");
            else
            {
                ChangeHand(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
                handIndex = inventoryIndex.IndexOf(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
            }
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            hotbarIndex = 1;
            if (hotbar[hotbarIndex].transform.childCount == 0) ChangeHand("");
            else
            {
                ChangeHand(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
                handIndex = inventoryIndex.IndexOf(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
            }
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            hotbarIndex = 2;
            if (hotbar[hotbarIndex].transform.childCount == 0) ChangeHand("");
            else
            {
                ChangeHand(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
                handIndex = inventoryIndex.IndexOf(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
            }
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            hotbarIndex = 3;
            if (hotbar[hotbarIndex].transform.childCount == 0) ChangeHand("");
            else
            {
                ChangeHand(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
                handIndex = inventoryIndex.IndexOf(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
            }
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            hotbarIndex = 4;
            if (hotbar[hotbarIndex].transform.childCount == 0) ChangeHand("");
            else
            {
                ChangeHand(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
                handIndex = inventoryIndex.IndexOf(hotbar[hotbarIndex].transform.GetChild(0).GetComponent<InventoryIcon>().item);
            }
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
