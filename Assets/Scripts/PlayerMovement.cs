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
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
}
