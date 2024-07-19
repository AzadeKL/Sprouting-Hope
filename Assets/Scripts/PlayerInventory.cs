using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{

    // TEMP string for hand
    public string hand = "hoe";


    [SerializeField] private GameObject inventoryUI;

    // index variable labeling what is currently equipped to hand
    public int handIndex = 0;

    // hotbar that can scroll through
    public List<string> hotBar;

    // inventory
    public List<GameObject> inventory;


    void Awake()
    {
        // set up starting inventory
        hotBar = new List<string>{"hoe","watering can","wheat seeds","",""};

    }

    void Update()
    {
        // enable/disable inventory window
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }

        // scroll wheel down the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (handIndex == 0) handIndex = 4;
            else handIndex--;
            Debug.Log(handIndex);
        }
        // scroll wheel up the hotbar
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            handIndex = (handIndex + 1) % 5;
            Debug.Log(handIndex);
        }
        // number keys for specific hotbar slots
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            handIndex = 0;
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            handIndex = 1;
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            handIndex = 2;
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            handIndex = 3;
        }
        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            handIndex = 4;
        }
    }
}
