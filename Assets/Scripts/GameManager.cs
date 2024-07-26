using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{

    [SerializeField] private Tilemap grassMap;
    [SerializeField] private Tilemap farmLand;
    [SerializeField] private Tilemap farmPlants;
    [SerializeField] private Tilemap buildings;

    [SerializeField] private SeedFactory seedFactory;
    public GameObject player;

    public List<Tile> wheat;
    private Dictionary<Vector3Int, int> wheatPlants = new Dictionary<Vector3Int, int>();
    public List<Tile> tomato;
    private Dictionary<Vector3Int, int> tomatoPlants = new Dictionary<Vector3Int, int>();
    public List<Tile> lentil;
    private Dictionary<Vector3Int, int> lentilPlants = new Dictionary<Vector3Int, int>();

    [Space]

    public List<TileBase> restaurant;
    public List<TileBase> house;
    public List<TileBase> silo;
    public List<TileBase> chickenCoop;


    //[SerializeField] private List<TileData> tileDatas;

    private Dictionary<Vector3Int, int> tileState = new Dictionary<Vector3Int, int>();


    private Vector3Int[] neighborPositions =
    {
        Vector3Int.up,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.down
    };


    private void Awake()
    {
        if (seedFactory == null) { seedFactory = GetComponent<SeedFactory>(); }

    }

    // function called when a tool or general change to the soil's state happens
    private void ChangeSoil(Vector3Int gridPosition, int state)
    {
        // if tile is called for the first time, add to list and give appropriate state
        if (!tileState.ContainsKey(gridPosition)) tileState.Add(gridPosition, state);
        else tileState[gridPosition] = state;
    }

    public void UpdateCrops(string crop)
    {
        List<Vector3Int> keys;
        switch (crop)
        {
            case "Wheat":
            Debug.Log("Wheat is growing!");
            keys = new List<Vector3Int>(wheatPlants.Keys);
            for (int i = 0; i < wheatPlants.Count; i++)
            {
                Vector3Int gridPosition = keys[i];
                wheatPlants[gridPosition]++;
                if (wheatPlants[gridPosition] > 2) wheatPlants[gridPosition] = 2;
                farmPlants.SetTile(gridPosition, wheat[wheatPlants[gridPosition]]);
            }
            break;
            case "Tomato":
            Debug.Log("Tomatoes are growing!");
            keys = new List<Vector3Int>(tomatoPlants.Keys);
            for (int i = 0; i < tomatoPlants.Count; i++)
            {
                Vector3Int gridPosition = keys[i];
                tomatoPlants[gridPosition]++;
                if (tomatoPlants[gridPosition] > 2) tomatoPlants[gridPosition] = 2;
                farmPlants.SetTile(gridPosition, tomato[tomatoPlants[gridPosition]]);
            }
            break;
            case "Lentils":
            Debug.Log("Lentils are growing!");
            keys = new List<Vector3Int>(lentilPlants.Keys);
            for (int i = 0; i < lentilPlants.Count; i++)
            {
                Vector3Int gridPosition = keys[i];
                lentilPlants[gridPosition]++;
                if (lentilPlants[gridPosition] > 2) lentilPlants[gridPosition] = 2;
                farmPlants.SetTile(gridPosition, lentil[lentilPlants[gridPosition]]);
            }
            break;
        }
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
                        break;
                    }
                    else if (chickenCoop.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Chicken Coop!");
                        break;
                    }
                    else if (house.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Farmhouse!");
                        break;
                    }
                    else if (silo.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Interacting with Silo!");
                        break;
                    }
                }
            }
        }



        // left click to interact with tool to tile
        if (Input.GetMouseButtonUp(0))
        {
            if (!player.GetComponent<PlayerInventory>().inventoryUI.activeSelf)
            {
                // get tile player is standing on
                //Vector3Int gridPosition = farmLand.WorldToCell(player.transform.position + ((player.GetComponent<PlayerTool>().direction.normalized) * 2.5f));
                Vector3 mousePosition = Input.mousePosition;
                mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
                mousePosition.z = 0;
                var result = mousePosition - player.transform.position;
                result.Normalize();
                Vector3Int gridPosition = farmLand.WorldToCell(player.transform.position + (result * 2.5f));
                TileBase clickedTile = farmLand.GetTile(gridPosition);
                // if farmland, check what tool was used
                if (clickedTile) switch (player.GetComponent<PlayerInventory>().inventoryIndex[player.GetComponent<PlayerInventory>().handIndex])
                    {
                        // if hoe equipped, till soil
                        case "Hoe":
                        if (!tileState.ContainsKey(gridPosition) || tileState[gridPosition] < 1)
                        {
                            ChangeSoil(gridPosition, 1);
                            farmLand.SetColor(gridPosition, new Color(0.6f, 0.4f, 0f));
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
                        case "Watering Can":
                        if (tileState.ContainsKey(gridPosition) && tileState[gridPosition] >= 1)
                        {
                            ChangeSoil(gridPosition, 2);
                            farmLand.SetColor(gridPosition, new Color(0.4f, 0.2f, 0f));
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
                            Debug.Log("Planted lentils seeds");
                        }
                        break;
                    }
            }
        }
    }




}
