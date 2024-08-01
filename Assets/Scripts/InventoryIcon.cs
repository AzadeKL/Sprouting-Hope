using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;


public class InventoryIcon : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    [SerializeField] private List<Sprite> imageicons;
    public TextMeshProUGUI quantity;

    public string item;
    public int sellValue;
    private GameObject player;
    private GameObject toolTip;

    private RectTransform rectTransform;
    public Transform lastParent;
    private Canvas canvas;

    private int dragged = -1;


    private void Awake()
    {
        toolTip = GameObject.Find("UI").transform.GetChild(4).gameObject;
        player = GameObject.Find("Player");
        rectTransform = transform.GetComponent<RectTransform>();
        canvas = rectTransform.root.GetComponent<Canvas>();
    }

    public void UpdateQuantity(int amount)
    {
        quantity.text = amount.ToString();
        if (amount <= 1)
        {
            quantity.gameObject.transform.parent.gameObject.GetComponent<Image>().enabled = false;
            quantity.enabled = false;
        }
        else
        {
            quantity.gameObject.transform.parent.gameObject.GetComponent<Image>().enabled = true;
            quantity.enabled = true;
        }
    }

    public void SetIcon(string tag)
    {
        item = tag;
        switch (tag)
        {
            case "Rusty Hoe":
            GetComponent<Image>().sprite = imageicons[0];
            break;
            case "Bronze Hoe":
            GetComponent<Image>().sprite = imageicons[1];
            break;
            case "Silver Hoe":
            GetComponent<Image>().sprite = imageicons[2];
            break;
            case "Gold Hoe":
            GetComponent<Image>().sprite = imageicons[3];
            break;
            case "Rusty Watering Can":
            GetComponent<Image>().sprite = imageicons[4];
            break;
            case "Bronze Watering Can":
            GetComponent<Image>().sprite = imageicons[5];
            break;
            case "Silver Watering Can":
            GetComponent<Image>().sprite = imageicons[6];
            break;
            case "Gold Watering Can":
            GetComponent<Image>().sprite = imageicons[7];
            break;
            case "Wheat Seeds":
            GetComponent<Image>().sprite = imageicons[8];
            sellValue = 1;
            break;
            case "Tomato Seeds":
            GetComponent<Image>().sprite = imageicons[9];
            sellValue = 5;
            break;
            case "Lentils Seeds":
            GetComponent<Image>().sprite = imageicons[10];
            sellValue = 8;
            break;
            case "Wheat":
            GetComponent<Image>().sprite = imageicons[11];
            sellValue = 100;
            break;
            case "Tomato":
            GetComponent<Image>().sprite = imageicons[12];
            sellValue = 100;
            break;
            case "Lentil":
            GetComponent<Image>().sprite = imageicons[13];
            sellValue = 150;
            break;
            case "Egg":
            GetComponent<Image>().sprite = imageicons[14];
            sellValue = 200;
            break;
            case "Chicken":
            GetComponent<Image>().sprite = imageicons[15];
            sellValue = 500;
            break;
            case "Pig":
            GetComponent<Image>().sprite = imageicons[16];
            sellValue = 700;
            break;
            case "Cow":
            GetComponent<Image>().sprite = imageicons[17];
            sellValue = 1000;
            break;
            default:
            GetComponent<Image>().sprite = imageicons[0];
            break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (player.GetComponent<PlayerInventory>().sellMode && sellValue > 0) toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item + "\n$" + sellValue;
        else toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item;
        if (dragged < 0) toolTip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toolTip.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // dragging with left, clicking with right to drop one
        if (dragged == 0 && eventData.button == PointerEventData.InputButton.Right)
        {
            UpdateQuantity(int.Parse(quantity.text) - 1);
            player.GetComponent<PlayerInventory>().AddToInventory(item);
            if (int.Parse(quantity.text) <= 0) Destroy(this.gameObject);
        }
        // dragging with right, clicking with left to drop one
        if (dragged == 1 && eventData.button == PointerEventData.InputButton.Left)
        {
            UpdateQuantity(int.Parse(quantity.text) - 1);
            player.GetComponent<PlayerInventory>().AddToInventory(item);
            if (int.Parse(quantity.text) <= 0) Destroy(this.gameObject);
        }
        // clicking normally to select or sell item
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // put selected item in hand
            if (!player.GetComponent<PlayerInventory>().sellMode)
            {
                player.GetComponent<PlayerInventory>().ChangeHandItem(item);
            }
            // if on sell mode, sell non-tool item
            else if (imageicons.IndexOf(GetComponent<Image>().sprite) > 7)
            {
                player.GetComponent<PlayerInventory>().RemoveFromInventory(item);
                Debug.Log(item + " was sold for $" + sellValue);
                player.GetComponent<PlayerInventory>().money += sellValue;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastParent = transform.parent;
        transform.SetParent(rectTransform.root, true);
        // dragging with left click
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            player.GetComponent<PlayerInventory>().RemoveFromInventoryOnly(item, true);
            dragged = 0;
        }
        // dragging with right click
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            player.GetComponent<PlayerInventory>().RemoveFromInventoryOnly(item, false);
            dragged = 1;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragged = -1;
        // if item still exists in inventory (right click dragging), merge items back to one slot
        if (player.GetComponent<PlayerInventory>().InventoryContainsItem(item))
        {
            player.GetComponent<PlayerInventory>().AddToInventory(item, int.Parse(quantity.text));
            //Destroy(this.gameObject);
        }
        // put item in new slot and add to inventory dicts again
        else
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (RaycastResult result in results)
            {

                if (result.gameObject != null && result.gameObject.GetComponent<RectTransform>() != null)
                {
                    //Debug.Log("UI Element clicked: " + result.gameObject.name);
                    if (result.gameObject.name.Contains("GridCell") && result.gameObject.transform.childCount == 0)
                    {
                        lastParent = result.gameObject.transform;
                    }
                }
            }
            transform.SetParent(lastParent, true);
            rectTransform.localPosition = Vector3.zero;
            //TODO
            var playerInventory = player.GetComponent<PlayerInventory>();
            playerInventory.AddToInventory(this, item, int.Parse(quantity.text));
            //playerInventory.inventor.inventory.Add(item, int.Parse(quantity.text));
            player.GetComponent<PlayerInventory>().inventoryIndex.Add(item);
            // player.GetComponent<PlayerInventory>().inventoryIcons.Add(item, this.gameObject);
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void ChangeParentColor(float colorAlpha)
    {
        return;
        var oldColor = lastParent.GetComponent<Image>().color;
        oldColor.a = colorAlpha;
        lastParent.GetComponent<Image>().color = oldColor;
    }
}
