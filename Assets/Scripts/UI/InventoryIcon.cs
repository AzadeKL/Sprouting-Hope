using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;


public class InventoryIcon : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    [SerializeField] private GameEvent inventoryChangedEvent;
    [SerializeField] private List<Sprite> imageicons;
    public TextMeshProUGUI quantity;

    public string item;
    public int sellValue;
    public int giveValue;
    private GameObject player;
    private GameObject toolTip;

    private RectTransform rectTransform;
    public Transform lastParent;
    private Canvas canvas;

    private int dragged = -1;


    private void Awake()
    {
        player = GameObject.Find("Player");
        rectTransform = transform.GetComponent<RectTransform>();
        canvas = rectTransform.root.GetComponent<Canvas>();
    }

    private void Start()
    {
        toolTip = FindObjectOfType<Tooltip>(true).gameObject;
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
            case "Rusty Shovel":
            GetComponent<Image>().sprite = imageicons[8];
            break;
            case "Bronze Shovel":
            GetComponent<Image>().sprite = imageicons[9];
            break;
            case "Silver Shovel":
            GetComponent<Image>().sprite = imageicons[10];
            break;
            case "Gold Shovel":
            GetComponent<Image>().sprite = imageicons[11];
            break;
            case "Tomato":
            GetComponent<Image>().sprite = imageicons[12];
            sellValue = 6;
            giveValue = 90;
            break;
            case "Lentil":
            GetComponent<Image>().sprite = imageicons[13];
            sellValue = 10;
            giveValue = 100;
            break;
            case "Egg":
            GetComponent<Image>().sprite = imageicons[14];
            sellValue = 200;
            giveValue = 100;
            break;
            case "Chicken":
            GetComponent<Image>().sprite = imageicons[15];
            sellValue = 80;
            giveValue = 120;
            break;
            case "Pig":
            GetComponent<Image>().sprite = imageicons[16];
            sellValue = 700;
            giveValue = 700;
            break;
            case "Cow":
            GetComponent<Image>().sprite = imageicons[17];
            sellValue = 1000;
            giveValue = 700;
            break;

            case "Wheat Seeds":
            GetComponent<Image>().sprite = imageicons[18];
            sellValue = 1;
            giveValue = 1;
            break;
            case "Tomato Seeds":
            GetComponent<Image>().sprite = imageicons[19];
            sellValue = 1;
            giveValue = 1;
            break;
            case "Lentils Seeds":
            GetComponent<Image>().sprite = imageicons[20];
            sellValue = 1;
            giveValue = 1;
            break;
            case "Wheat":
            GetComponent<Image>().sprite = imageicons[21];
            sellValue = 2;
            giveValue = 50;
            break;
            default:
            GetComponent<Image>().sprite = imageicons[22];
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
        else if (dragged == 1 && eventData.button == PointerEventData.InputButton.Left)
        {
            UpdateQuantity(int.Parse(quantity.text) - 1);
            player.GetComponent<PlayerInventory>().AddToInventory(item);
            if (int.Parse(quantity.text) <= 0) Destroy(this.gameObject);
        }
        // clicking normally to select or sell item
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // if on sell mode, sell non-tool item
            if (player.GetComponent<PlayerInventory>().sellMode && imageicons.IndexOf(GetComponent<Image>().sprite) > 11)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Debug.Log(item + " was sold for $" + (sellValue * int.Parse(quantity.text)));
                    player.GetComponent<PlayerInventory>().money += (sellValue * int.Parse(quantity.text));
                    player.GetComponent<PlayerInventory>().RemoveFromInventory(item, int.Parse(quantity.text));
                }
                else
                {
                    player.GetComponent<PlayerInventory>().RemoveFromInventory(item);
                    Debug.Log(item + " was sold for $" + sellValue);
                    player.GetComponent<PlayerInventory>().money += sellValue;
                }
            }
            // if on give mode, give away non-tool item
            else if (player.GetComponent<PlayerInventory>().giveMode && imageicons.IndexOf(GetComponent<Image>().sprite) > 11)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Debug.Log(item + " was given away for " + (giveValue * int.Parse(quantity.text)) + " goodness points!");
                    GameObject.Find("GameManager").GetComponent<GameManager>().mainProgress += (giveValue * int.Parse(quantity.text));
                    player.GetComponent<PlayerInventory>().RemoveFromInventory(item, int.Parse(quantity.text));
                }
                else
                {
                    Debug.Log(item + " was given away for " + giveValue + " goodness points!");
                    GameObject.Find("GameManager").GetComponent<GameManager>().mainProgress += giveValue;
                    player.GetComponent<PlayerInventory>().RemoveFromInventory(item);
                }

            }
            // put selected item in hand
            else if (!player.GetComponent<PlayerInventory>().sellMode && !player.GetComponent<PlayerInventory>().giveMode)
            {
                player.GetComponent<PlayerInventory>().ChangeHandItem(item);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (player.GetComponent<PlayerInventory>().sellMode && imageicons.IndexOf(GetComponent<Image>().sprite) > 11)
            {
                var control = Input.GetKey(KeyCode.LeftControl);
                int repeat = control == true ? (int) Mathf.Min(25, int.Parse(quantity.text)) : 5;
                for (int i = 0; i < repeat; i++)
                {
                    if (!player.GetComponent<PlayerInventory>().inventory.ContainsKey(item)) break;

                    player.GetComponent<PlayerInventory>().RemoveFromInventory(item);
                    player.GetComponent<PlayerInventory>().money += sellValue;
                }
            }
            else if (player.GetComponent<PlayerInventory>().giveMode && imageicons.IndexOf(GetComponent<Image>().sprite) > 11)
            {
                var control = Input.GetKey(KeyCode.LeftControl);
                int repeat = control == true ? (int) Mathf.Min(25, int.Parse(quantity.text)) : 5;
                for (int i = 0; i < repeat; i++)
                {
                    if (!player.GetComponent<PlayerInventory>().inventory.ContainsKey(item)) break;

                    player.GetComponent<PlayerInventory>().RemoveFromInventory(item);
                    GameObject.Find("GameManager").GetComponent<GameManager>().mainProgress += giveValue;
                }
            }


        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (DragDisabled() || dragged >= 0) return;


        lastParent = transform.parent;
        transform.SetParent(rectTransform.root, true);
        if (dragged < 0)
        {
            // dragging with left click
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (lastParent.gameObject.name.Contains("GridCell")) player.GetComponent<PlayerInventory>().RemoveFromInventoryOnly(item, true);
                else if (lastParent.gameObject.name.Contains("ChickenCell") || lastParent.gameObject.name.Contains("PigCell")) player.GetComponent<PlayerInventory>().AddAnimal(item, 0 - int.Parse(quantity.text));
                dragged = 0;
            }
            // dragging with right click
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (lastParent.gameObject.name.Contains("GridCell")) player.GetComponent<PlayerInventory>().RemoveFromInventoryOnly(item, false);
                else if (lastParent.gameObject.name.Contains("ChickenCell") || lastParent.gameObject.name.Contains("PigCell"))
                {
                    GameObject newIcon = Instantiate(player.GetComponent<PlayerInventory>().inventoryIcon, lastParent);
                    player.GetComponent<PlayerInventory>().StretchAndFill(newIcon.GetComponent<RectTransform>());
                    newIcon.GetComponent<InventoryIcon>().SetIcon(item);
                    newIcon.GetComponent<InventoryIcon>().UpdateQuantity((int) Mathf.Floor(int.Parse(quantity.text) / 2f));
                    player.GetComponent<PlayerInventory>().AddAnimal(item, 0 - (int) Mathf.Ceil(int.Parse(quantity.text) / 2f));
                    UpdateQuantity((int) Mathf.Ceil(int.Parse(quantity.text) / 2f));
                }
                dragged = 1;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragDisabled() || dragged < 0) return;


        Debug.Log("stopped");
        dragged = -1;
        // scan cell being dragged at
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (RaycastResult result in results)
        {

            if (result.gameObject != null && result.gameObject.GetComponent<RectTransform>() != null)
            {
                // if inventory cell, add to inventory
                if (result.gameObject.name.Contains("GridCell"))
                {

                    if (result.gameObject.transform.childCount > 0)
                    {
                        var otherElement = result.gameObject.transform.GetChild(0);
                        otherElement.SetParent(lastParent, true);
                        otherElement.GetComponent<RectTransform>().localPosition = Vector3.zero;
                    }

                    lastParent = result.gameObject.transform;

                }
                // if putting chicken in chicken coop, or pig in pig pen
                else if ((result.gameObject.name.Contains("ChickenCell") && item == "Chicken") || (result.gameObject.name.Contains("PigCell") && item == "Pig"))
                {
                    if (result.gameObject.transform.childCount > 0)
                    {
                        var otherElement = result.gameObject.transform.GetChild(0);
                        otherElement.SetParent(lastParent, true);
                        otherElement.GetComponent<RectTransform>().localPosition = Vector3.zero;
                        Destroy(otherElement.gameObject);
                    }
                    lastParent = result.gameObject.transform.parent.GetChild(0);
                }
            }
        }
        transform.SetParent(lastParent, true);
        rectTransform.localPosition = Vector3.zero;

        if (lastParent.gameObject.name.Contains("ChickenCell") && player.GetComponent<PlayerInventory>().chickenCoopInventory["Chicken"] > 0
            || lastParent.gameObject.name.Contains("PigCell") && player.GetComponent<PlayerInventory>().pigPenInventory > 0)
        {
            player.GetComponent<PlayerInventory>().AddAnimal(item, int.Parse(quantity.text));
            if (lastParent.gameObject.name.Contains("ChickenCell")) lastParent.GetChild(0).gameObject.GetComponent<InventoryIcon>().UpdateQuantity(player.GetComponent<PlayerInventory>().chickenCoopInventory[item]);
            else if (lastParent.gameObject.name.Contains("PigCell")) lastParent.GetChild(0).gameObject.GetComponent<InventoryIcon>().UpdateQuantity(player.GetComponent<PlayerInventory>().pigPenInventory);
        }
        else if (lastParent.gameObject.name.Contains("ChickenCell") || lastParent.gameObject.name.Contains("PigCell")) player.GetComponent<PlayerInventory>().AddAnimal(item, int.Parse(quantity.text));


        // if placed in inventory add back item to inventory dicts
        else if (lastParent.gameObject.name.Contains("GridCell"))
        {
            // if item still exists in inventory (right click dragging), merge items back to one slot
            if (player.GetComponent<PlayerInventory>().inventory.ContainsKey(item))
            {
                Debug.Log("Still exists");
                player.GetComponent<PlayerInventory>().AddToInventory(item, int.Parse(quantity.text));
                Destroy(this.gameObject);
                return;
            }
            else
            {
                player.GetComponent<PlayerInventory>().inventory.Add(item, int.Parse(quantity.text));
                player.GetComponent<PlayerInventory>().inventoryIndex.Add(item);
                player.GetComponent<PlayerInventory>().inventoryIcons.Add(item, this.gameObject);
            }
        }
        inventoryChangedEvent.TriggerEvent();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragDisabled() || dragged < 0) return;

        transform.position = eventData.position;
    }

    public void ChangeParentColor(float colorAlpha)
    {
        return;
        /*var oldColor = lastParent.GetComponent<Image>().color;
        oldColor.a = colorAlpha;
        lastParent.GetComponent<Image>().color = oldColor;*/
    }

    private bool DragDisabled()
    {
        if (player.GetComponent<PlayerInventory>().sellMode == true || player.GetComponent<PlayerInventory>().giveMode == true) return true;

        return false;
    }
}
