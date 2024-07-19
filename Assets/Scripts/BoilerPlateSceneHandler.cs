using UnityEngine;
using UnityEngine.SceneManagement;

public class BoilerPlateSceneHandler : MonoBehaviour
{

    void Start()
    {
#if !UNITY_EDITOR
        LoadMainMenu();
#endif
    }



    [ContextMenu("LoadMainMenu")]
    private void LoadMainMenu()
    {
        SceneManager.LoadScene(1);
    }


}
