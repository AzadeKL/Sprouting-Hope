using UnityEngine;
using UnityEngine.Events;


public class TutorialTile : MonoBehaviour
{
    [Header("Tutorial Text")]
    [Multiline,Tooltip("Enter what tutorial should say")]public string tutorialText;

    public UnityEvent<string, Collider2D> tutorialEvent;
    public UnityEvent tutorialEventClose;

    public bool isTutorialActive = true;

    // Start is called before the first frame update
    void Start()
    {
        tutorialEvent.AddListener(GameObject.FindGameObjectWithTag("TutorialPopUp").GetComponent<TutorialPopUp>().showPopUp);
        tutorialEventClose.AddListener(GameObject.FindGameObjectWithTag("TutorialPopUp").GetComponent<TutorialPopUp>().hidePopUp);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            tutorialEvent.Invoke(tutorialText, GetComponent<Collider2D>());  
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            tutorialEventClose.Invoke();  
        }
    }
}