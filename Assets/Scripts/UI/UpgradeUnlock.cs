using System.Collections.Generic;
using UnityEngine;

public class UpgradeUnlock : MonoBehaviour
{
    [Header("SilverTools")]
    public List<UpgradeIcon> silverToolsIcons;
    public int unlockSilverTools;

    [Space]
    [Header("GoldTools")]
    public List<UpgradeIcon> goldToolsIcons;
    public int unlockGoldTools;

    [Space]
    [Header("Animals")]
    public UpgradeIcon chickenIcon;
    public UpgradeIcon pigIcon;
    public int unlockChicken;
    public int unlockPig;

    [Space]
    [Header("Crops")]
    public UpgradeIcon lentilsIcon;
    public int unlockLentils;


    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void disableAll()
    {
        Debug.Log("Disable All");
        for (int i = 0; i < silverToolsIcons.Count; i++)
        {
            silverToolsIcons[i].DisableIcon();
            silverToolsIcons[i].setUpgradeRequirement(unlockSilverTools);
        }
        for (int i = 0; i < goldToolsIcons.Count; i++)
        {
            goldToolsIcons[i].DisableIcon();
            goldToolsIcons[i].setUpgradeRequirement(unlockGoldTools);
        }
        
        chickenIcon.DisableIcon();
        chickenIcon.setUpgradeRequirement(unlockChicken);
        
        pigIcon.DisableIcon();
        pigIcon.setUpgradeRequirement(unlockPig);
        
        lentilsIcon.DisableIcon();
        lentilsIcon.setUpgradeRequirement(unlockLentils);
    }

    public void checkUnlock(int progress)
    {
        if (progress >= unlockSilverTools)
        {
            foreach (UpgradeIcon tool in silverToolsIcons)
            {
                tool.EnableIcon();
            }
        }

        if (progress >= unlockGoldTools)
        {
            foreach (UpgradeIcon tool in goldToolsIcons)
            {
                tool.EnableIcon();
            }
        }

        if (progress >= unlockChicken)
        {
            chickenIcon.EnableIcon();
        }

        if (progress >= unlockPig)
        {
            pigIcon.EnableIcon();
        }

        if (progress >= unlockLentils)
        {
            lentilsIcon.EnableIcon();
        }
    }
}
