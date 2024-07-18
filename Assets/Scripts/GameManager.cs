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
    public GameObject player;


    [SerializeField] private List<TileData> tileDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;
    private Dictionary<TileBase, int> tileState;


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
        if (Input.GetKey(KeyCode.E))
        {
            player.GetComponent<PlayerInventory>().hand = "hoe";
            Debug.Log("Now equipped " + player.GetComponent<PlayerInventory>().hand);
        }
        if (Input.GetKey(KeyCode.F))
        {
            player.GetComponent<PlayerInventory>().hand = "wheat seeds";
            Debug.Log("Now equipped " + player.GetComponent<PlayerInventory>().hand);
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int gridPosition = farmLand.WorldToCell(player.transform.position);
            TileBase clickedTile = farmLand.GetTile(gridPosition);
            if (clickedTile) switch(player.GetComponent<PlayerInventory>().hand)
            {
                case "hoe":
                    tileState[clickedTile] = 1;
                    Debug.Log("Dirt space set to " +  tileState[clickedTile]);
                    break;
                case "wheat seeds":
                    if (tileState[clickedTile] == 1)
                    {
                        farmPlants.SetTile(gridPosition, sprite);
                        //plantTile.GetTileData.sprite = sprite;
                        Debug.Log("Planted wheat seeds");
                    }
                    break;
            }
            
        }
    }

}
