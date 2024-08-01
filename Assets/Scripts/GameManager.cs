using SaveSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour, SaveSystem.ISaveable
{
    [SerializeField]
    private float farmingrange = 1f;

    [Header("Tilemap")]
    [SerializeField] private Tilemap grassMap;
    [SerializeField] private Tilemap farmLand;
    [SerializeField] private Tilemap farmPlants;
    [SerializeField] private Tilemap buildings;

    [Space]
    [SerializeField] private SeedFactory seedFactory;
    [Space]
    [Header("Player")]
    public GameObject player;
    public GameObject inventoryUI;
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
    public List<Tile> wheat;
    private Dictionary<Vector3Int, int> wheatPlants = new Dictionary<Vector3Int, int>();
    public List<Tile> tomato;
    private Dictionary<Vector3Int, int> tomatoPlants = new Dictionary<Vector3Int, int>();
    public List<Tile> lentil;
    private Dictionary<Vector3Int, int> lentilPlants = new Dictionary<Vector3Int, int>();

    [Space]
    [Header("Main Objective")]
    public int mainProgress;
    [SerializeField] private int maxProgress;
    [SerializeField] private Slider progressMeter;

    [Space]
    [Header("Buildings")]
    public List<TileBase> restaurant;
    public List<TileBase> truck;
    public List<TileBase> house;
    [SerializeField] private GameObject houseUI;
    public List<TileBase> pigPen;
    [SerializeField] private GameObject pigPenUI;
    public List<TileBase> chickenCoop;
    [SerializeField] private GameObject chickenCoopUI;
    public List<TileBase> storage;
    [SerializeField] private GameObject storageUI;

    [SerializeField] private GameObject EggPrefab;
    [SerializeField] private int eggCount = 5;




    private Dictionary<Vector3Int, int> tileState = new Dictionary<Vector3Int, int>();


    private Vector3Int[] neighborPositions =
    {
        Vector3Int.up,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.down
    };
    private bool disableTool = false;

    private float itemRangeModifier = 0f;
    private float timer = 1f;
    private float timerModifierPercentage = 0f;
    private void Awake()
    {
        if (seedFactory == null) { seedFactory = GetComponent<SeedFactory>(); }
        progressMeter.maxValue = maxProgress;

    }
    private void Start()
    {
        SaveSystem.DataManager.instance.Load(this);
    }
    public void Save(GameData gameData)
    {
        Func<Vector3Int, string> Vector3dIntToString = (v) => { return string.Join(',', v.x, v.y, v.z); };
        Func<string, KeyValuePair<Vector3Int, int>, string> PlantToEntry = (plantName, point) => string.Join(":", plantName, Vector3dIntToString(point.Key), point.Value);

        foreach (var key_value in wheatPlants) gameData.gameManagerPlants.Add(PlantToEntry("Wheat", key_value));
        foreach (var key_value in tomatoPlants) gameData.gameManagerPlants.Add(PlantToEntry("Tomato", key_value));
        foreach (var key_value in lentilPlants) gameData.gameManagerPlants.Add(PlantToEntry("Lentil", key_value));
    }
    public bool Load(GameData gameData)
    {
        Func<string, Vector3Int> Vector3dIntFromString = (str) => { var tmp = str.Split(','); return new Vector3Int(Convert.ToInt32(tmp[0]), Convert.ToInt32(tmp[1]), Convert.ToInt32(tmp[2])); };

        foreach (var entry in gameData.gameManagerPlants)
        {
            var parsed = entry.Split(':');
            string cropName = parsed[0];
            Vector3Int gridPosition = Vector3dIntFromString(parsed[1]);
            int growthState = Convert.ToInt32(parsed[2]);
            PlowField(gridPosition, false);
            AddCrop(cropName, gridPosition, growthState);
        }

        return true;
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
            case "Lentil":
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

    private void SetDefaultField(Vector3Int gridPosition)
    {
        farmLand.SetTile(gridPosition, defaultField);
        ChangeSoil(gridPosition, 0);
    }

    private void ChangeSoil(Vector3Int gridPosition, int state)
    {
        //Debug.Log("Changing soil at position: " + gridPosition + ", growth: " + state);
        // if tile is called for the first time, add to list and give appropriate state
        if (!tileState.ContainsKey(gridPosition)) tileState.Add(gridPosition, state);
        else tileState[gridPosition] = state;
    }

    private void PlowOrHarvestField(Vector3Int gridPosition)
    {
        // If the space is unset, plow it
        if (!tileState.ContainsKey(gridPosition) || tileState[gridPosition] < 1)
        {
            PlowField(gridPosition);
        }
        // If the space is set, harvest it
        else
        {
            HarvestCrop(gridPosition);
        }
    }

    private void PlowField(Vector3Int gridPosition, bool createSeeds = true)
    {
        ChangeSoil(gridPosition, 1);
        farmLand.SetTile(gridPosition, plowedField);
        Debug.Log("Dirt space set to " + tileState[gridPosition] + ", plowed");
        if (createSeeds) seedFactory.CreateSeed(player.transform.position);
    }

    private void WaterField(Vector3Int gridPosition)
    {
        if (!tileState.ContainsKey(gridPosition) || (tileState[gridPosition] < 1))
        {
            Debug.Log("Failed to water crop - grid location not initialized: " + gridPosition);
            return;
        }

        ChangeSoil(gridPosition, 2);
        farmLand.SetTile(gridPosition, wateredField);
        Debug.Log("Dirt space set to " + tileState[gridPosition] + ", watered");
    }

    private void AddCrop(string cropName, Vector3Int gridPosition, int growthState = 0)
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
            SetDefaultField(gridPosition);
            return;
        }

        farmPlants.SetTile(gridPosition, crop[growthState]);
        cropPlants.Add(gridPosition, growthState);
        string seedName = GetSeedName(cropName);
        player.GetComponent<PlayerInventory>().RemoveFromInventory(seedName);
        StartCoroutine(GrowTime(gridPosition));
        Debug.Log("Planted " + seedName);
    }

    public void UpdateCrops(Vector3Int gridPosition)
    {
        string cropName = "";
        List<Tile> crop = null;
        Dictionary<Vector3Int, int> cropPlants = null;
        eggCount = Mathf.Min(eggCount + 5, 10);
        //buildings.transform.DOScale(Vector3.one * 1.05f, 0.3f).SetLoops(2, LoopType.Yoyo);
        if (!GetCropVarsAtGridPosition(gridPosition, ref cropName, ref crop, ref cropPlants))
        {
            Debug.Log("No crop to update at position: " + gridPosition);
            SetDefaultField(gridPosition);
            return;
        }

        Debug.Log(cropName + " is growing!");
        if (cropPlants[gridPosition] < 2) ++cropPlants[gridPosition];
        farmPlants.SetTile(gridPosition, crop[cropPlants[gridPosition]]);
        StartCoroutine(GrowTime(gridPosition));
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
        cropPlants.Remove(gridPosition);
        seedFactory.CreateCrop(gridPosition, cropName);
        Debug.Log("Harvested " + cropName);
    }

    private void Update()
    {
        progressMeter.value = mainProgress;
        progressMeter.gameObject.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = mainProgress.ToString() + "/" + maxProgress.ToString();
        // if progress meets requirement, win the game (prompt to return to menu or continue playing?)
        if (mainProgress >= maxProgress)
        {
            Debug.Log("Game is WON!");
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

        if (houseUI.activeSelf || chickenCoopUI.activeSelf || inventoryUI.activeSelf || storageUI.activeSelf || pigPenUI.activeSelf) player.GetComponent<PlayerMovement>().menuUp = true;
        else player.GetComponent<PlayerMovement>().menuUp = false;

        // interact with building or other interactable object
        if (true)
        {

            var flag = Input.GetKeyUp(KeyCode.F);

            Vector3Int gridPosition = buildings.WorldToCell(player.transform.position);
            foreach (Vector3Int neighborPosition in neighborPositions)
            {
                if (buildings.HasTile(gridPosition + neighborPosition))
                {
                    if (restaurant.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Restaurant!");

                        if (flag)
                        {
                            inventoryUI.SetActive(true);
                            player.GetComponent<PlayerInventory>().sellMode = true;
                            inventoryUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = sellInventory;
                        }
                        else
                        {
                            playerWorldCanvas.SetActive(true);
                        }


                        break;
                    }
                    else if (truck.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Truck!");

                        if (flag)
                        {
                            inventoryUI.SetActive(true);
                            player.GetComponent<PlayerInventory>().giveMode = true;
                            inventoryUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = giveInventory;
                        }
                        else
                        {
                            playerWorldCanvas.SetActive(true);
                        }


                        break;
                    }
                    else if (chickenCoop.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Chicken Coop!");

                        if (flag)
                        {
                            inventoryUI.SetActive(true);
                            chickenCoopUI.SetActive(true);
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
                        Debug.Log("Interacting with Farmhouse!");

                        if (flag)
                        {
                            houseUI.SetActive(true);
                        }
                        else
                        {
                            playerWorldCanvas.SetActive(true);
                        }

                        break;
                    }
                    else if (pigPen.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Pig Pen!");

                        if (flag)
                        {
                            pigPenUI.SetActive(true);
                            inventoryUI.SetActive(true);
                        }
                        else
                        {
                            playerWorldCanvas.SetActive(true);
                        }

                        break;
                    }
                    else if (storage.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Storage!");

                        if (flag)
                        {
                            storageUI.SetActive(true);
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


        // esc key to either close existing windows or open pause menu
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (player.GetComponent<PlayerMovement>().menuUp)
            {
                inventoryUI.SetActive(false);
                pigPenUI.SetActive(false);
                houseUI.SetActive(false);
                storageUI.SetActive(false);
                chickenCoopUI.SetActive(false);
                player.GetComponent<PlayerInventory>().sellMode = false;
                inventoryUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = normalInventory;
            }
            else
            {
                // open/close pause menu here
            }
        }
        // enable/disable inventory window only
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (!player.GetComponent<PlayerMovement>().menuUp) inventoryUI.SetActive(true);
            else
            {
                CloseUIPanels();
            }
            if (!inventoryUI.activeSelf) inventoryUI.transform.parent.GetChild(4).gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (player.GetComponent<PlayerMovement>().menuUp == true)
            {
                PauseMenu.instance.notAllowed = true;
                CloseUIPanels();
                if (!inventoryUI.activeSelf) inventoryUI.transform.parent.GetChild(4).gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Invoke(nameof(EnablePauseMenu), 0.2f);

        }


        // left click to interact with tool to tile
        if (Input.GetMouseButton(0) && disableTool == false)
        {
            if (!inventoryUI.activeSelf && CheckTimer())
            {
                timer = 0f;
                // get tile player is standing on
                //Vector3Int gridPosition = farmLand.WorldToCell(player.transform.position + ((player.GetComponent<PlayerTool>().direction.normalized) * 2.5f));
                Vector3 mousePosition = Input.mousePosition;
                mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
                mousePosition.z = 0;
                var playerpos = playerCenter.position;
                playerpos.z = 0;
                var result = mousePosition - playerpos;
                result.Normalize();
                Debugger.Log(result + " mouse direction normalized", Debugger.PriorityLevel.Medium);
                //Vector3Int gridPosition = farmLand.WorldToCell(playerpos + (result * farmingrange));
                Vector3Int gridPosition = farmLand.WorldToCell(tilePos);
                TileBase clickedTile = farmLand.GetTile(gridPosition);
                // if farmland, check what tool was used
                string handItem = player.GetComponent<PlayerInventory>().handItem;
                if (clickedTile) switch (handItem)
                    {
                        // if hoe equipped, till soil
                        case "Rusty Hoe":
                        case "Bronze Hoe":
                        case "Silver Hoe":
                        case "Gold Hoe":
                            PlowOrHarvestField(gridPosition);
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

    private void CloseUIPanels()
    {
        inventoryUI.SetActive(false);
        pigPenUI.SetActive(false);
        houseUI.SetActive(false);
        storageUI.SetActive(false);
        chickenCoopUI.SetActive(false);
        player.GetComponent<PlayerInventory>().sellMode = false;
        inventoryUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = normalInventory;
    }

    IEnumerator GrowTime(Vector3Int gridPosition)
    {
        float time = 10f;
        // set grwoth time by plant type
        if (wheatPlants.ContainsKey(gridPosition)) time = 20f;
        else if (tomatoPlants.ContainsKey(gridPosition)) time = 20f;
        else if (lentilPlants.ContainsKey(gridPosition)) time = 15f;


        // if watered, half the growth time
        yield return new WaitForSeconds(time / 2);
        // otherwise wait the full cycle to grow
        if (tileState[gridPosition] != 2) yield return new WaitForSeconds(time / 2);
        ChangeSoil(gridPosition, 1);
        farmLand.SetTile(gridPosition, plowedField);
        UpdateCrops(gridPosition);
    }

    private void EnablePauseMenu()
    {
        PauseMenu.instance.notAllowed = false;
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
            //Debugger.Log("Name im HandChange " + name, Debugger.PriorityLevel.High);
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
                _ => 0f
            };
            timerModifierPercentage = timerModifier;


            //Debugger.Log("Result of HandChange " + result, Debugger.PriorityLevel.High);
        }
    }

}
