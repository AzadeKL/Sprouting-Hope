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
using UnityEngine.Serialization;

public class FarmGameManager : MonoBehaviour, SaveSystem.ISaveable
{
    //[SerializeField] private float farmingrange = 1f;

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
    [SerializeField] private Tilemap farmLand;
    [SerializeField] private Tilemap farmPlants;
    [SerializeField] private Tilemap buildings;

    private static Tilemap fieldDirtMap;
    private static Tilemap fieldPlantMap;

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

    // TODO: Move Ground/Field tiles to their own class
    [Space]
    [Header("GroundTiles")]
    [SerializeField] private float fieldRevertTime = 200f;
    [SerializeField] private TileBase defaultField;
    [SerializeField] private TileBase plowedField;
    [SerializeField] private TileBase wateredField;

    private static float dirtRevertTime;

    private enum DirtState
    {
        Default,
        Plowed,
        PlowedAndWatered,
        NumStates
    }
    private static Dictionary<string, DirtState> dirtStateLabelToEnum = new Dictionary<string, DirtState> {
        { DirtState.Default.ToString(), DirtState.Default },
        { DirtState.Plowed.ToString(), DirtState.Plowed },
        { DirtState.PlowedAndWatered.ToString(), DirtState.PlowedAndWatered } };
    private static Dictionary<DirtState, TileBase> dirtTiles = new Dictionary<DirtState, TileBase>();

    [Space]
    [Header("Plants")]
    [SerializeField] private float wateringTimeReduction;
    private static float waterMult;

    [SerializeField] private float wheatGrowTime;
    [FormerlySerializedAs("wheat")]
    public List<Tile> wheatGrowthLevels;
    [SerializeField] private float tomatoGrowTime;
    [FormerlySerializedAs("tomato")]
    public List<Tile> tomatoGrowthLevels;
    [SerializeField] private float lentilGrowTime;
    [FormerlySerializedAs("lentil")]
    public List<Tile> lentilGrowthLevels;

    private enum CropType
    {
        Wheat,
        Tomato,
        Lentil,
        NumCropTypes
    }
    private static Dictionary<string, CropType> cropTypeLabelToEnum = new Dictionary<string, CropType> { 
        { CropType.Wheat.ToString(), CropType.Wheat },
        { CropType.Tomato.ToString(), CropType.Tomato },
        { CropType.Lentil.ToString(), CropType.Lentil } };

    private class Crop
    {
        public CropType type;
        public string name;
        public float growTime;
        public List<Tile> growthLevelTiles;

        public Crop(CropType type, string name, float growTime, List<Tile> growthLevelTiles)
        {
            this.type = type;
            this.name = name;
            this.growTime = growTime;
            this.growthLevelTiles = growthLevelTiles;
        }
    }
    private static Dictionary<CropType, Crop> crops;

    private class CropPlant {
        public Crop crop;
        public int growthLevel;

        public CropPlant(string cropName, int growthLevel)
        {
            this.crop = crops[cropTypeLabelToEnum[cropName]];
            this.growthLevel = growthLevel;
        }

        // Constructor, for use by Load
        public CropPlant(string attribs)
        {
            FromAttribString(attribs);
        }

        // Attribute conversion, for use by Save
        public string ToAttribString()
        {
            return string.Join(":", crop.name.ToString(), growthLevel);
        }

        // Attribute conversion, for use by Load
        public CropPlant FromAttribString(string attribs)
        {
            var parsed = attribs.Split(':');
            this.crop = crops[cropTypeLabelToEnum[parsed[0]]];
            this.growthLevel = Convert.ToInt32(parsed[1]);
            return this;
        }
    }

    class Field
    {
        public Vector3Int gridPosition;
        public DirtState dirtState = DirtState.Default;
        public float startTime = 0f;
        public float endTime = 0f;

        private CropPlant cropPlant = null;

        public Field(Vector3Int gridPosition, DirtState dirtState, float startTime)
        {
            this.gridPosition = gridPosition;
            this.dirtState = dirtState;
            UpdateStartTime(startTime);
            this.cropPlant = null;
            UpdateTiles();
        }

