using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Player to Follow")]
    [SerializeField]
    GameObject player;

    [Space]
    [Header("Distance from center allowed")]
    // distance from the center of the screen [X] to allow camera to follow player
    [SerializeField]
    float ClampX = 5.5f;

    // distance from the center of the screen [Y] to allow camera to follow player
    [SerializeField]
    float ClampY = 3.5f;

    private Vector2 player_prev_position = Vector2.zero;


    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(player.transform.position.x - transform.position.x) > ClampX || Mathf.Abs(player.transform.position.y - transform.position.y) > ClampY)
        {
            Vector2 delta_position;
            delta_position = (Vector2) player.transform.position - player_prev_position;
            transform.Translate(delta_position);
        }
        player_prev_position = player.transform.position;
    }
}
