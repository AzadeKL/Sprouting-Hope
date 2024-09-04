using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopUp : MonoBehaviour
{

    [Header("Tutorial Text")]
    [SerializeField, Tooltip("Do not set in Inspector, only to show what will be displayed")] 
    private string tutorialText;
    private string[] tutorialTextList;
    private bool isVisiable = false;

    [Header("Tutorial PopUp References")]
    public GameObject tutorialPopUp;
    public Image backgroundImage;
    public TextMeshProUGUI textObject;

    [Header("Tutorial PopUp Settings")]
    public Vector2 popUpPadding = new Vector2(40, 20);
    public float maxWidth = 500;

    // Start is called before the first frame update
    void Start()
    {
        setVisibility(false);
        textObject.SetText("");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setText(string text)
    {
        tutorialText = text;
        tutorialTextList = tutorialText.Split(' ');
        Debug.Log("Tutorial Text list: " + tutorialTextList.Length);
    }

    private void setVisibility(bool visibility)
    {
        tutorialPopUp.SetActive(visibility);
        // tutorialTextObject.gameObject.SetActive(visibility);
    }

    private void setDimensionsforText()
    {
        Vector2 textSize = textObject.GetRenderedValues(false);
        while(true)
        {
            tutorialText = tutorialText.Trim();
            textObject.SetText(tutorialText);
            textObject.ForceMeshUpdate();
            textSize = textObject.GetRenderedValues(false);
            if(textSize.x < maxWidth)
            {
                break;
            }
            else
            {
                List<string> tutorialTextListTemp = new List<string>();
                string tempString = "";
                for (int i = 0; i < tutorialTextList.Length; i++)
                {
                    if (textObject.GetPreferredValues(tempString + " " + tutorialTextList[i]).x < maxWidth)
                    {
                        tempString += " " + tutorialTextList[i];
                    }
                    else
                    {
                        tutorialTextListTemp.Add(tempString);
                        tempString = tutorialTextList[i];
                    }
                }
                tutorialTextListTemp.Add(tempString);
                tutorialText = string.Join("\n", tutorialTextListTemp);
            }
        }
        
        
        setBox(textSize);
    }

    private void setBox(Vector2 size)
    {
        tutorialPopUp.GetComponent<RectTransform>().sizeDelta = size + popUpPadding;
        Vector2 tutorialPosition = new Vector2(-size.x / 2, -size.y / 2);
        textObject.rectTransform.localPosition = tutorialPosition * new Vector2(2, 1);
        tutorialPopUp.GetComponent<RectTransform>().localPosition = tutorialPosition;
    }

    public void showPopUp(string text)
    {
        setText(text);
        setDimensionsforText();
        setVisibility(true);
    }

    public void hidePopUp()
    {
        textObject.SetText("");
        textObject.ForceMeshUpdate();
        // setDimensionsforText();
        setVisibility(false);
    }


}
