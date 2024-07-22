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

    public void UpdateQuantity(int amount)
    {
        quantity.text = amount.ToString();
    }

    public void SetIcon(string tag)
    {
        switch(tag)
            {
                case "Wheat Seeds":
                    item = tag;
                    GetComponent<Image>().sprite = imageicons[0];
                    break;
            }
    }
}
