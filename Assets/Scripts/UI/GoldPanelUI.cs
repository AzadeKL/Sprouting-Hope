using UnityEngine;

public class GoldPanelUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI textMeshProUGUI;

    [SerializeField]
    private FloatReference money;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        textMeshProUGUI.text = "Gold: " + money.Value.ToString("F0");
    }
}
