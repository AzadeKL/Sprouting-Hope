using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private List<SceneField> notAllowedScenes;


    [SerializeField]
    private AudioClip select_clip;

    [SerializeField]
    private AudioClip back_clip;

    //[SerializeField]
    //private GameEvent sceneIsChaning;

    [SerializeField]
    private GameObject pauseMenuUI;

    //[SerializeField]
    //private GameObject InGameDebugObject;

    private bool isPaused = false;

    //public delegate void TogglePause();
    //public static TogglePause togglePause;
    private static PauseMenu instance;

    private bool notAllowed = true;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        pauseMenuUI.SetActive(false);



    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // InGameDebugObject.SetActive(false);
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        notAllowed = false;

        foreach (var sceneField in notAllowedScenes)
        {
            notAllowed = sceneField.SceneName.Equals(scene.name);
            if (notAllowed == true) break;
        }

        if (notAllowed == true)
        {
            Resume();
        }
    }

    //private void OnEnable()
    //{
    //    togglePause += HandlePause;
    //}
    //private void OnDisable()
    //{
    //    togglePause -= HandlePause;
    //}

    private void Update()
    {
        if (notAllowed == true) return;


        if (Input.GetKeyUp(KeyCode.Escape))
        {
            HandlePause();
        }
    }
    public void HandlePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);

        Time.timeScale = 0f; // This will pause the game
        isPaused = true;
    }


    public void LoadLevelWithSceneField(SceneField level)
    {
        Resume();
        // sceneIsChaning.TriggerEvent();
        SceneManager.LoadScene(level);
    }
    public void LoadLevel(int level)
    {
        Resume();
        // sceneIsChaning.TriggerEvent();
        SceneManager.LoadScene(level);
    }

    public void LoadMenu()
    {

        Resume();
        //sceneIsChaning.TriggerEvent();
        SceneManager.LoadScene(0);

    }
    public void QuitGame()
    {
        // You can add more logic here like confirmation dialog if needed
        Application.Quit();
    }



}