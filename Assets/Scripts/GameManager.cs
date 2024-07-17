using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    
    [SerializeField] private Tilemap map;
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
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int gridPosition = map.WorldToCell(player.transform.position);
            TileBase clickedTile = map.GetTile(gridPosition);
            if (clickedTile)
            {
                tileState[clickedTile]++;
                int newState = tileState[clickedTile];
                Debug.Log("Dirt space set to " + newState);
            }
        }
    }

}
