using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    private Rigidbody rb;
    private PlayerInputActions inputActions;

    private Vector2 moveInput;
    private bool jumpPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void FixedUpdate()
    {
        // Движение по горизонтали
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 velocity = move * moveSpeed;
        velocity.y = rb.linearVelocity.y; // сохраняем гравитацию

        rb.linearVelocity = velocity;

        // Прыжок
        if (jumpPressed && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}