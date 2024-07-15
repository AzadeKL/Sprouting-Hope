using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // player speed variable
    public float speed = 10.0f;


    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
        Vector2 right = new Vector2 (1,0);
        transform.Translate(right * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
        Vector2 left = new Vector2 (-1,0);
        transform.Translate(left * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
        Vector2 right = new Vector2 (0,1);
        transform.Translate(right * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
        Vector2 left = new Vector2 (0,-1);
        transform.Translate(left * speed * Time.deltaTime);
        }
    }
}
