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
    private GameManager gameManager;
    private PlayerInventory playerInventory;
    private GameObject toolTip;

    private RectTransform rectTransform;
    public Transform lastParent;
    private Canvas canvas;

    private int dragged = -1;


    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        playerInventory = GameObject.Find("Player").GetComponent<PlayerInventory>();
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
            sellValue = 50;
            giveValue = 150;
            break;
            case "Chicken":
            GetComponent<Image>().sprite = imageicons[15];
            sellValue = 50;
            giveValue = 250;
            break;
            case "Pig":
            GetComponent<Image>().sprite = imageicons[16];
            sellValue = 125;
            giveValue = 350;
            break;
            case "Cow":
            GetComponent<Image>().sprite = imageicons[17];
            sellValue = 1000;
            giveValue = 700;
            break;

            case "Wheat Seeds":
            GetComponent<Image>().sprite = imageicons[18];
            sellValue = 0;
            giveValue = 1;
            break;
            case "Tomato Seeds":
            GetComponent<Image>().sprite = imageicons[19];
            sellValue = 2;
            giveValue = 1;
            break;
            case "Lentils Seeds":
            GetComponent<Image>().sprite = imageicons[20];
            sellValue = 5;
            giveValue = 1;
            break;
            case "Wheat":
            GetComponent<Image>().sprite = imageicons[21];
            sellValue = 3;
            giveValue = 50;
            break;
            default:
            GetComponent<Image>().sprite = imageicons[22];
            break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playerInventory.sellMode && sellValue > 0) toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item + "\n$" + sellValue;
        else if (playerInventory.giveMode && giveValue > 0) toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item + "\n+" + giveValue;
        else toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item;
        if (dragged < 0) toolTip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toolTip.SetActive(false);
    }

    private bool leftrightmulti = false;
    public void OnPointerUp(PointerEventData eventData)
    {
        // dragging with left, clicking with right to drop one || dragging with right, clicking with left to drop one
        if ((dragged == 0 && eventData.button == PointerEventData.InputButton.Right)
            || (dragged == 1 && eventData.button == PointerEventData.InputButton.Left))
        {
            leftrightmulti = true;
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
                        UpdateQuantity(int.Parse(quantity.text) - 1);
                        playerInventory.AddToInventory(item);
                    }
                    else if (result.gameObject.name.Contains("ChickenCell") && item == "Chicken")
                    {
                        UpdateQuantity(int.Parse(quantity.text) - 1);
                        gameManager.AddAnimal(item, 1);
                        result.gameObject.transform.GetChild(0).gameObject.GetComponent<InventoryIcon>().UpdateQuantity(gameManager.chickenCoopInventory[item]);
                    }
                    else if (result.gameObject.name.Contains("PigCell") && item == "Pig")
                    {
                        UpdateQuantity(int.Parse(quantity.text) - 1);
                        gameManager.AddAnimal(item, 1);
                        result.gameObject.transform.GetChild(0).gameObject.GetComponent<InventoryIcon>().UpdateQuantity(gameManager.pigPenInventory);
                    }
                    if (int.Parse(quantity.text) <= 0)
                    {
                        toolTip.SetActive(false);
                        Destroy(this.gameObject);
                    }
                }

            }
        }
        // clicking normally to select or sell item
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // if on sell mode, sell non-tool item
            if (playerInventory.sellMode && imageicons.IndexOf(GetComponent<Image>().sprite) > 11)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Debug.Log(item + " was sold for $" + (sellValue * int.Parse(quantity.text)));
                    playerInventory.money += (sellValue * int.Parse(quantity.text));
                    playerInventory.RemoveFromInventory(item, int.Parse(quantity.text));
                }
                else
                {
                    playerInventory.RemoveFromInventory(item);
                    Debug.Log(item + " was sold for $" + sellValue);
                    playerInventory.money += sellValue;
                }
            }
            // if on give mode, give away non-tool item
            else if (playerInventory.giveMode && imageicons.IndexOf(GetComponent<Image>().sprite) > 11)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Debug.Log(item + " was given away for " + (giveValue * int.Parse(quantity.text)) + " goodness points!");
                    gameManager.mainProgress += (giveValue * int.Parse(quantity.text));
                    playerInventory.RemoveFromInventory(item, int.Parse(quantity.text));
                }
                else
                {
                    Debug.Log(item + " was given away for " + giveValue + " goodness points!");
                    gameManager.mainProgress += giveValue;
                    playerInventory.RemoveFromInventory(item);
                }

            }
            // put selected item in hand
            else if (!playerInventory.sellMode && !playerInventory.giveMode)
            {
                playerInventory.ChangeHandItem(item);
                Debugger.Log("Value of dragged is= " + dragged, Debugger.PriorityLevel.MustShown);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (playerInventory.sellMode && imageicons.IndexOf(GetComponent<Image>().sprite) > 11)
            {
                var control = Input.GetKey(KeyCode.LeftShift);
                int repeat = control == true ? (int) Mathf.Min(25, int.Parse(quantity.text)) : 5;
                for (int i = 0; i < repeat; i++)
                {
                    if (!playerInventory.inventory.ContainsKey(item)) break;

                    playerInventory.RemoveFromInventory(item);
                    playerInventory.money += sellValue;
                }
            }
            else if (playerInventory.giveMode && imageicons.IndexOf(GetComponent<Image>().sprite) > 11)
            {
                var control = Input.GetKey(KeyCode.LeftShift);
                int repeat = control == true ? (int) Mathf.Min(25, int.Parse(quantity.text)) : 5;
                for (int i = 0; i < repeat; i++)
                {
                    if (!playerInventory.inventory.ContainsKey(item)) break;

                    playerInventory.RemoveFromInventory(item);
                    gameManager.mainProgress += giveValue;
                }
            }


        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (DragDisabled() || dragged >= 0) return;

        Debugger.Log("DragStarted", Debugger.PriorityLevel.Low);
        lastParent = transform.parent;
        transform.SetParent(rectTransform.root, true);
        if (dragged < 0)
        {
            // dragging with left click
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (lastParent.gameObject.name.Contains("GridCell")) playerInventory.RemoveFromInventoryOnly(item, true);
                else if (lastParent.gameObject.name.Contains("ChickenCell") || lastParent.gameObject.name.Contains("PigCell")) gameManager.AddAnimalNumb(item, 0 - int.Parse(quantity.text));
                dragged = 0;
            }
            // dragging with right click
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (lastParent.gameObject.name.Contains("GridCell")) playerInventory.RemoveFromInventoryOnly(item, false);
                else if (lastParent.gameObject.name.Contains("ChickenCell") || lastParent.gameObject.name.Contains("PigCell"))
                {
                    GameObject newIcon = Instantiate(playerInventory.inventoryIcon, lastParent);
                    playerInventory.StretchAndFill(newIcon.GetComponent<RectTransform>());
                    newIcon.GetComponent<InventoryIcon>().SetIcon(item);
                    newIcon.GetComponent<InventoryIcon>().UpdateQuantity((int) Mathf.Floor(int.Parse(quantity.text) / 2f));
                    if (int.Parse(newIcon.GetComponent<InventoryIcon>().quantity.text) <= 0) Destroy(newIcon);
                    gameManager.AddAnimalNumb(item, 0 - (int) Mathf.Ceil(int.Parse(quantity.text) / 2f));
                    UpdateQuantity((int) Mathf.Ceil(int.Parse(quantity.text) / 2f));
                }
                dragged = 1;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragDisabled() || dragged < 0) return;

        var dragFromAnimalShelter = (lastParent.name.Contains("PigCell") || lastParent.name.Contains("ChickenCell"));
        var rightDragged = (dragged == 1);
        var leftDragged = (dragged == 0);



        Debugger.Log("stopped", Debugger.PriorityLevel.LeastImportant);
        dragged = -1;
        // scan cell being dragged at
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        //This condition creates a bug and same as right draging to a full cell;
        var BUGCREATER = dragFromAnimalShelter == true && rightDragged;//&& results.Count == 0 It is not zero because raycast is getting the object itself


        foreach (RaycastResult result in results)
        {

            if (result.gameObject != null && result.gameObject.GetComponent<RectTransform>() != null)
            {
                // if inventory cell, add to inventory
                if (result.gameObject.name.Contains("GridCell"))
                {
                    if (result.gameObject.transform.childCount > 0)
                    {
                        if (dragFromAnimalShelter == true || rightDragged == true || leftrightmulti == true)
                        {
                            leftrightmulti = false;
                            //var otherInventoryIcon = result.gameObject.transform.GetChild(0).GetComponent<InventoryIcon>();
                            //if (otherInventoryIcon != null)
                            //{
                            //    if (otherInventoryIcon.item == item && dragFromAnimalShelter)
                            //    {

                            //        playerInventory.AddToInventory(item, int.Parse(quantity.text));
                            //        otherInventoryIcon.UpdateQuantity(int.Parse(quantity.text));
                            //        Destroy(this.gameObject);
                            //        return;

                            //    }
                            //    else
                            //    {
                            //        break;
                            //    }
                            //}

                            break;


                        }
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

                        if (rightDragged == true)
                        {
                            //Right dragging to itself back in Animal Pens. I have no idea how EGGS will react to these changes.
                            var myOtherHalf = lastParent.GetChild(0).GetComponent<InventoryIcon>();
                            var total = int.Parse(myOtherHalf.quantity.text) + int.Parse(quantity.text);
                            UpdateQuantity(total);
                            BUGCREATER = false;
                        }

                        var otherElement = result.gameObject.transform.GetChild(0);
                        otherElement.SetParent(lastParent, true);
                        otherElement.GetComponent<RectTransform>().localPosition = Vector3.zero;
                        toolTip.SetActive(false);
                        Destroy(otherElement.gameObject);
                    }
                    lastParent = result.gameObject.transform.parent.GetChild(0);



                }
            }
        }


        if (BUGCREATER == true && lastParent.childCount > 0)
        {
            var myOtherHalf = lastParent.GetChild(0).GetComponent<InventoryIcon>();
            var total = int.Parse(myOtherHalf.quantity.text) + int.Parse(quantity.text);
            myOtherHalf.UpdateQuantity(total);

            //TODO need  1 more step with pigPenInventory? (testing with pigs generally)

            Debugger.Log("BUGCREATER Check total is  " + total + " but gamemanager value for PIGS is " + gameManager.pigPenInventory, Debugger.PriorityLevel.MustShown);

            Destroy(gameObject);
            return;
        }

        transform.SetParent(lastParent, true);
        rectTransform.localPosition = Vector3.zero;

        Debug.Log("new parent: " + lastParent.gameObject.name);
        if (lastParent.gameObject.name.Contains("ChickenCell") || lastParent.gameObject.name.Contains("PigCell"))
        {
            gameManager.AddAnimal(item, int.Parse(quantity.text));
        }
        // if placed in inventory add back item to inventory dicts
        else if (lastParent.gameObject.name.Contains("GridCell"))
        {
            // if item still exists in inventory (right click dragging), merge items back to one slot
            if (playerInventory.inventory.ContainsKey(item))
            {
                Debugger.Log("Still exists", Debugger.PriorityLevel.Medium);
                playerInventory.AddToInventory(item, int.Parse(quantity.text));
                toolTip.SetActive(false);
                Destroy(this.gameObject);
                return;
            }
            else
            {
                playerInventory.inventory.Add(item, int.Parse(quantity.text));
                playerInventory.inventoryIndex.Add(item);
                playerInventory.inventoryIcons.Add(item, this.gameObject);
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
        if (playerInventory.sellMode == true || playerInventory.giveMode == true) return true;

        return false;
    }

}
