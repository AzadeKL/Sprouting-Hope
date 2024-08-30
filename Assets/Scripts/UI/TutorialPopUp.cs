using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialPopUp : MonoBehaviour
{

    [Header("Tutorial Text")]
    [SerializeField, Tooltip("Do not set in Inspector, only to show what will be displayed")] 
    private string tutorialText;
    private bool isVisiable = false;

    [Header("Tutorial PopUp References")]
    public GameObject tutorialPopUp;
    public Image backgroundImage;
    public TextMeshProUGUI tutorialTextObject;

    [Header("Tutorial PopUp Settings")]
    public float popUpTime = 5f;

    //Locking Mechanism
    private bool popUpLock = true;

    // Start is called before the first frame update
    void Start()
    {
        tutorialPopUp.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setText(string text)
    {
        tutorialText = text;
    }

    private void setVisibility(bool visibility)
    {
        tutorialPopUp.SetActive(visibility);
    }

    public bool P()
    {
        if (popUpLock)
        {
            popUpLock = false;
            return true;
        }
        else return false;
    }

    public void V()
    {
        popUpLock = true;
    }

    private void setDimensionsforText()
    {
        tutorialTextObject.text = tutorialText;
        tutorialTextObject.ForceMeshUpdate();
        Vector2 textSize = tutorialTextObject.GetRenderedValues(false);
        RectTransform rt = tutorialPopUp.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(textSize.x + 20, textSize.y + 20);
    }

    public void showPopUp(string text)
    {
        // if(!P()) return;
        setText(text);
        setDimensionsforText();
        setVisibility(true);
    }

    public void hidePopUp()
    {
        tutorialPopUp.SetActive(false);
        // V();
    }


}
