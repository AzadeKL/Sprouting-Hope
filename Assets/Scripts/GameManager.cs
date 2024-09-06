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

public class GameManager : MonoBehaviour, SaveSystem.ISaveable
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
    [SerializeField] private Tilemap farmLand;
    [SerializeField] private Tilemap farmPlants;
    [SerializeField] private Tilemap buildings;

    [Space]
    [SerializeField] private SeedFactory seedFactory;
    [SerializeField] private GameObject waterPrefab;

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
    [SerializeField] private Transform outliner;
    [SerializeField] private float itemRange = 3f;
    [SerializeField] private GameObject playerWorldCanvas;
    [SerializeField] private Image playerActionImage;

    [Space]
    [Header("GroundTiles")]

    [SerializeField] private TileBase defaultField;
    [SerializeField] private TileBase plowedField;
    [SerializeField] private TileBase wateredField;

    [Space]
    [Header("Plants")]
    [SerializeField] private float wateringTimeReduction;
    public List<Tile> wheat;
    [SerializeField] private float wheatGrowTime;
    private Dictionary<Vector3Int, int> wheatPlants = new Dictionary<Vector3Int, int>();
    public List<Tile> tomato;
    [SerializeField] private float tomatoGrowTime;
    private Dictionary<Vector3Int, int> tomatoPlants = new Dictionary<Vector3Int, int>();
    public List<Tile> lentil;
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
    public List<TileBase> truck;
    public List<TileBase> house;
    [SerializeField] private GameObject houseUI;

    [Space]
    [Header("Sound Effects")]
    [SerializeField] private List<AudioClip> plowSounds;
    [SerializeField] private List<AudioClip> reapSounds;
    [SerializeField] private List<AudioClip> waterSounds;

    [Space]
    [Header("AnimalManager")]
    [SerializeField] private AnimalManager animalManager;

    private GameObject toolTip;//UI tooltip 


    private Dictionary<Vector3Int, int> tileState = new Dictionary<Vector3Int, int>();


    private Vector3Int[] neighborPositions =
    {
        Vector3Int.up,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.down
    };
    private bool disableTool = false;
    private bool isModalMode = false;
    public bool escDisabled = false;

    private float itemRangeModifier = 0f;
    private float timer = 1f;
    private float timerModifierPercentage = 0f;
    private float baselineTimerModifierPercentage = 0f;

    private AudioSource audioSource;
    private void Awake()
    {
        helpToggle = helpUI.transform.GetChild(2).GetComponent<Toggle>();
        if (seedFactory == null) { seedFactory = GetComponent<SeedFactory>(); }
        progressMeter.maxValue = maxProgress;
        audioSource = GetComponent<AudioSource>();

    }
    private void Start()
    {
        playerInventory = player.GetComponent<PlayerInventory>();
        toolTip = FindObjectOfType<Tooltip>(true).gameObject;
        //Debug.Log("Current showHelpOnNewGameKey is set to: " + PlayerPrefs.GetInt(showHelpOnNewGameKey, 2));
        animalManager = GetComponent<AnimalManager>();
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
            ISaveable.AddKey(gameData.farmGameManagerTileStates, tile.Key, tile.Value);
        }

        Func<string, KeyValuePair<Vector3Int, int>, string> PlantToEntry = (plantName, point) => string.Join(":", plantName, SaveSystem.ISaveable.Vector3IntToString(point.Key), point.Value, growStartTime[point.Key], growTotalTime[point.Key]);
        foreach (var key_value in wheatPlants) gameData.farmGameManagerPlants.Add(PlantToEntry("Wheat", key_value));
        foreach (var key_value in tomatoPlants) gameData.farmGameManagerPlants.Add(PlantToEntry("Tomato", key_value));
        foreach (var key_value in lentilPlants) gameData.farmGameManagerPlants.Add(PlantToEntry("Lentil", key_value));
    }
    public bool Load(GameData gameData)
    {
        mainProgress = gameData.gameManagerMainProgress;
        foreach (var key_value in gameData.farmGameManagerTileStates)
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
        foreach (var entry in gameData.farmGameManagerPlants)
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

        return true;
    }



    void SetIsModalMode(bool isSet)
    {
        isModalMode = isSet;
        player.GetComponent<PlayerMovement>().stop = isSet;
    }

    void UpdateIsModalMode()
    {
        SetIsModalMode(helpUI.activeSelf || houseUI.activeSelf || inventoryUI.activeSelf || animalManager.IsModalMode());
        // If a modal mode is active, disable the pause menu, for the rest of the frame.
        PauseMenu.instance.notAllowed = isModalMode;
    }

    void ExitModalMode()
    {
        ExitInventoryMode();
        helpUI.SetActive(false);
        houseUI.SetActive(false);
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

    private bool GetCropVarsForCropName(string cropName, ref List<Tile> crop, ref Dictionary<Vector3Int, int> cropPlants)
    {
        switch (cropName)
        {
            case "Wheat":
                crop = wheat;
                cropPlants = wheatPlants;
                break;
            case "Tomato":
                crop = tomato;
                cropPlants = tomatoPlants;
                break;
            case "Lentils":
                crop = lentil;
                cropPlants = lentilPlants;
                break;
            default:
                crop = null;
                cropPlants = null;
                break;
        }

        return crop != null;
    }

    private bool GetCropVarsAtGridPosition(Vector3Int gridPosition, ref string cropName, ref List<Tile> crop, ref Dictionary<Vector3Int, int> cropPlants)
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

        return GetCropVarsForCropName(cropName, ref crop, ref cropPlants);
    }

    private void SetDirtFieldState(Vector3Int gridPosition, DirtFieldState dirtFieldState)
    {
        TileBase fieldType = null;
        switch (dirtFieldState)
        {
            case DirtFieldState.Default:
                fieldType = defaultField;
                break;
            case DirtFieldState.Plowed:
                fieldType = plowedField;
                break;
            case DirtFieldState.Watered:
                fieldType = wateredField;
                break;
            default:
                Debug.Log("Unrecognized dirt field state(" + dirtFieldState + ") at position: " + gridPosition);
                return;
        }
        Debug.Log("Set dirt field state at position: " + gridPosition + ", dirtFieldState: " + dirtFieldState + " = " + (int)dirtFieldState);
        if (!tileState.ContainsKey(gridPosition))
        {
            tileState.Add(gridPosition, (int)dirtFieldState);
        }
        else
        {
            tileState[gridPosition] = (int)dirtFieldState;
        }
        farmLand.SetTile(gridPosition, fieldType);
    }

    private void PlowField(Vector3Int gridPosition)
    {
        if (!tileState.ContainsKey(gridPosition) || tileState[gridPosition] < 1)
        {
            SetDirtFieldState(gridPosition, DirtFieldState.Plowed);
            seedFactory.CreateSeed(gridPosition);
            PlayActionSound(plowSounds[Random.Range(0, plowSounds.Count)]);
        }
    }

    private void WaterField(Vector3Int gridPosition)
    {
        if (!tileState.ContainsKey(gridPosition) || (tileState[gridPosition] < 1))
        {
            Debug.Log("Failed to water crop - grid location not initialized: " + gridPosition);
            return;
        }

        if (farmLand.GetTile(gridPosition) == (wateredField)) return;

        if (growTotalTime.ContainsKey(gridPosition)) growTotalTime[gridPosition] *= wateringTimeReduction;
        SetDirtFieldState(gridPosition, DirtFieldState.Watered);
        PlayActionSound(waterSounds[Random.Range(0, waterSounds.Count)]);
        Instantiate(waterPrefab, gridPosition, Quaternion.identity);
    }

    private void AddCrop(string cropName, Vector3Int gridPosition, int growthState = 0, float startTime = -1f, float totalTime = 60f, bool useSeed = true)
    {
        Debug.Log("Adding crop(" + cropName + ") at position: " + gridPosition + ", growth: " + growthState);
        if (!tileState.ContainsKey(gridPosition) || (tileState[gridPosition] < 1))
        {
            Debug.Log("Failed to add crop - grid location not initialized: " + gridPosition);
            return;
        }
        if (farmPlants.HasTile(gridPosition))
        {
            Debug.Log("Failed to add cropt - grid location in use: " + gridPosition);
            return;
        }

        List<Tile> crop = null;
        Dictionary<Vector3Int, int> cropPlants = null;
        if (!GetCropVarsForCropName(cropName, ref crop, ref cropPlants))
        {
            Debug.Log("Invalid crop(" + cropName + ") at position: " + gridPosition);
            SetDirtFieldState(gridPosition, DirtFieldState.Default);
            return;
        }
        if (startTime < 0) startTime = time;
        Debug.Log(startTime);
        growStartTime.Add(gridPosition, startTime);
        growTotalTime.Add(gridPosition, totalTime);
        farmPlants.SetTile(gridPosition, crop[growthState]);
        cropPlants.Add(gridPosition, growthState);
        string seedName = GetSeedName(cropName);
        if (useSeed) playerInventory.RemoveFromInventory(seedName);
        StartCoroutine(GrowTime(gridPosition));
        Debug.Log("Planted " + seedName);
    }

    public void UpdateCrops(Vector3Int gridPosition)
    {
        string cropName = "";
        List<Tile> crop = null;
        Dictionary<Vector3Int, int> cropPlants = null;
        if (!GetCropVarsAtGridPosition(gridPosition, ref cropName, ref crop, ref cropPlants))
        {
            Debug.Log("No crop to update at position: " + gridPosition);
            SetDirtFieldState(gridPosition, (DirtFieldState)Mathf.Max(0, tileState[gridPosition] - 1));
            return;
        }

        Debug.Log(cropName + " is growing!");
        Debug.Log(time);
        growStartTime[gridPosition] = time;
        if (cropPlants[gridPosition] < 2) ++cropPlants[gridPosition];
        farmPlants.SetTile(gridPosition, crop[cropPlants[gridPosition]]);
        if (cropPlants[gridPosition] < 2) StartCoroutine(GrowTime(gridPosition));
        else StartCoroutine(DefaultSoil(gridPosition));
    }

    private void HarvestCrop(Vector3Int gridPosition)
    {
        string cropName = "";
        List<Tile> crop = null;
        Dictionary<Vector3Int, int> cropPlants = null;
        if (!GetCropVarsAtGridPosition(gridPosition, ref cropName, ref crop, ref cropPlants))
        {
            Debug.Log("No crop to harvest at position: " + gridPosition);
            return;
        }
        if (cropPlants[gridPosition] < 2)
        {
            Debug.Log("Crop(" + cropName + " is too young to harvest at position: " + gridPosition + " growth: " + cropPlants[gridPosition]);
            return;
        }

        farmPlants.SetTile(gridPosition, null);
        growStartTime.Remove(gridPosition);
        growTotalTime.Remove(gridPosition);
        cropPlants.Remove(gridPosition);
        seedFactory.CreateCrop(gridPosition, cropName);
        Debug.Log("Harvested " + cropName);
        PlayActionSound(reapSounds[Random.Range(0, reapSounds.Count)]);
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
    public int GetChickenFeed()
    {
        return animalManager.GetChickenFeed();
    }
    public int GetPigFeed()
    {
        return animalManager.GetPigFeed();
    }

    private bool GetBuildingOpen(string building)
    {
        switch (building)
        {
            case "Restaurant":
                return time24HFormat.Value >= 8f && time24HFormat.Value <= 20f;
            case "Truck":
                return time24HFormat.Value >= 8f && time24HFormat.Value <= 20f;
            default:
                return false;
        }
    }

    public void ChangeBuildingState(string building, bool full)
    {
        List<TileBase> chickenCoop = animalManager.chickenCoop;
        List<TileBase> PigPen = animalManager.pigPen;

        Debug.Log("Finding building " + building);
        string newbuilding;
        switch (building)
        {
            case "Chicken Coop":
                if (full)
                {
                    building = "Chicken Coop_EMPTY_";
                    newbuilding = "Chicken Coop_FULL_";
                }
                else
                {
                    building = "Chicken Coop_FULL_";
                    newbuilding = "Chicken Coop_EMPTY_";
                }
                for (int i = 0; i < 15; i++)
                {
                    TileBase oldTile = null;
                    TileBase newTile = null;
                    foreach (TileBase tile in animalManager.chickenCoop)
                    {
                        if (tile.name == building + i.ToString())
                        {
                            oldTile = tile;
                        }
                        else if (tile.name == newbuilding + i.ToString())
                        {
                            newTile = tile;
                        }
                        if (oldTile && newTile)
                        {
                            buildings.SwapTile(oldTile, newTile);
                            break;
                        }
                    }
                }
                break;
            case "Pig Pen":
                if (full)
                {
                    building = "Trough-Empty_";
                    newbuilding = "Trough-Filled_";
                }
                else
                {
                    building = "Trough-Filled_";
                    newbuilding = "Trough-Empty_";
                }
                for (int i = 0; i < 10; i++)
                {
                    TileBase oldTile = null;
                    TileBase newTile = null;
                    foreach (TileBase tile in animalManager.pigPen)
                    {
                        if (tile.name == building + i.ToString())
                        {
                            oldTile = tile;
                        }
                        else if (tile.name == newbuilding + i.ToString())
                        {
                            newTile = tile;
                        }
                        if (oldTile && newTile)
                        {
                            buildings.SwapTile(oldTile, newTile);
                            break;
                        }
                    }
                }
                break;
        }
    }

    private void Update()
    {
        // Check if the game is paused
        //Debugger.Log("Pause state: " + PauseMenu.instance.IsPaused());
        if (PauseMenu.instance.IsPaused()) return;
        progressMeter.value = mainProgress;
        progressMeter.gameObject.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = mainProgress.ToString() + "/" + maxProgress.ToString();
        // if progress meets requirement, win the game (prompt to return to menu or continue playing?)
        if (mainProgress >= maxProgress)
        {
            winText.SetActive(true);
        }

        Vector3 mousepos = Input.mousePosition;
        mousepos = Camera.main.ScreenToWorldPoint(mousepos);
        var tilePos = farmLand.WorldToCell(mousepos);
        var hoveredTile = farmLand.GetTile(tilePos);
        // Debugger.Log(hoveredTile.name, Debugger.PriorityLevel.Low);
        if ((tilePos - playerCenter.position).sqrMagnitude < (itemRange + itemRangeModifier) && hoveredTile != null)
        {
            outliner.transform.position = tilePos;
            disableTool = false;
            outliner.gameObject.SetActive(true);
        }
        else
        {
            outliner.gameObject.SetActive(false);
            disableTool = true;
        }

        UpdateIsModalMode();

        // interact with building or other interactable object
        if (isModalMode == false)
        {

            var interactWBuildingKeyPressed = Input.GetKeyUp(KeyCode.F);

            Vector3Int gridPosition = buildings.WorldToCell(playerCenter.position);
            foreach (Vector3Int neighborPosition in neighborPositions)
            {
                if (truck.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                {
                    Debugger.Log("Interacting with Truck!", Debugger.PriorityLevel.LeastImportant);

                    if (interactWBuildingKeyPressed)
                    {
                        SceneManager.LoadScene("TownScene");
                    }
                    else
                    {
                        playerWorldCanvas.SetActive(true);
                    }
                    break;
                }
                if (animalManager.GetChickenCoop().Contains(buildings.GetTile(gridPosition + neighborPosition)))
                {
                    Debugger.Log("Interacting with Chicken Coop!", Debugger.PriorityLevel.LeastImportant);
                    if (interactWBuildingKeyPressed)
                    {
                        animalManager.GetChickenUI().SetActive(true);
                        inventoryUI.SetActive(true);
                    }
                    else
                    {
                        playerWorldCanvas.SetActive(true);
                    }
                    break;
                }
                else if (house.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                {
                    Debugger.Log("Interacting with Farmhouse!", Debugger.PriorityLevel.LeastImportant);

                    if (interactWBuildingKeyPressed)
                    {
                        houseUI.SetActive(true);
                    }
                    else
                    {
                        playerWorldCanvas.SetActive(true);
                    }

                    break;
                }
                else if (animalManager.GetPigPen().Contains(buildings.GetTile(gridPosition + neighborPosition)))
                {
                    Debugger.Log("Interacting with Pig Pen!", Debugger.PriorityLevel.LeastImportant);

                    if (interactWBuildingKeyPressed)
                    {
                        animalManager.GetPigUI().SetActive(true);
                        inventoryUI.SetActive(true);
                    }
                    else
                    {
                        playerWorldCanvas.SetActive(true);
                    }

                    break;
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
                if (inventoryUI.activeSelf) ExitModalMode();
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

        // left click to interact with tool to tile
        if (Input.GetMouseButton(0) && disableTool == false)
        {
            if (!inventoryUI.activeSelf && CheckTimer())
            {
                timer = 0f;
                Vector3 mousePosition = Input.mousePosition;
                mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
                mousePosition.z = 0;
                var playerpos = playerCenter.position;
                playerpos.z = 0;
                var result = mousePosition - playerpos;
                result.Normalize();
                Debugger.Log(result + " mouse direction normalized", Debugger.PriorityLevel.Medium);
                Vector3Int gridPosition = farmLand.WorldToCell(tilePos);
                TileBase clickedTile = farmLand.GetTile(gridPosition);
                // if farmland, check what tool was used
                string handItem = playerInventory.handItem;
                if (clickedTile) switch (handItem)
                    {
                        // if Shovel equipped, till souil
                        case "Rusty Shovel":
                        case "Bronze Shovel":
                        case "Silver Shovel":
                        case "Gold Shovel":
                            PlowField(gridPosition);
                            StartCoroutine(DefaultSoil(gridPosition));
                            break;
                        // if hoe equipped, harvest
                        case "Rusty Hoe":
                        case "Bronze Hoe":
                        case "Silver Hoe":
                        case "Gold Hoe":
                            HarvestCrop(gridPosition);
                            break;
                        // if watering can equipped, water soil for faster growth
                        case "Rusty Watering Can":
                        case "Bronze Watering Can":
                        case "Silver Watering Can":
                        case "Gold Watering Can":
                            WaterField(gridPosition);
                            break;
                        // if seeds equipped, plant corresponding seedling
                        case "Wheat Seeds":
                        case "Tomato Seeds":
                        case "Lentils Seeds":
                            AddCrop(GetCropName(handItem), gridPosition);
                            break;
                    }
            }
        }

        playerActionImage.fillAmount = CalculateTime() / 1f;
        playerActionImage.color = Color.Lerp(Color.white, Color.green, playerActionImage.fillAmount);
        timer += Time.deltaTime;
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
        if (!wheatPlants.ContainsKey(gridPosition) && !tomatoPlants.ContainsKey(gridPosition) && !lentilPlants.ContainsKey(gridPosition) && !farmPlants.HasTile(gridPosition))
        {
            SetDirtFieldState(gridPosition, (DirtFieldState)Mathf.Max(0, tileState[gridPosition] - 1));
            StartCoroutine(DefaultSoil(gridPosition));
        }
    }

    private bool CheckTimer()
    {
        return (CalculateTime() > 1f);
    }

    private float CalculateTime()
    {
        return timer + (1f * timerModifierPercentage);
    }

    public void HandleHandChange(GameObject item)
    {
        if (item == null)
        {
            itemRangeModifier = 0;
            timerModifierPercentage = 0f;
        }
        else
        {
            var name = item.GetComponent<InventoryIcon>().item;
            var result = name switch
            {
                string a when a.Contains("Rusty") => 1f,
                string b when b.Contains("Bronze") => 2f,
                string b when b.Contains("Silver") => 5f,
                string b when b.Contains("Gold") => 20f,
                _ => 0f
            };
            itemRangeModifier = result;

            var timerModifier = name switch
            {
                string a when a.Contains("Rusty") => 0f,
                string b when b.Contains("Bronze") => 0.2f,
                string b when b.Contains("Silver") => 0.5f,
                string b when b.Contains("Gold") => 1f,
                _ => baselineTimerModifierPercentage
            };
            timerModifierPercentage = timerModifier;

            var baseLineModifier = name switch
            {
                string a when a.Contains("Rusty Shovel") => 0f,
                string b when b.Contains("Bronze Shovel") => 0.2f,
                string b when b.Contains("Silver Shovel") => 0.5f,
                string b when b.Contains("Gold Shovel") => 1f,
                _ => 0
            };

            baselineTimerModifierPercentage = Mathf.Max(baselineTimerModifierPercentage, baseLineModifier);
        }
    }

    private void PlayActionSound(AudioClip clip)
    {
        float upperValue = Mathf.Lerp(1f, 0.3f, timerModifierPercentage);
        audioSource.PlayOneShot(clip, Random.Range(0.3f, upperValue));
    }

}