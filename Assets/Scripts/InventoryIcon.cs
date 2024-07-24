using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryIcon : MonoBehaviour
{
    [SerializeField] private List<Sprite> imageicons;
    public TextMeshProUGUI quantity;

    public string item;
    private GameObject player;

    private void Awake()
    {
        player = GameObject.Find("Player");
    }

    public void UpdateQuantity(int amount)
    {
        quantity.text = amount.ToString();
    }

    public void SetIcon(string tag)
    {
        item = tag;
        switch(tag)
        {
            case "Wheat Seeds":
                GetComponent<Image>().sprite = imageicons[0];
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
}
