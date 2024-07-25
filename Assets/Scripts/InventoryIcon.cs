using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;


public class InventoryIcon : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private List<Sprite> imageicons;
    public TextMeshProUGUI quantity;

    public string item;
    private GameObject player;

    private RectTransform rectTransform;
    public Transform lastParent;
    private Canvas canvas;


    private void Awake()
    {
        player = GameObject.Find("Player");
        rectTransform = transform.GetComponent<RectTransform>();
        canvas = rectTransform.root.GetComponent<Canvas>();
    }

    public void UpdateQuantity(int amount)
    {
        quantity.text = amount.ToString();
    }

    public void SetIcon(string tag)
    {
        item = tag;
        switch (tag)
        {
            case "Wheat Seeds":
            GetComponent<Image>().sprite = imageicons[0];
            break;
            case "Hoe":
            GetComponent<Image>().sprite = imageicons[1];
            break;
            case "Watering Can":
            GetComponent<Image>().sprite = imageicons[2];
            break;
            default:
            GetComponent<Image>().sprite = imageicons[0];
            break;
        }
    }

    public void Clicked()
    {
        Debug.Log("clicked " + item);
        player.GetComponent<PlayerInventory>().handIndex = player.GetComponent<PlayerInventory>().inventoryIndex.IndexOf(item);
        player.GetComponent<PlayerInventory>().ChangeHand(item);
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
