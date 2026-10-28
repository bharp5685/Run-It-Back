using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // How fast the returner moves
    public float moveSpeed = 5f;

    // Limits so the returner stays on the field
    public float minX = -5.5f;
    public float maxX = 5.5f;
    public float minY = -8.5f;
    public float maxY = 8.5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        // Grab the Rigidbody2D so we can move the player
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            moveInput.y += 1f;
        }

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
        {
            moveInput.y -= 1f;
        }

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput.x -= 1f;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput.x += 1f;
        }

        // Keeps diagonal movement from being faster
        moveInput = moveInput.normalized;
    }

    void FixedUpdate()
    {
        Vector2 newPosition =
            rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        // Keep the returner inside the field
        newPosition.x = Mathf.Clamp(
            newPosition.x,
            minX,
            maxX
        );

        newPosition.y = Mathf.Clamp(
            newPosition.y,
            minY,
            maxY
        );

        rb.MovePosition(newPosition);
    }
}