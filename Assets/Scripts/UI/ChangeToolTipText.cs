using TMPro;
using UnityEngine;

public class ChangeToolTipText : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private TextMeshProUGUI toolTipTextField;
    [SerializeField] private GameObject upgradeUI;

    [Multiline] [SerializeField] private string inventorytext;
    [Multiline] [SerializeField] private string sellModeText;
    [Multiline] [SerializeField] private string giveModeText;
    [Multiline] [SerializeField] private string storeText;


    public void changeToolTipText()
    {
        if (inventory.sellMode) toolTipTextField.text = sellModeText;
        else if (inventory.giveMode) toolTipTextField.text = giveModeText;
        else if (upgradeUI.activeSelf) toolTipTextField.text = storeText;
        else toolTipTextField.text = inventorytext;
    }

}
