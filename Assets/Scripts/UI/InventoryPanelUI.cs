using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelUI : MonoBehaviour
{

    private List<Transform> tabPanels;

    private void Awake()
    {


    }

    private void Start()
    {
        tabPanels = new();
        //var panels = transform.GetChild(1).GetComponentsInChildren<Transform>();
        foreach (Transform tr in transform.GetChild(1))
        {
            tabPanels.Add(tr);
        }
    }

    private void OnEnable()
    {
        var buttons = transform.GetChild(0).GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => ButtonClicks(button));
        }

    }

    private void OnDisable()
    {
        var buttons = transform.GetChild(0).gameObject.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }


    private void ButtonClicks(Selectable selectable)
    {
        var index = selectable.transform.GetSiblingIndex();
        foreach (var tab in tabPanels)
        {
            tab.gameObject.SetActive(false);
        }
        tabPanels[index].gameObject.SetActive(true);
    }

}
