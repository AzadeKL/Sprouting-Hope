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

    public Tile sprite;
    public List<TileBase> buildingSprites;
    public GameObject player;


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

    private void Update()
    {


        // interact with building or other interactable object
        if (Input.GetKeyUp(KeyCode.F))
        {
            Vector3Int gridPosition = buildings.WorldToCell(player.transform.position);
            foreach (var neighborPosition in neighborPositions)
            {
                if (buildings.HasTile(gridPosition + neighborPosition))
                {
                    if (buildingSprites.Contains(buildings.GetTile(gridPosition + neighborPosition)))
                    {
                        Debug.Log("Building Detected!");
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
                Vector3Int gridPosition = farmLand.WorldToCell(player.transform.position);
                TileBase clickedTile = farmLand.GetTile(gridPosition);
                // if farmland, check what tool was used
                if (clickedTile) switch (player.GetComponent<PlayerInventory>().inventoryIndex[player.GetComponent<PlayerInventory>().handIndex])
                {
                    // if hoe equipped, till soil
                    case "Hoe":
                    ChangeSoil(gridPosition, 1);
                    farmLand.SetColor(gridPosition, new Color(0.6f, 0.4f, 0f));
                    Debug.Log("Dirt space set to " + tileState[gridPosition] + ", tilled");
                    seedFactory.CreateSeed(player.transform.position);
                    break;
                    // if wheat seeds equipped, plant wheat seedling
                    case "Wheat Seeds":
                    if (tileState.ContainsKey(gridPosition) && tileState[gridPosition] >= 1 && !farmPlants.HasTile(gridPosition))
                    {
                        farmPlants.SetTile(gridPosition, sprite);
                        player.GetComponent<PlayerInventory>().RemoveFromInventory("Wheat Seeds");
                        //plantTile.GetTileData.sprite = sprite;
                        Debug.Log("Planted wheat seeds");
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
                }
            }
        }
    }




}
