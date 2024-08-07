using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // player speed variable
    public float speed = 10.0f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public bool stop = false;

    [SerializeField] private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!stop)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            moveInput.Normalize();

            rb.velocity = moveInput * speed;
            if (moveInput != Vector2.zero)
            {
                animator.SetBool("isWalking", true);
                animator.SetFloat("XInput", moveInput.x);
                animator.SetFloat("YInput", moveInput.y);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
}
