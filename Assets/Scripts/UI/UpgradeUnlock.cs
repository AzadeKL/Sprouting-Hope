using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.Mathematics;
using UnityEngine;

public class UpgradeUnlock : MonoBehaviour
{

    // public GameManager gameManager;

    [Space]
    [Header("SilverTools")]
    // public List<GameObject> silverTools;
    public List<UpgradeIcon> silverToolsIcons;
    public int unlockSilverTools;

    [Space]
    [Header("GoldTools")]
    // public List<GameObject> goldTools;
    public List<UpgradeIcon> goldToolsIcons;
    public int unlockGoldTools;

    [Space]
    [Header("Animals")]
    // public GameObject chicken;
    public UpgradeIcon chickenIcon;
    // public GameObject pig;
    public UpgradeIcon pigIcon;
    public int unlockChicken;
    public int unlockPig;

    [Space]
    [Header("Crops")]
    // public GameObject lentils;
    public UpgradeIcon lentilsIcon;
    public int unlockLentils;


    void Awake()
    {
        //If not disabled, disable all upgrades
        Debug.Log("Awake");
        foreach (UpgradeIcon tool in silverToolsIcons)
        {
            tool.DisableIcon();
        }
        foreach (UpgradeIcon tool in goldToolsIcons)
        {
            tool.DisableIcon();
        }
        chickenIcon.DisableIcon();
        pigIcon.DisableIcon();
        lentilsIcon.DisableIcon();
    }

    // Start is called before the first frame update
    void Start()
    {
        // //If not disabled, disable all upgrades
        // foreach (UpgradeIcon tool in silverToolsIcons)
        // {
        //     tool.Disable();
        // }
        // foreach (UpgradeIcon tool in goldToolsIcons)
        // {
        //     tool.Disable();
        // }
        // chickenIcon.Disable();
        // pigIcon.Disable();
        // lentilsIcon.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void checkUnlock(int progress)
    {
        if (progress >= unlockSilverTools)
        {
            foreach (UpgradeIcon tool in silverToolsIcons)
            {
                tool.Enable();
            }
        }

        if (progress >= unlockGoldTools)
        {
            foreach (UpgradeIcon tool in goldToolsIcons)
            {
                tool.Enable();
            }
        }

        if (progress >= unlockChicken)
        {
            chickenIcon.Enable();
        }

        if (progress >= unlockPig)
        {
            pigIcon.Enable();
        }

        if (progress >= unlockLentils)
        {
            lentilsIcon.Enable();
        }
    }
}
