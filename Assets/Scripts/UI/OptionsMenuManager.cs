using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenuManager : MonoBehaviour
{


    [SerializeField] private AudioClip ui_Back_Clip;

    [SerializeField] private TMPro.TMP_Dropdown _qualityDropdown;
    [SerializeField] private TMPro.TMP_Dropdown _fspCapDropdown;
    [SerializeField] private TMPro.TMP_Dropdown _resolutionDropdown;



    private int[] fpsArray = { -1, 30, 60, 75, 144 };

    //private int[] resolutionW = { 800, 1920, 60, 75, 144 };
    //private int[] resolutionH = { 600, 1080, 60, 75, 144 };

    private string seperator = "x";

    private Resolution[] resArray;

    private GameObject graphyFPS;
    private void Start()
    {


        //_qualityDropdown.value = QualitySettings.GetQualityLevel();
        //_fspCapDropdown.value = 0;

        //var graphy = GameObject.Find("[Graphy]");
        //if (graphy != null)
        //{
        //    graphyFPS = graphy.transform.GetChild(0).gameObject;
        //}


        //for (int i = 0; i < fpsArray.Length; i++)
        //{
        //    if (fpsArray[i] == Application.targetFrameRate)
        //    {
        //        _fspCapDropdown.value = i;
        //    }

        //}

        resArray = Screen.resolutions;
        List<string> resolutionList = new List<string>();
        foreach (var resolution in resArray)
        {
            string itemName = resolution.width + seperator + resolution.height;
            if (resolutionList.Contains(itemName) == false)
            {
                resolutionList.Add(itemName);
            }
            itemName += " in " + resolution.refreshRateRatio;
            // Debugger.Log("" + itemName, Debugger.PriorityLevel.Medium);

        }
        resolutionList.Reverse();
        _resolutionDropdown.AddOptions(resolutionList);
        var currentRes = Screen.currentResolution;
        var index = resolutionList.FindIndex(x => x == currentRes.width + seperator + currentRes.height);
        _resolutionDropdown.value = index;
    }


    public void SetQuality(int qualityIndex)
    {
        //Debugger.Log("Quality Setting is " + QualitySettings.GetQualitySettings().name, Debugger.PriorityLevel.MustShown);
        //Debugger.Log("Quality Setting names " + QualitySettings.names.ToString(), Debugger.PriorityLevel.MustShown);
        //Debugger.Log("New Quality Setting is " + qualityIndex, Debugger.PriorityLevel.MustShown);
        QualitySettings.SetQualityLevel(qualityIndex);

    }

    public void SetFps(int fpsLimitIndex)
    {
        //Debugger.Log("Current HZ from monitor" + Screen.currentResolution.refreshRate, Debugger.PriorityLevel.MustShown);
        Application.targetFrameRate = fpsArray[fpsLimitIndex % fpsArray.Length];
    }

    public void SetScreenResolution(int resolutionIndex)
    {
        var text = _resolutionDropdown.options[resolutionIndex].text;
        var result = text.Split(seperator);
        // resolutionIndex = resArray.Length-1 - resolutionIndex;
        //resolutionIndex %= resArray.Length;
        //Screen.SetResolution(resArray[resolutionIndex].width, resArray[resolutionIndex].height,true);
        Screen.SetResolution(int.Parse(result[0]), int.Parse(result[1]), true);
        //Debugger.Log("Resolution Set to" + Screen.currentResolution.width + seperator + Screen.currentResolution.height, Debugger.PriorityLevel.MustShown);
    }

    public void SwitchFPS(bool flag)
    {
        if (graphyFPS == null) return;

        graphyFPS.SetActive(flag);
    }

    public void BackButton()
    {
        SceneManager.LoadScene(0);
    }

    public void ResetGameState()
    {
        //   DataManager.instance.ResetGameState();
    }

    public void PlayOneShot(AudioClip clip)
    {
        // MusicManager.instance.PlayOneShotClip(clip);
    }
}