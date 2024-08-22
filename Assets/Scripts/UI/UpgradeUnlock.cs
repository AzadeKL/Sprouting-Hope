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
    public List<GameObject> silverTools;
    public int unlockSilverTools;

    [Space]
    [Header("GoldTools")]
    public List<GameObject> goldTools;
    public int unlockGoldTools;

    [Space]
    [Header("Animals")]
    public GameObject chicken;
    public GameObject pig;
    public int unlockChicken;
    public int unlockPig;

    [Space]
    [Header("Crops")]
    public GameObject lentils;
    public int unlockLentils;   
    // Start is called before the first frame update
    void Start()
    {
        //If not disabled, disable all upgrades
        foreach (GameObject tool in silverTools)
        {
            tool.SetActive(false);
        }
        foreach (GameObject tool in goldTools)
        {
            tool.SetActive(false);
        }
        chicken.SetActive(false);
        pig.SetActive(false);
        lentils.SetActive(false);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void checkUnlock(int progress)
    {
        if (progress >= unlockSilverTools)
        {
            foreach (GameObject tool in silverTools)
            {
                tool.SetActive(true);
            }
        }

        if (progress >= unlockGoldTools)
        {
            foreach (GameObject tool in goldTools)
            {
                tool.SetActive(true);
            }
        }

        if (progress >= unlockChicken)
        {
            chicken.SetActive(true);
        }

        if (progress >= unlockPig)
        {
            pig.SetActive(true);
        }

        if (progress >= unlockLentils)
        {
            lentils.SetActive(true);
        }
    }
}