        // Constructor, for use by Load
        public Field(string attribs)
        {
            FromAttribString(attribs);
        }

        // Attribute conversion, for use by Save
        public string ToAttribString()
        {
            string attribs = string.Join(":", ISaveable.Vector3IntToString(gridPosition), dirtState, startTime, endTime);
            if (cropPlant != null)
            {
                attribs = string.Join(";", attribs, cropPlant.ToAttribString());
            }
            return attribs;
        }

        // Attribute conversion, for use by Load
        public void FromAttribString(string attribs)
        {
            var parsed = attribs.Split(';');
            var fieldAttribs = parsed[0].Split(":");
            this.gridPosition = ISaveable.Vector3IntFromString(fieldAttribs[0]);
            this.dirtState = dirtStateLabelToEnum[fieldAttribs[1]];
            this.startTime = (float)Convert.ToDouble(fieldAttribs[2]);
            this.endTime = (float)Convert.ToDouble(fieldAttribs[3]);
            if (parsed.Length > 1)
            {
                this.cropPlant = new CropPlant(parsed[1]);
            }
            else
            {
                this.cropPlant = null;
            }
            UpdateTiles();
        }

        public string GetCropName()
        {
            return (cropPlant != null) ? cropPlant.crop.name : null;
        }

        public bool HasGrowingCrop()
        {
            return (cropPlant != null) && (cropPlant.growthLevel < (cropPlant.crop.growthLevelTiles.Count - 1));
        }

        public bool HasGrownCrop()
        {
            return (cropPlant != null) && (cropPlant.growthLevel == (cropPlant.crop.growthLevelTiles.Count - 1));
        }

        private bool IsWatered()
        {
            return this.dirtState == DirtState.PlowedAndWatered;
        }

        private float GetTimeInc()
        {
            if (HasGrowingCrop())
            {
                if (IsWatered())
                {
                    return cropPlant.crop.growTime * waterMult;
                }
                else
                {
                    return cropPlant.crop.growTime;
                }
            }

            if (this.dirtState > DirtState.Default)
            {
                return dirtRevertTime;
            }

            return 0f;
        }

        private void UpdateStartTime(float startTime)
        {
            this.startTime = startTime;
            this.endTime = startTime + GetTimeInc();
        }

        private void WaterCrop()
        {
            if (!HasGrowingCrop()) return;
            this.endTime = this.startTime + (this.endTime - this.startTime) * waterMult;
        }

        public bool SetDirtState(DirtState dirtState, float startTime)
        {
            if (dirtState == this.dirtState)
            {
                Debug.Log("Failed to change dirt state - already at state: " + dirtState);
                return false;
            }

            switch (dirtState)
            {
                case DirtState.Default:
                    break;
                case DirtState.Plowed:
                    break;
                case DirtState.PlowedAndWatered:
                    if (this.dirtState < DirtState.Plowed)
                    {
                        Debug.Log("Failed to change dirt state - field not plowed - must be plowed before watered");
                        return false;
                    }
                    WaterCrop();
                    break;
                default:
                    Debug.Log("Failed to change dirt state - unrecognized state: " + dirtState);
                    return false;
            }
            this.dirtState = dirtState;
            UpdateTiles();

            if (!HasGrowingCrop())
            {
                UpdateStartTime(startTime);
            }

            return true;
        }

        public void UpdateState(float time)
        {
            if (time < this.endTime) return;
            // Check if nothing to update
            if ((this.dirtState == DirtState.Default) && !HasGrowingCrop()) return;

            // Dirt states revert
            switch (this.dirtState)
            {
                case DirtState.Default:
                    break;
                case DirtState.Plowed:
                    if (cropPlant == null)
                    {
                        this.dirtState = DirtState.Default;
                    }
                    break;
                case DirtState.PlowedAndWatered:
                    this.dirtState = DirtState.Plowed;
                    break;
            }

            // Crops grow
            if (HasGrowingCrop())
            {
                ++cropPlant.growthLevel;
            }

            UpdateTiles();

            UpdateStartTime(endTime);
        }

