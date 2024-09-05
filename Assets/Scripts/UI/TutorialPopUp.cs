using System.Collections.Generic;
using SaveSystem;
using TMPro;
using Unity.VisualScripting;
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
    private Transform player;

    [Header("Tutorial PopUp Settings")]
    public Vector2 popUpPadding = new Vector2(40, 20);
    public float maxWidth = 500;

    private bool areTutorialActive = true;

    private Collider2D lastActiveCollider;

    // Start is called before the first frame update
    void Start()
    {
        setVisibility(false);
        textObject.SetText("");
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            areTutorialActive = !areTutorialActive;
            if (areTutorialActive)
            {
                if (player.gameObject.GetComponent<Collider2D>().IsTouching(lastActiveCollider))
                {
                    showPopUp(tutorialText, lastActiveCollider);
                }
            }
            else
            {
                hidePopUp();
            }
        }
    }

    //Setting text for the tutorial and
    //splitting the text into words , (useful for setting the text in multiple lines)
    public void setText(string text)
    {
        tutorialText = text;
        if(tutorialText != "")
        {
            tutorialTextList = tutorialText.Split(' ');
        }
        
    }

    //Setting the visibility of the tutorial pop up
    private void setVisibility(bool visibility)
    {
        tutorialPopUp.SetActive(visibility);
        // tutorialTextObject.gameObject.SetActive(visibility);
    }

    //Setting the dimensions of the text
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

    //Setting the dimensions of the box as well as replacing the text at correct position
    private void setBox(Vector2 size)
    {
        tutorialPopUp.GetComponent<RectTransform>().sizeDelta = size + popUpPadding;
        Vector2 tutorialPosition = new Vector2(-size.x / 2, -size.y / 2);
        textObject.rectTransform.localPosition = tutorialPosition * new Vector2(2, 1);
        tutorialPopUp.GetComponent<RectTransform>().localPosition = tutorialPosition;
    }

    //Showing the pop up
    //Called by TutorialTile.cs via Event
    public void showPopUp(string text, Collider2D collider)
    {
        lastActiveCollider = collider;
        setText(text);
        if(!areTutorialActive)
        {
            return;
        }
        setDimensionsforText();
        setVisibility(true);
    }

    //Hiding the pop up
    //Called by TutorialTile.cs via Event
    public void hidePopUp()
    {
        textObject.SetText("");
        textObject.ForceMeshUpdate();
        // setDimensionsforText();
        setVisibility(false);
    }

}
