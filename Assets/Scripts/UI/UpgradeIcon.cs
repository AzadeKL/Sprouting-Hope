using SaveSystem;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;


public class UpgradeIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDestroyable
{

    public string item;
    [Multiline]
    [SerializeField] private string upgradeEffect;
    [SerializeField] private string upgradeRequirement;
    [SerializeField] bool tool;
    public int cost;
    private GameObject player;
    private GameObject toolTip;

    [SerializeField] private List<GameObject> prevUpgrades;
    private RectTransform rectTransform;
    public Transform lastParent;
    private Canvas canvas;

    //Image Manipulation
    private Image imageIcon;
    private Button button;
    private Color originalColor;
    public Color disabledColor;
    private bool isDisabled = false;

    public string GenerateDestroyedId()
    {
        return SaveSystem.IDestroyable.GetGameObjectPathWId(gameObject, item);
    }

    private void Awake()
    {
        player = GameObject.Find("Player");
        rectTransform = transform.GetComponent<RectTransform>();
        canvas = rectTransform.root.GetComponent<Canvas>();
        transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "$" + cost.ToString();

        imageIcon = GetComponent<Image>();
        originalColor = imageIcon.color;
        disabledColor = Color.black;
        button = GetComponent<Button>();

        if (SaveSystem.DataManager.instance.IsDestroyedDestroyable(this))
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        toolTip = FindObjectOfType<Tooltip>(true).gameObject;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("hovering " + item);
        if(!isDisabled)
        {
            if (upgradeEffect != "")
            {
                toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item + "\n" + upgradeEffect;
            }
            else
            {
                toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item;
            }

        }
        else
        {
            toolTip.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item + "\n" + "You need " + upgradeRequirement + " goodness to unlock this upgrade";
        }
        toolTip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("no longer hovering");
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
                foreach (GameObject oldItem in prevUpgrades)
                {
                    if (oldItem != null)
                    {
                        var upgradeIcon = oldItem.GetComponent<UpgradeIcon>();
                        if (!SaveSystem.DataManager.instance.IsDestroyedDestroyable(upgradeIcon))
                        {
                            SaveSystem.DataManager.instance.AddDestroyedDestroyable(upgradeIcon);
                        }
                        toolTip.SetActive(false);
                        Destroy(oldItem);
                    }
                }
                Debug.Log("Destroying item: " + this.gameObject.name);
                SaveSystem.DataManager.instance.AddDestroyedDestroyable(this);
                toolTip.SetActive(false);
                Destroy(this.gameObject);
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
        /*var oldColor = lastParent.GetComponent<Image>().color;
        oldColor.a = colorAlpha;
        lastParent.GetComponent<Image>().color = oldColor;*/
    }

    public void EnableIcon()
    {
        isDisabled = false;
        // imageIcon.color = originalColor;
    }

    public void DisableIcon()
    {
        isDisabled = true;
    }

    void OnEnable()
    {
        if (isDisabled)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }

    void OnBecameVisible()
    {
        if (isDisabled)
        {
            // imageIcon.color = disabledColor;
            button.interactable = false;
        }
        else
        {
            // imageIcon.color = originalColor;
            button.interactable = true;
        }
    }

    public void setUpgradeRequirement(int requirement)
    {
        upgradeRequirement = requirement.ToString();
    }
}
