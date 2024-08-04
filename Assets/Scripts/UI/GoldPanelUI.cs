using UnityEngine;

public class GoldPanelUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI textMeshProUGUI;

    [SerializeField]
    private PlayerInventory player;


    // Update is called once per frame
    void Update()
    {
        textMeshProUGUI.text = "" + player.money.ToString();
    }
}
