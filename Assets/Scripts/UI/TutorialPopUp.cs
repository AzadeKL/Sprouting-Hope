using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

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
    public Vector2 popUpPadding = new Vector2(40, 20);

    // Start is called before the first frame update
    void Start()
    {
        setVisibility(false);
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
        // tutorialTextObject.gameObject.SetActive(visibility);
    }

    private void setDimensionsforText()
    {
        tutorialTextObject.SetText(tutorialText);
        tutorialTextObject.ForceMeshUpdate();
        Vector2 textSize = tutorialTextObject.GetRenderedValues(false);
        
        setBox(textSize);
    }

    private void setBox(Vector2 size)
    {
        tutorialPopUp.GetComponent<RectTransform>().sizeDelta = size + popUpPadding;
        Vector2 tutorialPosition = new Vector2(-(size.x / 2), size.y / 2);
        tutorialTextObject.rectTransform.localPosition = tutorialPosition;
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
        tutorialTextObject.SetText("");
        tutorialTextObject.ForceMeshUpdate();
        // setDimensionsforText();
        setVisibility(false);
    }


}