        public bool PlantCrop(string cropName, float startTime)
        {
            if (this.dirtState < DirtState.Plowed)
            {
                Debug.Log("Failed to add crop - field not plowed");
                return false;
            }
            if (this.cropPlant != null)
            {
                Debug.Log("Failed to add crop - field already contains crop");
                return false;
            }
            cropPlant = new CropPlant(cropName, 0);
            UpdateTiles();

            UpdateStartTime(startTime);

            return true;
        }

        public bool HarvestCrop(float startTime)
        {
            if (!HasGrownCrop()) return false;

            cropPlant = null;
            UpdateTiles();

            UpdateStartTime(startTime);

            return true;
        }

        private void UpdateTiles()
        {
            // Update the dirt tile
            fieldDirtMap.SetTile(gridPosition, dirtTiles[dirtState]);

            // Update the crop tile
            if (cropPlant != null)
            { 
                fieldPlantMap.SetTile(gridPosition, cropPlant.crop.growthLevelTiles[cropPlant.growthLevel]);
            }
            else
            {
                fieldPlantMap.SetTile(gridPosition, null);
            }
        }
    }
    private Dictionary<Vector3Int, Field> fields = new Dictionary<Vector3Int, Field>();

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

        waterMult = wateringTimeReduction;
        dirtRevertTime = fieldRevertTime / timePerTick;

        fieldDirtMap = farmLand;
        fieldPlantMap = farmPlants;

        dirtTiles = new Dictionary<DirtState, TileBase>();
        dirtTiles.Add(DirtState.Default, defaultField);
        dirtTiles.Add(DirtState.Plowed, plowedField);
        dirtTiles.Add(DirtState.PlowedAndWatered, wateredField);

