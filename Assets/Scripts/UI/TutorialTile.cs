using UnityEngine;
using UnityEngine.Events;


public class TutorialTile : MonoBehaviour
{
    [Header("Tutorial Text")]
    [Multiline,Tooltip("Enter what tutorial should say")]public string tutorialText;

    public UnityEvent<string, Collider2D, bool> tutorialEvent;
    public UnityEvent tutorialEventClose;

    // public bool isTutorialActive = true;
    public bool shouldBeDestroyed = false;
    public bool isFirstTime = true;

    // Start is called before the first frame update
    void Start()
    {
        tutorialEvent.AddListener(GameObject.FindGameObjectWithTag("TutorialPopUp").GetComponent<TutorialPopUp>().showPopUp);
        tutorialEventClose.AddListener(GameObject.FindGameObjectWithTag("TutorialPopUp").GetComponent<TutorialPopUp>().hidePopUp);

        string loadedTutorials = PlayerPrefs.GetString("LoadedTutorials", "");
        if (loadedTutorials.Contains(gameObject.name))
        {
            isFirstTime = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            tutorialEvent.Invoke(tutorialText, GetComponent<Collider2D>(), isFirstTime);
            isFirstTime = false;
            string loadedTutorials = PlayerPrefs.GetString("LoadedTutorials", "");
            if (!loadedTutorials.Contains(gameObject.name))
            {
                PlayerPrefs.SetString("LoadedTutorials", loadedTutorials + gameObject.name + ",");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            tutorialEventClose.Invoke();
            if (shouldBeDestroyed)
            {
                Destroy(gameObject);
            } 
        }
    }
}
