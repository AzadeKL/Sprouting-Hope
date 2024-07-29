using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;


public class UpgradeIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public string item;
    [SerializeField] bool tool;
    public int cost;
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
        transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "$" + cost.ToString();
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
        if (player.GetComponent<PlayerInventory>().money >= cost)
        {
            player.GetComponent<PlayerInventory>().money -= cost;
            player.GetComponent<PlayerInventory>().AddToInventory(item);
            // swap old item with better item?
            if (tool) 
            {
                Destroy(this.gameObject);
                toolTip.SetActive(false);
            }
        }
        else
        {
            // provide some feedback depicting that player cannot afford it
        }
    }

    public void ChangeParentColor(float colorAlpha)
    {
        return;
        var oldColor = lastParent.GetComponent<Image>().color;
        oldColor.a = colorAlpha;
        lastParent.GetComponent<Image>().color = oldColor;
    }
}