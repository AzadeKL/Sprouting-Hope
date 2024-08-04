using TMPro;
using UnityEngine;

public class ChangeToolTipText : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private TextMeshProUGUI toolTipTextField;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private GameObject chickenCoopUI;
    [SerializeField] private GameObject pigPenUI;

    [Multiline] [SerializeField] private string inventorytext;
    [Multiline] [SerializeField] private string sellModeText;
    [Multiline] [SerializeField] private string giveModeText;
    [Multiline] [SerializeField] private string storeText;
    [Multiline] [SerializeField] private string pigPenText;
    [Multiline] [SerializeField] private string ChickenCoopText;


    public void changeToolTipText()
    {
        if (inventory.sellMode) toolTipTextField.text = sellModeText;
        else if (inventory.giveMode) toolTipTextField.text = giveModeText;
        else if (upgradeUI.activeSelf) toolTipTextField.text = storeText;
        else if (pigPenUI.activeSelf) toolTipTextField.text = pigPenText;
        else if (chickenCoopUI.activeSelf) toolTipTextField.text = ChickenCoopText;
        else toolTipTextField.text = inventorytext;
    }

}
