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
    // Enum for the field states
    private enum DirtFieldState
    {
        Default,
        Plowed,
        Watered,
        NumDirtFieldStates
    }

    //[SerializeField] private float farmingrange = 1f;

    public GameObject helpUI;
    private Toggle helpToggle;
    [SerializeField] private string showHelpOnNewGameKey = "showHelpOnNewGame";

    [Header("Time")]
    public float time;
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
    [Header("Plants")]
    [SerializeField] private float wateringTimeReduction;
    [SerializeField] private float wheatGrowTime;
    private Dictionary<Vector3Int, int> wheatPlants = new Dictionary<Vector3Int, int>();
    [SerializeField] private float tomatoGrowTime;
    private Dictionary<Vector3Int, int> tomatoPlants = new Dictionary<Vector3Int, int>();
    [SerializeField] private float lentilGrowTime;
    private Dictionary<Vector3Int, int> lentilPlants = new Dictionary<Vector3Int, int>();

    private Dictionary<Vector3Int, float> growStartTime = new Dictionary<Vector3Int, float>();
    private Dictionary<Vector3Int, float> growTotalTime = new Dictionary<Vector3Int, float>();

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
    [Header("AnimalManager")]
    [SerializeField] private AnimalManager animalManager;
    [Header("UpgradeManager")]
    [SerializeField] private UpgradeUnlock upgradeManager;

    private GameObject toolTip;//UI tooltip 


    private Dictionary<Vector3Int, int> tileState = new Dictionary<Vector3Int, int>();


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
        playerInventory = player.GetComponent<PlayerInventory>();
        toolTip = FindObjectOfType<Tooltip>(true).gameObject;
        //Debug.Log("Current showHelpOnNewGameKey is set to: " + PlayerPrefs.GetInt(showHelpOnNewGameKey, 2));
        animalManager = GetComponent<AnimalManager>();
        upgradeManager = GetComponent<UpgradeUnlock>();
        upgradeManager.disableAll();
        bool isNewGame = !SaveSystem.DataManager.instance.Load(this);
        bool showHelpOnNewGame = PlayerPrefs.GetInt(showHelpOnNewGameKey, 1) == 1;
        helpToggle.isOn = showHelpOnNewGame;
        helpUI.SetActive(isNewGame && showHelpOnNewGame);
    }
    public void Save(GameData gameData)
    {
        gameData.gameManagerMainProgress = mainProgress;

        foreach (var tile in tileState)
        {
            ISaveable.AddKey(gameData.gameManagerTileStates, tile.Key, tile.Value);
        }

        Func<string, KeyValuePair<Vector3Int, int>, string> PlantToEntry = (plantName, point) => string.Join(":", plantName, SaveSystem.ISaveable.Vector3IntToString(point.Key), point.Value, growStartTime[point.Key], growTotalTime[point.Key]);
        foreach (var key_value in wheatPlants) gameData.gameManagerPlants.Add(PlantToEntry("Wheat", key_value));
        foreach (var key_value in tomatoPlants) gameData.gameManagerPlants.Add(PlantToEntry("Tomato", key_value));
        foreach (var key_value in lentilPlants) gameData.gameManagerPlants.Add(PlantToEntry("Lentil", key_value));

        animalManager.Save(gameData);
    }
    public bool Load(GameData gameData)
    {
        mainProgress = gameData.gameManagerMainProgress;
        // upgradeManager.checkUnlock(mainProgress);
        foreach (var key_value in gameData.gameManagerTileStates)
        {
            var entry = ISaveable.ParseKey(key_value);
            var gridPosition = ISaveable.Vector3IntFromString(entry[0]);
            int fieldState = Convert.ToInt32(entry[1]);
            if (IsDirtFieldState(fieldState))
            {
                SetDirtFieldState(gridPosition, (DirtFieldState)fieldState);
            }
            else
            {
                // Field is a crop field
                SetDirtFieldState(gridPosition, DirtFieldState.Plowed);
            }
        }

        Debug.Log("Loading plants");
        foreach (var entry in gameData.gameManagerPlants)
        {
            Debug.Log("Loading plants: " + entry);
            var parsed = entry.Split(':');
            string cropName = parsed[0];
            Vector3Int gridPosition = SaveSystem.ISaveable.Vector3IntFromString(parsed[1]);
            int growthState = Convert.ToInt32(parsed[2]);
            float startTime = (float)Convert.ToDouble(parsed[3]);
            float totalTime = (float)Convert.ToDouble(parsed[4]);
            AddCrop(cropName, gridPosition, growthState, startTime, totalTime, false);
        }

        animalManager.Load(gameData, playerInventory);

        return true;
    }



    void SetIsModalMode(bool isSet)
    {
        isModalMode = isSet;
        player.GetComponent<PlayerMovement>().stop = isSet;
    }

    void UpdateIsModalMode()
    {
        SetIsModalMode(helpUI.activeSelf || inventoryUI.activeSelf || toolsUI.activeSelf || animalsUI.activeSelf || animalManager.IsModalMode());
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
        animalManager.ExitModalMode();
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

    private static bool IsDirtFieldState(int state)
    {
        return (state >= 0) && (state < (int)DirtFieldState.NumDirtFieldStates);
    }

    private static string GetSeedName(string cropName)
    {
        return cropName + " Seeds";
    }

    private static string GetCropName(string seedName)
    {
        return seedName.Substring(0, seedName.IndexOf(' '));
    }

    private bool GetCropVarsForCropName(string cropName, ref Dictionary<Vector3Int, int> cropPlants)
    {
        switch (cropName)
        {
            case "Wheat":
                cropPlants = wheatPlants;
                break;
            case "Tomato":
                cropPlants = tomatoPlants;
                break;
            case "Lentils":
                cropPlants = lentilPlants;
                break;
            default:
                cropPlants = null;
                break;
        }

        return cropPlants != null;
    }

    private bool GetCropVarsAtGridPosition(Vector3Int gridPosition, ref string cropName, ref Dictionary<Vector3Int, int> cropPlants)
    {
        cropName = "";
        if (wheatPlants.ContainsKey(gridPosition))
        {
            cropName = "Wheat";
        }
        else if (tomatoPlants.ContainsKey(gridPosition))
        {
            cropName = "Tomato";
        }
        else if (lentilPlants.ContainsKey(gridPosition))
        {
            cropName = "Lentils";
        }

        return GetCropVarsForCropName(cropName, ref cropPlants);
    }

    private void SetDirtFieldState(Vector3Int gridPosition, DirtFieldState dirtFieldState)
    {
        Debug.Log("Set dirt field state at position: " + gridPosition + ", dirtFieldState: " + dirtFieldState + " = " + (int)dirtFieldState);
        if (tileState.ContainsKey(gridPosition))
        {
            tileState[gridPosition] = (int)dirtFieldState;
        }
    }

    private void AddCrop(string cropName, Vector3Int gridPosition, int growthState = 0, float startTime = -1f, float totalTime = 60f, bool useSeed = true)
    {
        Debug.Log("Adding crop(" + cropName + ") at position: " + gridPosition + ", growth: " + growthState);
        if (!tileState.ContainsKey(gridPosition) || (tileState[gridPosition] < 1))
        {
            Debug.Log("Failed to add crop - grid location not initialized: " + gridPosition);
            return;
        }

        Dictionary<Vector3Int, int> cropPlants = null;
        if (!GetCropVarsForCropName(cropName, ref cropPlants))
        {
            Debug.Log("Invalid crop(" + cropName + ") at position: " + gridPosition);
            SetDirtFieldState(gridPosition, DirtFieldState.Default);
            return;
        }
        if (startTime < 0) startTime = time;
        Debug.Log(startTime);
        growStartTime.Add(gridPosition, startTime);
        growTotalTime.Add(gridPosition, totalTime);
        cropPlants.Add(gridPosition, growthState);
        string seedName = GetSeedName(cropName);
        if (useSeed) playerInventory.RemoveFromInventory(seedName);
        StartCoroutine(GrowTime(gridPosition));
        Debug.Log("Planted " + seedName);
    }

    public void UpdateCrops(Vector3Int gridPosition)
    {
        string cropName = "";
        Dictionary<Vector3Int, int> cropPlants = null;
        if (!GetCropVarsAtGridPosition(gridPosition, ref cropName, ref cropPlants))
        {
            Debug.Log("No crop to update at position: " + gridPosition);
            SetDirtFieldState(gridPosition, (DirtFieldState)Mathf.Max(0, tileState[gridPosition] - 1));
            return;
        }

        Debug.Log(cropName + " is growing!");
        Debug.Log(time);
        growStartTime[gridPosition] = time;
        if (cropPlants[gridPosition] < 2) ++cropPlants[gridPosition];
        if (cropPlants[gridPosition] < 2) StartCoroutine(GrowTime(gridPosition));
        else StartCoroutine(DefaultSoil(gridPosition));
    }


    public Transform GetChickenSlot()
    {
        return animalManager.GetChickenSlot();
    }
    public Transform GetPigSlot()
    {
        return animalManager.GetPigSlot();
    }
    public Transform GetChickenFeedSlot()
    {
        return animalManager.GetChickenFeedSlot();
    }
    public Transform GetPigFeedSlot()
    {
        return animalManager.GetPigFeedSlot();
    }
    public void AddChickenFeed(int amount)
    {
        animalManager.AddChickenFeed(amount);
    }
    public void AddPigFeed(int amount)
    {
        animalManager.AddPigFeed(amount);
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

    IEnumerator GrowTime(Vector3Int gridPosition)
    {
        if (!growTotalTime.ContainsKey(gridPosition)) growTotalTime.Add(gridPosition, 60f);
        // set growth time by plant type
        if (wheatPlants.ContainsKey(gridPosition)) growTotalTime[gridPosition] = wheatGrowTime / timePerTick;
        else if (tomatoPlants.ContainsKey(gridPosition)) growTotalTime[gridPosition] = tomatoGrowTime / timePerTick;
        else if (lentilPlants.ContainsKey(gridPosition)) growTotalTime[gridPosition] = lentilGrowTime / timePerTick;

        Debug.Log("Waiting for time to pass...");
        yield return new WaitUntil(() => time - growStartTime[gridPosition] >= growTotalTime[gridPosition]);
        Debug.Log(time + " = " + growStartTime[gridPosition] + " + " + growTotalTime[gridPosition]);

        SetDirtFieldState(gridPosition, DirtFieldState.Plowed);
        UpdateCrops(gridPosition);
    }

    public int GetNumPigs()
    {
        return animalManager.GetNumPigs();
    }

    public int GetNumChickens()
    {
        return animalManager.GetNumChickens();
    }

    public int GetNumEggs()
    {
        return animalManager.GetNumEggs();
    }

    public void UpdateAnimals()
    {
        animalManager.UpdateAnimals(playerInventory);
    }

    IEnumerator DefaultSoil(Vector3Int gridPosition)
    {
        float time = 60f;
        yield return new WaitForSeconds(time);
        if (!wheatPlants.ContainsKey(gridPosition) && !tomatoPlants.ContainsKey(gridPosition) && !lentilPlants.ContainsKey(gridPosition))
        {
            SetDirtFieldState(gridPosition, (DirtFieldState)Mathf.Max(0, tileState[gridPosition] - 1));
            StartCoroutine(DefaultSoil(gridPosition));
        }
    }

}