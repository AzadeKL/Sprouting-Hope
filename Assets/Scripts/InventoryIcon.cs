using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;


public class InventoryIcon : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private List<Sprite> imageicons;
    public TextMeshProUGUI quantity;

    public string item;
    private GameObject player;
    private GameObject toolTip;

    private RectTransform rectTransform;
    public Transform lastParent;
    private Canvas canvas;


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
        if (amount == 1)
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
                break;
            case "Tomato Seeds":
                GetComponent<Image>().sprite = imageicons[9];
                break;
            case "Lentils Seeds":
                GetComponent<Image>().sprite = imageicons[10];
                break;
            case "Wheat":
                GetComponent<Image>().sprite = imageicons[11];
                break;
            case "Tomato":
                GetComponent<Image>().sprite = imageicons[12];
                break;
            case "Lentil":
                GetComponent<Image>().sprite = imageicons[13];
                break;
            case "Egg":
                GetComponent<Image>().sprite = imageicons[14];
                break;
            case "Chicken":
                GetComponent<Image>().sprite = imageicons[15];
                break;
            case "Pig":
                GetComponent<Image>().sprite = imageicons[16];
                break;
            case "Cow":
                GetComponent<Image>().sprite = imageicons[17];
                break;
            default:
                GetComponent<Image>().sprite = imageicons[0];
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("hovering " + item);
        toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item;
        toolTip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("no longer hovering");
        toolTip.SetActive(false);
    }

    public void Clicked()
    {
        Debug.Log("clicked " + item);
        // put selected item in hand
        if (!player.GetComponent<PlayerInventory>().sellMode)
        {
            player.GetComponent<PlayerInventory>().handIndex = player.GetComponent<PlayerInventory>().inventoryIndex.IndexOf(item);
            player.GetComponent<PlayerInventory>().ChangeHand(item);
        }
        // if on sell mode, sell non-tool item
        else if (imageicons.IndexOf(GetComponent<Image>().sprite) > 1)
        {
            player.GetComponent<PlayerInventory>().RemoveFromInventory(item);
            Debug.Log(item + " was sold for $$$");
            // item sold
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastParent = transform.parent;
        transform.SetParent(rectTransform.root, true);
    }

    public void OnEndDrag(PointerEventData eventData)
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
