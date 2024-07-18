using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    
    [SerializeField] private Tilemap farmLand;
    [SerializeField] private Tilemap farmPlants;
    [SerializeField] private Tilemap buildings;

    public Tile sprite;
    public List<TileBase> buildingSprites;
    public GameObject player;


    [SerializeField] private List<TileData> tileDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;
    private Dictionary<TileBase, int> tileState;

    
    private Vector3Int[] neighborPositions =
    {
        Vector3Int.up,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.down
    };


    private void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();
        tileState = new Dictionary<TileBase, int>();

        foreach (var tileData in tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
                tileState.Add(tile, tileData.dirtState);
            }
        }
    }

    private void Update()
    {
        /*
        // TEMP equip hoe
        if (Input.GetKeyUp(KeyCode.E))
        {
            player.GetComponent<PlayerInventory>().hand = "hoe";
            Debug.Log("Now equipped " + player.GetComponent<PlayerInventory>().hand);
        }
        // TEMP equip wheat seeds
        if (Input.GetKeyUp(KeyCode.R))
        {
            player.GetComponent<PlayerInventory>().hand = "wheat seeds";
            Debug.Log("Now equipped " + player.GetComponent<PlayerInventory>().hand);
        }
        // TEMP equip watering can
        if (Input.GetKeyUp(KeyCode.Q))
        {
            player.GetComponent<PlayerInventory>().hand = "watering can";
            Debug.Log("Now equipped " + player.GetComponent<PlayerInventory>().hand);
        }*/



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
            // get tile player is standing on
            Vector3Int gridPosition = farmLand.WorldToCell(player.transform.position);
            TileBase clickedTile = farmLand.GetTile(gridPosition);
            // if farmland, check what tool was used
            if (clickedTile) switch(player.GetComponent<PlayerInventory>().hotBar[player.GetComponent<PlayerInventory>().handIndex])
            {
                // if hoe equipped, till soil
                case "hoe":
                    tileState[clickedTile] = 1;
                    farmLand.SetColor(gridPosition, new Color(0.6f, 0.4f, 0f));
                    Debug.Log("Dirt space set to " +  tileState[clickedTile] + ", tilled");
                    break;
                // if wheat seeds equipped plant seedling
                case "wheat seeds":
                    if (tileState[clickedTile] >= 1)
                    {
                        farmPlants.SetTile(gridPosition, sprite);
                        //plantTile.GetTileData.sprite = sprite;
                        Debug.Log("Planted wheat seeds");
                    }
                    break;
                case "watering can":
                    if (tileState[clickedTile] >= 1)
                    {
                        tileState[clickedTile] = 2;
                        farmLand.SetColor(gridPosition, new Color(0.4f, 0.2f, 0f));
                        Debug.Log("Dirt space set to " +  tileState[clickedTile] + ", watered");
                    }
                    break;
            }
            
        }
    }

}
