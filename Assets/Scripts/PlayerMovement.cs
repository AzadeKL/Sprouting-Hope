using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // player speed variable
    public float speed = 10.0f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public bool menuUp = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!menuUp)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            moveInput.Normalize();

            rb.velocity = moveInput * speed;
        }


        
        /*if (Input.GetKey(KeyCode.D))
        {
        Vector2 right = new Vector2 (1,0);
        rb.velocity(right * speed);
        
        }
        if (Input.GetKey(KeyCode.A))
        {
        Vector2 left = new Vector2 (-1,0);
        rb.velocity(right * speed);
        }
        if (Input.GetKey(KeyCode.W))
        {
        Vector2 right = new Vector2 (0,1);
        rb.velocity(right * speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
        Vector2 left = new Vector2 (0,-1);
        rb.velocity(right * speed);
        }*/
    }
}
