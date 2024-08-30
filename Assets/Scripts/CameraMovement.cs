using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraMovement : MonoBehaviour
{
    [Header("Player to Follow")]
    [SerializeField] GameObject player;

    [Space]
    [Header("Distance from center allowed")]
    // distance from the center of the screen [X] to allow camera to follow player
    //[SerializeField] float ClampX = 5.5f;

    // distance from the center of the screen [Y] to allow camera to follow player
    //[SerializeField] float ClampY = 3.5f;


    [SerializeField] private Tilemap boundsMap;

    private Vector2 player_prev_position = Vector2.zero;
    private Vector2 worldMin;
    private Vector2 worldMax;


    void Awake()
    {
        // get map boundaries (bottom left, top right)
        worldMin = boundsMap.transform.TransformPoint(boundsMap.localBounds.min);
        worldMax = boundsMap.transform.TransformPoint(boundsMap.localBounds.max);
        Debug.Log("Min: " + worldMin);
        Debug.Log("Max: " + worldMax);
    }
    // Update is called once per frame
    void Update()
    {
        // camera set to follow player
        /*if ((Mathf.Abs(player.transform.position.x - transform.position.x) > ClampX || Mathf.Abs(player.transform.position.y - transform.position.y) > ClampY))
        {
            Vector2 delta_position;
            delta_position = (Vector2) player.transform.position - player_prev_position;
            transform.Translate(delta_position);*/
        transform.position = new Vector3(Mathf.Clamp(player.transform.position.x, worldMin.x + 9, worldMax.x - 20), Mathf.Clamp(player.transform.position.y, worldMin.y + 5, worldMax.y - 5), transform.position.z);
        //}
        player_prev_position = player.transform.position;
    }
}
