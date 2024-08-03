using TMPro;
using UnityEngine;

public class ChangeToolTipText : MonoBehaviour
{
    [SerializeField]
    private PlayerInventory inventory;
    [SerializeField]
    private TextMeshProUGUI toolTipTextField;

    [Multiline]
    [SerializeField]
    private string inventorytext;
    [Multiline]
    [SerializeField]
    private string sellModeText;
    [Multiline]
    [SerializeField]
    private string giveModeText;


    public void changeToolTipText()
    {
        if (inventory.sellMode)
        {
            toolTipTextField.text = sellModeText;
        }
        else if (inventory.giveMode)
        {
            toolTipTextField.text = giveModeText;
        }
        else
        {
            toolTipTextField.text = inventorytext;
        }
    }

}
