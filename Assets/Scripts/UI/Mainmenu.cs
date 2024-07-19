using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Mainmenu : MonoBehaviour
{
    [SerializeField]
    private AudioClip selectClip;


    [SerializeField]
    private AudioClip backClip;

    private AudioSource audioSource;

    [SerializeField]
    private SceneField optionsScene;

    [SerializeField]
    private Button conButton;

    [SerializeField]
    private EventSystem eventSystem;

    private GameObject lastSelectable;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        //conButton.interactable = !DataManager.instance.isNewGame();//NOT
        if (conButton.IsInteractable())
        {
            eventSystem.firstSelectedGameObject = conButton.gameObject;

        }
        lastSelectable = eventSystem.firstSelectedGameObject;
    }

    private void Update()
    {

        if (eventSystem.currentSelectedGameObject == null) return;

        //TODO this feels so bad. 
        //Suprise Suprise It was bad.
        //if (eventSystem.currentSelectedGameObject != lastSelectable)
        //{

        //    audioSource.PlayOneShot(upClip);
        //    var x = lastSelectable.FindSelectableOnUp();
        //    var y = lastSelectable.FindSelectableOnDown();
        //    var t = lastSelectable.FindSelectable(Vector3.left);
        //    if (lastSelectable.FindSelectableOnUp() == eventSystem.currentSelectedGameObject)
        //    {
        //        audioSource.PlayOneShot(upClip);
        //    }
        //    else if (lastSelectable.FindSelectableOnDown() == eventSystem.currentSelectedGameObject)
        //    {
        //        audioSource.PlayOneShot(downClip);
        //    }
        //    lastSelectable = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
        //}

        //if (eventSystem.currentSelectedGameObject != lastSelectable)
        //{
        //    audioSource.PlayOneShot(upClip);
        //    lastSelectable = eventSystem.currentSelectedGameObject;
        //}


    }

    public void Continue()
    {
        //var sceneTOLoad = DataManager.instance.GetLastScene();
        // SceneManager.LoadScene(sceneTOLoad);
    }
    // This method is called when the Play button is pressed
    public void PlayGame()
    {
        // DataManager.instance.ResetGameState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // This method is called when the Options button is pressed
    public void OpenOptions()
    {
        // Load the options scene (assuming options are at build index 2)
        SceneManager.LoadScene(optionsScene);
        Debug.Log("Options");
    }

    // This method is called when the Quit button is pressed
    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }


}
