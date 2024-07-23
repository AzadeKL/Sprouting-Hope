using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserInterfaceGridLayout;

public class InventoryPanelUI : MonoBehaviour
{

    private List<Transform> tabPanels;
    private List<RectTransform> buttonsRectTr;

    private void Awake()
    {
        buttonsRectTr = new();

        tabPanels = new();
        //var panels = transform.GetChild(1).GetComponentsInChildren<Transform>();
        foreach (Transform tr in transform.GetChild(1))
        {
            tabPanels.Add(tr);
        }
        foreach (Transform tr in transform.GetChild(0))
        {
            buttonsRectTr.Add(tr.GetComponentInChildren<Button>().GetComponent<RectTransform>());
        }
    }

    private void Start()
    {

    }

    private void OnEnable()
    {
        foreach (Transform tr in transform.GetChild(0))
        {
            tr.GetComponentInChildren<Button>().onClick.AddListener(() => ButtonClicks(tr.GetComponentInChildren<Button>()));
        }
        ResetVisuals();
    }

    private void OnDisable()
    {
        foreach (Transform tr in transform.GetChild(0))
        {
            tr.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        }
    }


    private void ButtonClicks(Selectable selectable)
    {
        var index = selectable.transform.parent.GetSiblingIndex();
        foreach (var tab in tabPanels)
        {
            tab.gameObject.SetActive(false);
        }
        foreach (var rectTransform in buttonsRectTr)
        {
            rectTransform.DOAnchorPosY(-25, 1f).SetEase(Ease.OutBounce);
        }
        tabPanels[index].gameObject.SetActive(true);
        buttonsRectTr[index].DOAnchorPosY(0, 1f).SetEase(Ease.OutCubic);
        InsideTabAnimations(tabPanels[index].GetComponent<FlexibleGridLayout>());
    }


    private void InsideTabAnimations(FlexibleGridLayout flexibleGridLayout)
    {
        StartCoroutine(InsideTabAnimationsCorutine(flexibleGridLayout));
    }

    IEnumerator InsideTabAnimationsCorutine(FlexibleGridLayout flexibleGridLayout)
    {
        float timeLimit = 0.3f;
        var startVec = Vector2.one * 40;
        var endVec = Vector2.one * 10;

        for (var timePassed = 0f; timePassed < timeLimit; timePassed += Time.deltaTime)
        {
            var factor = timePassed / timeLimit;
            var result = Vector2.Lerp(startVec, endVec, factor);

            flexibleGridLayout.spacing = result;
            flexibleGridLayout.SetComponentDirty();
            yield return null;
        }

        flexibleGridLayout.spacing = endVec;
        flexibleGridLayout.SetComponentDirty();
    }

    private void ResetVisuals()
    {
        foreach (var tabPanel in tabPanels)
        {
            tabPanel.GetComponent<FlexibleGridLayout>().spacing = Vector2.one * 10;
        }
        // buttonsRectTr[0].GetComponent<Button>().onClick.Invoke();
    }

}
