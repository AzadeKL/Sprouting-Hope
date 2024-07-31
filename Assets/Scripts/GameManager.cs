using SaveSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
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
    public Transform playerCenter;
    [SerializeField] private Transform outliner;
    [SerializeField] private float itemRange = 3f;

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
    [Header("Buildings")]
    public List<TileBase> restaurant;
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
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private FloatReference money;

    //[SerializeField] private List<TileData> tileDatas;

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
    private void Awake()
    {
        if (seedFactory == null) { seedFactory = GetComponent<SeedFactory>(); }

    }

    private void ChangeSoil(Vector3Int gridPosition, int state)
    {
        // if tile is called for the first time, add to list and give appropriate state
        if (!tileState.ContainsKey(gridPosition)) tileState.Add(gridPosition, state);
        else tileState[gridPosition] = state;
    }

    public void UpdateCrops(Vector3Int gridPosition)
    {
        List<Vector3Int> keys;
        eggCount = Mathf.Min(eggCount + 5, 10);
        //buildings.transform.DOScale(Vector3.one * 1.05f, 0.3f).SetLoops(2, LoopType.Yoyo);
        string crop = "";
        if (wheatPlants.ContainsKey(gridPosition))
        {
            crop = "Wheat";
        }
        else if (tomatoPlants.ContainsKey(gridPosition))
        {
            crop = "Tomato";
        }
        else if (lentilPlants.ContainsKey(gridPosition))
        {
            crop = "Lentils";
        }
        switch (crop)
        {
            case "Wheat":
            Debug.Log("Wheat is growing!");
            wheatPlants[gridPosition]++;
            if (wheatPlants[gridPosition] > 2) wheatPlants[gridPosition] = 2;
            farmPlants.SetTile(gridPosition, wheat[wheatPlants[gridPosition]]);
            break;
            case "Tomato":
            Debug.Log("Tomatoes are growing!");
            tomatoPlants[gridPosition]++;
            if (tomatoPlants[gridPosition] > 2) tomatoPlants[gridPosition] = 2;
            farmPlants.SetTile(gridPosition, tomato[tomatoPlants[gridPosition]]);
            break;
            case "Lentils":
            Debug.Log("Lentils are growing!");
            lentilPlants[gridPosition]++;
            if (lentilPlants[gridPosition] > 2) lentilPlants[gridPosition] = 2;
            farmPlants.SetTile(gridPosition, lentil[lentilPlants[gridPosition]]);
            break;
            default:
            farmLand.SetTile(gridPosition, defaultField);
            ChangeSoil(gridPosition, 0);
            break;
        }
        if (crop != "") StartCoroutine(GrowTime(gridPosition));
    }

    private void HarvestCrop(Vector3Int gridPosition)
    {
        if (wheatPlants.ContainsKey(gridPosition) && wheatPlants[gridPosition] == 2)
        {
            farmPlants.SetTile(gridPosition, null);
            wheatPlants.Remove(gridPosition);
            seedFactory.CreateCrop(gridPosition, "Wheat");
            Debug.Log("Harvested Wheat");
        }
        else if (tomatoPlants.ContainsKey(gridPosition))
        {
            farmPlants.SetTile(gridPosition, null);
            tomatoPlants.Remove(gridPosition);
            seedFactory.CreateCrop(gridPosition, "Tomato");
            Debug.Log("Harvested Tomato");
        }
        else if (lentilPlants.ContainsKey(gridPosition))
        {
            farmPlants.SetTile(gridPosition, null);
            lentilPlants.Remove(gridPosition);
            seedFactory.CreateCrop(gridPosition, "Lentil");
            Debug.Log("Harvested Lentils");
        }
    }

    private void Update()
    {

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
        if (Input.GetKeyUp(KeyCode.F))
        {
            Vector3Int gridPosition = buildings.WorldToCell(player.transform.position);
            foreach (Vector3Int neighborPosition in neighborPositions)
            {
                if (buildings.HasTile(gridPosition + neighborPosition))
                {
                    if (restaurant.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Restaurant!");

                        inventoryUI.SetActive(true);
                        player.GetComponent<PlayerInventory>().sellMode = true;
                        inventoryUI.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Image>().color = sellInventory;

                        /*if (player.GetComponent<PlayerInventory>().inventory.TryGetValue("Egg", out int eggCount))
                        {
                            if (eggCount > 0)
                            {
                                player.GetComponent<PlayerInventory>().RemoveFromInventory("Egg");
                                money.Value += 25f;
                            }
                            else
                            {
                                audioSource.Play();
                            }
                        }
                        else
                        {
                            audioSource.Play();
                        }*/

                        break;
                    }
                    else if (chickenCoop.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Chicken Coop!");

                        chickenCoopUI.SetActive(true);
                        inventoryUI.SetActive(true);
                        /*if (eggCount > 0)
                        {
                            Instantiate(EggPrefab, playerCenter.position, Quaternion.identity);
                            eggCount--;
                        }
                        else
                        {
                            audioSource.Play();
                        }*/

                        break;
                    }
                    else if (house.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Farmhouse!");

                        houseUI.SetActive(true);
                        break;
                    }
                    else if (pigPen.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Pig Pen!");

                        pigPenUI.SetActive(true);
                        inventoryUI.SetActive(true);
                        break;
                    }
                    else if (storage.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Storage!");

                        storageUI.SetActive(true);
                        break;
                    }
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
        if (Input.GetMouseButtonUp(0) && disableTool == false)
        {
            if (!inventoryUI.activeSelf)
            {
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
                if (clickedTile) switch (player.GetComponent<PlayerInventory>().handItem)
                    {
                        // if hoe equipped, till soil
                        case "Rusty Hoe":
                        if (!tileState.ContainsKey(gridPosition) || tileState[gridPosition] < 1)
                        {
                            ChangeSoil(gridPosition, 1);
                            farmLand.SetTile(gridPosition, plowedField);
                            Debug.Log("Dirt space set to " + tileState[gridPosition] + ", tilled");
                            seedFactory.CreateSeed(player.transform.position);
                        }
                        // TEMP hoe for now harvests plants
                        else
                        {
                            HarvestCrop(gridPosition);
                        }
                        break;
                        // if hoe equipped, till soil
                        case "Bronze Hoe":
                        if (!tileState.ContainsKey(gridPosition) || tileState[gridPosition] < 1)
                        {
                            ChangeSoil(gridPosition, 1);
                            farmLand.SetTile(gridPosition, plowedField);
                            Debug.Log("Dirt space set to " + tileState[gridPosition] + ", tilled");
                            seedFactory.CreateSeed(player.transform.position);
                        }
                        // TEMP hoe for now harvests plants
                        else
                        {
                            HarvestCrop(gridPosition);
                        }
                        break;
                        // if hoe equipped, till soil
                        case "Silver Hoe":
                        if (!tileState.ContainsKey(gridPosition) || tileState[gridPosition] < 1)
                        {
                            ChangeSoil(gridPosition, 1);
                            farmLand.SetTile(gridPosition, plowedField);
                            Debug.Log("Dirt space set to " + tileState[gridPosition] + ", tilled");
                            seedFactory.CreateSeed(player.transform.position);
                        }
                        // TEMP hoe for now harvests plants
                        else
                        {
                            HarvestCrop(gridPosition);
                        }
                        break;
                        // if hoe equipped, till soil
                        case "Gold Hoe":
                        if (!tileState.ContainsKey(gridPosition) || tileState[gridPosition] < 1)
                        {
                            ChangeSoil(gridPosition, 1);
                            farmLand.SetTile(gridPosition, plowedField);
                            Debug.Log("Dirt space set to " + tileState[gridPosition] + ", tilled");
                            seedFactory.CreateSeed(player.transform.position);
                        }
                        // TEMP hoe for now harvests plants
                        else
                        {
                            HarvestCrop(gridPosition);
                        }
                        break;
                        // if watering can equipped, water soil for faster growth
                        case "Rusty Watering Can":
                        if (tileState.ContainsKey(gridPosition) && tileState[gridPosition] >= 1)
                        {
                            ChangeSoil(gridPosition, 2);
                            farmLand.SetTile(gridPosition, wateredField);
                            Debug.Log("Dirt space set to " + tileState[gridPosition] + ", watered");
                        }
                        break;
                        // if watering can equipped, water soil for faster growth
                        case "Bronze Watering Can":
                        if (tileState.ContainsKey(gridPosition) && tileState[gridPosition] >= 1)
                        {
                            ChangeSoil(gridPosition, 2);
                            farmLand.SetTile(gridPosition, wateredField);
                            Debug.Log("Dirt space set to " + tileState[gridPosition] + ", watered");
                        }
                        break;
                        // if watering can equipped, water soil for faster growth
                        case "Silver Watering Can":
                        if (tileState.ContainsKey(gridPosition) && tileState[gridPosition] >= 1)
                        {
                            ChangeSoil(gridPosition, 2);
                            farmLand.SetTile(gridPosition, wateredField);
                            Debug.Log("Dirt space set to " + tileState[gridPosition] + ", watered");
                        }
                        break;
                        // if watering can equipped, water soil for faster growth
                        case "Gold Watering Can":
                        if (tileState.ContainsKey(gridPosition) && tileState[gridPosition] >= 1)
                        {
                            ChangeSoil(gridPosition, 2);
                            farmLand.SetTile(gridPosition, wateredField);
                            Debug.Log("Dirt space set to " + tileState[gridPosition] + ", watered");
                        }
                        break;
                        // if wheat seeds equipped, plant wheat seedling
                        case "Wheat Seeds":
                        if (tileState.ContainsKey(gridPosition) && tileState[gridPosition] >= 1 && !farmPlants.HasTile(gridPosition))
                        {
                            farmPlants.SetTile(gridPosition, wheat[0]);
                            wheatPlants.Add(gridPosition, 0);
                            player.GetComponent<PlayerInventory>().RemoveFromInventory("Wheat Seeds");
                            StartCoroutine(GrowTime(gridPosition));
                            Debug.Log("Planted wheat seeds");
                        }
                        break;
                        // if tomato seeds equipped, plant tomato seedling
                        case "Tomato Seeds":
                        if (tileState.ContainsKey(gridPosition) && tileState[gridPosition] >= 1 && !farmPlants.HasTile(gridPosition))
                        {
                            farmPlants.SetTile(gridPosition, tomato[0]);
                            tomatoPlants.Add(gridPosition, 0);
                            player.GetComponent<PlayerInventory>().RemoveFromInventory("Tomato Seeds");
                            StartCoroutine(GrowTime(gridPosition));
                            Debug.Log("Planted tomato seeds");
                        }
                        break;
                        // if tomato seeds equipped, plant tomato seedling
                        case "Lentils Seeds":
                        if (tileState.ContainsKey(gridPosition) && tileState[gridPosition] >= 1 && !farmPlants.HasTile(gridPosition))
                        {
                            farmPlants.SetTile(gridPosition, lentil[0]);
                            lentilPlants.Add(gridPosition, 0);
                            player.GetComponent<PlayerInventory>().RemoveFromInventory("Lentils Seeds");
                            StartCoroutine(GrowTime(gridPosition));
                            Debug.Log("Planted lentils seeds");
                        }
                        break;
                    }
            }
        }
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

    public void HandleHandChange(GameObject item)
    {
        if (item == null)
        {
            itemRangeModifier = 0;
        }
        else
        {
            //var tagName = item.tag;

            //Debugger.Log("Tag name im HandChange " + tagName, Debugger.PriorityLevel.High);

            var name = item.GetComponent<InventoryIcon>().item;
            Debugger.Log("Name im HandChange " + name, Debugger.PriorityLevel.High);
            var result = name switch
            {
                string a when a.Contains("Rusty") => 1f,
                string b when b.Contains("Bronze") => 2f,
                string b when b.Contains("Silver") => 5f,
                string b when b.Contains("Gold") => 20f,
                _ => 0f
            };
            itemRangeModifier = result;
            Debugger.Log("Result of HandChange " + result, Debugger.PriorityLevel.High);
        }
    }

}
