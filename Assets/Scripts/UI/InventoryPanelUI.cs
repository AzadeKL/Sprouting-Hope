using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelUI : MonoBehaviour
{

    private List<Transform> tabPanels;
    private List<RectTransform> buttonsRectTr;

    private void Awake()
    {
        buttonsRectTr = new();

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
        foreach (Transform tr in transform.GetChild(0))
        {
            buttonsRectTr.Add(tr.GetComponentInChildren<Button>().GetComponent<RectTransform>());
            tr.GetComponentInChildren<Button>().onClick.AddListener(() => ButtonClicks(tr.GetComponentInChildren<Button>()));
        }
    }

    private void OnDisable()
    {
        var buttons = transform.GetChild(0).GetChild(0).gameObject.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }


    private void ButtonClicks(Selectable selectable)
    {
        var index = selectable.transform.parent.GetSiblingIndex();
        foreach (var tab in tabPanels)
        {
            tab.gameObject.SetActive(false);
            // tab.GetComponent<RectTransform>().DOAnchorPosY
        }
        foreach (var rectTransform in buttonsRectTr)
        {
            rectTransform.DOAnchorPosY(-25, 1f).SetEase(Ease.OutBounce);
        }
        tabPanels[index].gameObject.SetActive(true);
        buttonsRectTr[index].DOAnchorPosY(0, 1f).SetEase(Ease.OutCubic);
    }

}