        crops = new Dictionary<CropType, Crop>();
        crops.Add(CropType.Wheat, new Crop(CropType.Wheat, "Wheat", wheatGrowTime / timePerTick, wheatGrowthLevels));
        crops.Add(CropType.Tomato, new Crop(CropType.Tomato, "Tomato", tomatoGrowTime / timePerTick, tomatoGrowthLevels));
        crops.Add(CropType.Lentil, new Crop(CropType.Lentil, "Lentil", lentilGrowTime / timePerTick, lentilGrowthLevels));
    }
    
    private void Start()
    {
        dayNightCycle = GameObject.Find("Global Light 2D").GetComponent<DayNightCycle>();
        playerInventory = player.GetComponent<PlayerInventory>();
        toolTip = FindObjectOfType<Tooltip>(true).gameObject;
        //Debug.Log("Current showHelpOnNewGameKey is set to: " + PlayerPrefs.GetInt(showHelpOnNewGameKey, 2));
        animalManager = GetComponent<AnimalManager>();
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
        Debug.Log("Saving FarmGameManager");

        gameData.gameManagerMainProgress = mainProgress;

        gameData.farmSaveTime = time;
        gameData.farmSaveDay = day;

        gameData.farmGameManagerFields = new List<string>();
        foreach (var field in fields)
        {
            ISaveable.AddKey(gameData.farmGameManagerFields, field.Key, field.Value.ToAttribString());
        }
    }

    public bool Load(GameData gameData)
    {
        Debug.Log("Loading FarmGameManager");

        mainProgress = gameData.gameManagerMainProgress;

        time = gameData.farmSaveTime;
        day = gameData.farmSaveDay;

        Debug.Log("Loading fields");
        fields = new Dictionary<Vector3Int, Field>();
        foreach (var key_value in gameData.farmGameManagerFields)
        {
            var entry = ISaveable.ParseKey(key_value);
            var gridPosition = ISaveable.Vector3IntFromString(entry[0]);
            var field = new Field(entry[1]);
            fields.Add(gridPosition, field);
            StartCoroutine(UpdateField(field));
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

    private static bool IsDirtState(int state)
    {
        return (state >= 0) && (state < (int) DirtState.NumStates);
    }

    private static string GetSeedName(string cropName)
    {
        return cropName + " Seeds";
    }

    private static string GetCropName(string seedName)
    {
        return seedName.Substring(0, seedName.IndexOf(' '));
    }

    private void PlowField(Vector3Int gridPosition)
    {
        if (!fields.ContainsKey(gridPosition))
        {
            var field = new Field(gridPosition, DirtState.Plowed, time);
            fields.Add(gridPosition, field);
            // Plowed is the first state. Monitor and update the state, until the field reverts to its default.
            StartCoroutine(UpdateField(field));
        }
        else if (!fields[gridPosition].SetDirtState(DirtState.Plowed, time))
        {
            Debug.Log("Failed to plow field at grid location: " + gridPosition);
            return;
        }

        seedFactory.CreateSeed(gridPosition);
        PlayActionSound(plowSounds[Random.Range(0, plowSounds.Count)]);
        Debug.Log("Plowed field at " + gridPosition + " at time " + time);
    }

    private void WaterField(Vector3Int gridPosition)
    {
        if (!fields.ContainsKey(gridPosition))
        {
            Debug.Log("Failed to water field - grid location not initialized: " + gridPosition);
            return;
        }
        if (!fields[gridPosition].SetDirtState(DirtState.PlowedAndWatered, time))
        {
            Debug.Log("Failed to water field grid location: " + gridPosition);
            return;
        }

        PlayActionSound(waterSounds[Random.Range(0, waterSounds.Count)]);
        Instantiate(waterPrefab, gridPosition, Quaternion.identity);
        Debug.Log("Watered field at " + gridPosition + " at time " + time);
    }

    private void PlantCrop(Vector3Int gridPosition, string cropName)
    {
        if (!fields.ContainsKey(gridPosition))
        {
            Debug.Log("Failed to plant crop - grid location not initialized: " + gridPosition);
            return;
        }
        if (!fields[gridPosition].PlantCrop(cropName, time))
        {
            Debug.Log("Failed to plant crop at grid location: " + gridPosition);
            return;
        }

        string seedName = GetSeedName(cropName);
        playerInventory.RemoveFromInventory(seedName);
        Debug.Log("Planted " + seedName + " at " + gridPosition + " at time " + time);
    }

    private void HarvestCrop(Vector3Int gridPosition)
    {
        if (!fields.ContainsKey(gridPosition))
        {
            Debug.Log("Failed to harvest crop - grid location not initialized: " + gridPosition);
            return;
        }
        var field = fields[gridPosition];
        var cropName = field.GetCropName();
        if (cropName == null)
        {
            Debug.Log("Failed to harvest crop - grid location has no crop: " + gridPosition);
        }
        if (field.HasGrowingCrop())
        {
            Debug.Log("Crop(" + cropName + " is too young to harvest at position: " + gridPosition);
            return;
        }
        if (!field.HarvestCrop(time))
        {
            Debug.Log("Failed to harvest crop at grid location: " + gridPosition);
            return;
        }

        seedFactory.CreateCrop(gridPosition, cropName);
        PlayActionSound(reapSounds[Random.Range(0, reapSounds.Count)]);
        Debug.Log("Harvested " + cropName + " at " + gridPosition + " at time " + time);
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

        int last_day = day;
        time = dayNightCycle.GetTime();
        day = dayNightCycle.GetDay();
        for (int i = last_day; i < day; ++i) NewDay();

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
                        	PlantCrop(gridPosition, GetCropName(handItem));
                        	break;
                    }
            }
        }

        playerActionImage.fillAmount = CalculateTime() / 1f;
        playerActionImage.color = Color.Lerp(Color.white, Color.green, playerActionImage.fillAmount);
        timer += Time.deltaTime;
    }

    IEnumerator UpdateField(Field field)
    {
        while (field.dirtState != DirtState.Default)

        {
            Debug.Log("Updating field at grid position: " + field.gridPosition + " - waiting for time to pass...");
            yield return new WaitUntil(() => time > field.endTime);
            field.UpdateState(time);
        }
        Debug.Log("Reverted field at grid positionL " + field.gridPosition);
        fields.Remove(field.gridPosition);
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

    private void NewDay()
    {
        animalManager.UpdateAnimals(playerInventory);
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