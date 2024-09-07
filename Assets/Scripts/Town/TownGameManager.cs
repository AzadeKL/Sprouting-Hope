using SaveSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TownGameManager : MonoBehaviour, SaveSystem.ISaveable
{
    public GameObject helpUI;
    private Toggle helpToggle;
    [SerializeField] private string showHelpOnNewGameKey = "showHelpOnNewGame";

    [Header("Time")]
    private DayNightCycle dayNightCycle;
    private float time;
    private int day;
    [SerializeField] private FloatReference time24HFormat;
    [SerializeField] private float timePerTick;

    [Space]
    [Header("Tilemap")]
    [SerializeField] private Tilemap grassMap;
    [SerializeField] private Tilemap buildings;

    [Space]
    [Header("Player")]
    public GameObject player;
    private PlayerInventory playerInventory;
    public GameObject inventoryUI;
    public Image inventoryUIHeaderImage;
    public TextMeshProUGUI inventoryUIHeaderTextField;
    [SerializeField] Color normalInventory;
    [SerializeField] Color sellInventory;
    [SerializeField] Color giveInventory;
    public Transform playerCenter;
    [SerializeField] private GameObject playerWorldCanvas;

    [Space]
    [Header("Main Objective")]
    public int mainProgress;
    [SerializeField] private GameObject winText;
    [SerializeField] private int maxProgress;
    [SerializeField] private Slider progressMeter;

    [Space]
    [Header("Buildings")]
    public List<TileBase> restaurant;
    public List<TileBase> truck;
    public List<TileBase> humanitarian;
    public List<TileBase> tools;
    [SerializeField] private GameObject toolsUI;
    public List<TileBase> animals;
    [SerializeField] private GameObject animalsUI;

    [Space]
    [Header("UpgradeManager")]
    [SerializeField] private UpgradeUnlock upgradeManager;

    private GameObject toolTip;//UI tooltip 

    private Vector3Int[] neighborPositions =
    {
        Vector3Int.up,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.down
    };
    private bool isModalMode = false;
    public bool escDisabled = false;

    private AudioSource audioSource;
    private void Awake()
    {
        helpToggle = helpUI.transform.GetChild(2).GetComponent<Toggle>();
        progressMeter.maxValue = maxProgress;
        audioSource = GetComponent<AudioSource>();

    }
    private void Start()
    {
        dayNightCycle = GameObject.Find("Global Light 2D").GetComponent<DayNightCycle>();
        playerInventory = player.GetComponent<PlayerInventory>();
        toolTip = FindObjectOfType<Tooltip>(true).gameObject;
        //Debug.Log("Current showHelpOnNewGameKey is set to: " + PlayerPrefs.GetInt(showHelpOnNewGameKey, 2));
        upgradeManager = GetComponent<UpgradeUnlock>();
        upgradeManager.disableAll();
        bool isNewGame = !SaveSystem.DataManager.instance.Load(this);
        if (isNewGame)
        {
            time = dayNightCycle.GetTime();
            day = dayNightCycle.GetDay();
        }
        bool showHelpOnNewGame = PlayerPrefs.GetInt(showHelpOnNewGameKey, 1) == 1;
        helpToggle.isOn = showHelpOnNewGame;
        helpUI.SetActive(isNewGame && showHelpOnNewGame);
    }
    public void Save(GameData gameData)
    {
        Debug.Log("Saving TownGameManager");

        gameData.gameManagerMainProgress = mainProgress;

        gameData.townSaveTime = time;
        gameData.townSaveDay = day;
    }
    public bool Load(GameData gameData)
    {
        Debug.Log("Loading TownGameManager");

        mainProgress = gameData.gameManagerMainProgress;
        // upgradeManager.checkUnlock(mainProgress);

        time = gameData.townSaveTime;
        day = gameData.townSaveDay;

        return true;
    }



    void SetIsModalMode(bool isSet)
    {
        isModalMode = isSet;
        player.GetComponent<PlayerMovement>().stop = isSet;
    }

    void UpdateIsModalMode()
    {
        SetIsModalMode(helpUI.activeSelf || inventoryUI.activeSelf || toolsUI.activeSelf || animalsUI.activeSelf);
        // If a modal mode is active, disable the pause menu, for the rest of the frame.
        PauseMenu.instance.notAllowed = isModalMode;
    }

    void ExitModalMode()
    {
        ExitInventoryMode();
        helpUI.SetActive(false);
        toolsUI.SetActive(false);
        animalsUI.SetActive(false);
        playerInventory.sellMode = false;
        playerInventory.giveMode = false;
        SetIsModalMode(false);
    }

    void EnterInventoryMode()
    {
        inventoryUI.SetActive(true);
        // toolTip.SetActive(true);
    }

    void ExitInventoryMode()
    {
        inventoryUI.SetActive(false);
        playerInventory.sellMode = false;
        playerInventory.giveMode = false;
        inventoryUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = normalInventory;
        inventoryUIHeaderImage.color = normalInventory;
        inventoryUIHeaderTextField.text = "Inventory";
        toolTip.SetActive(false);
    }

    public void UpdateHelpPrefFromPopup()
    {
        //Debug.Log("Setting showHelpOnNewGameKey to: " + ((helpToggle.isOn) ? 1 : 0));
        PlayerPrefs.SetInt(showHelpOnNewGameKey, (helpToggle.isOn) ? 1 : 0);
    }

    private bool GetBuildingOpen(string building)
    {
        switch (building)
        {
            case "Restaurant":
                return time24HFormat.Value >= 8f && time24HFormat.Value <= 20f;
            case "Truck":
                return time24HFormat.Value >= 8f && time24HFormat.Value <= 20f;
            case "Humanitarian":
                return time24HFormat.Value >= 8f && time24HFormat.Value <= 20f;
            default:
                return false;
        }
    }

    private void Update()
    {
        // Check if the game is paused
        //Debugger.Log("Pause state: " + PauseMenu.instance.IsPaused());
        if (PauseMenu.instance.IsPaused()) return;

        int last_day = day;
        time = dayNightCycle.GetTime();
        day = dayNightCycle.GetDay();
        for (int i = last_day; i < day; ++i) NewDay();

        progressMeter.value = mainProgress;
        progressMeter.gameObject.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = mainProgress.ToString() + "/" + maxProgress.ToString();
        upgradeManager.checkUnlock(mainProgress);
        // if progress meets requirement, win the game (prompt to return to menu or continue playing?)
        if (mainProgress >= maxProgress)
        {
            winText.SetActive(true);
        }

        UpdateIsModalMode();

        // interact with building or other interactable object
        if (isModalMode == false)
        {

            var interactWBuildingKeyPressed = Input.GetKeyUp(KeyCode.F);

            Vector3Int gridPosition = buildings.WorldToCell(playerCenter.transform.position);
            foreach (Vector3Int neighborPosition in neighborPositions)
            {
                if (buildings.HasTile(gridPosition + neighborPosition))
                {
                    if (restaurant.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debugger.Log("Interacting with Restaurant!", Debugger.PriorityLevel.LeastImportant);

                        if (interactWBuildingKeyPressed)
                        {
                            inventoryUI.SetActive(true);
                            playerInventory.sellMode = true;
                            inventoryUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = sellInventory;
                            inventoryUIHeaderImage.color = sellInventory;
                            inventoryUIHeaderTextField.text = "Fresh Food";
                        }
                        else
                        {
                            playerWorldCanvas.SetActive(true);
                        }


                        break;
                    }
                    else if (truck.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        if (interactWBuildingKeyPressed)
                        {
                            // TODO: Find an event that triggers right before scene unload
                            SaveSystem.DataManager.instance.UpdateAndSaveToFile();
                            SceneManager.LoadScene("SampleScene");
                        }
                        else
                        {
                            playerWorldCanvas.SetActive(true);
                        }
                        break;
                    }
                    else if (humanitarian.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debugger.Log("Interacting with Humanitarian Aid!", Debugger.PriorityLevel.LeastImportant);

                        if (interactWBuildingKeyPressed)
                        {
                            inventoryUI.SetActive(true);
                            playerInventory.giveMode = true;
                            inventoryUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = giveInventory;
                            inventoryUIHeaderImage.color = giveInventory;
                            inventoryUIHeaderTextField.text = "Donate";
                        }
                        else
                        {
                            playerWorldCanvas.SetActive(true);
                        }


                        break;
                    }
                    else if (tools.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debugger.Log("Interacting with tools!", Debugger.PriorityLevel.LeastImportant);

                        if (interactWBuildingKeyPressed)
                        {
                            toolsUI.SetActive(true);
                        }
                        else
                        {
                            playerWorldCanvas.SetActive(true);
                        }

                        break;
                    }
                    else if (animals.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debugger.Log("Interacting with animals!", Debugger.PriorityLevel.LeastImportant);

                        if (interactWBuildingKeyPressed)
                        {
                            animalsUI.SetActive(true);
                        }
                        else
                        {
                            playerWorldCanvas.SetActive(true);
                        }

                        break;
                    }
                }
                else
                {
                    playerWorldCanvas.SetActive(false);
                }
            }
        }

        // enable/disable help window only
        if (Input.GetKeyUp(KeyCode.H))
        {
            if (isModalMode)
            {
                if (helpUI.activeSelf) helpUI.SetActive(false);
            }
            else
            {
                helpUI.SetActive(true);
            }
        }

        // enable/disable inventory window
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (isModalMode)
            {
                // Inventory may be open with other menus - exit them all
                if (inventoryUI.activeSelf || toolsUI.activeSelf || animalsUI.activeSelf) ExitModalMode();
            }
            else
            {
                EnterInventoryMode();
            }
        }

        // esc key to either close existing windows or open pause menu
        if (Input.GetKeyUp(KeyCode.Escape) && escDisabled == false)
        {
            if (isModalMode)
            {
                ExitModalMode();
            }
            // The PauseMenu handles Esc key-presses independently.
        }
    }

    private void NewDay()
    {
    }
}