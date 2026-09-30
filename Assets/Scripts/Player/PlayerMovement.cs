using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    public enum PostureState
    {
        Standing,
        Crouching,
        Prone
    }

    [Header("Скорость по состояниям")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float proneSpeed = 1f;

    [Header("Высота по состояниям (доля от роста)")]
    [Range(0.4f, 1f)]
    [SerializeField] private float crouchHeightMultiplier = 0.6f;

    [Range(0.15f, 0.6f)]
    [SerializeField] private float proneHeightMultiplier = 0.3f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerHealth playerHealth;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;

    private float standingHeight;
    private Vector3 standingCenter;

    public PostureState CurrentPosture { get; private set; } = PostureState.Standing;
    public bool IsCrouching => CurrentPosture == PostureState.Crouching;
    public bool IsProne => CurrentPosture == PostureState.Prone;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        playerHealth = GetComponent<PlayerHealth>();
        inputActions = new PlayerInputActions();

        standingHeight = capsule.height;
        standingCenter = capsule.center;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Disable();
        moveInput = Vector2.zero;
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        bool crouchHeld = Keyboard.current != null &&
            (Keyboard.current.leftCtrlKey.isPressed ||
             Keyboard.current.rightCtrlKey.isPressed);

        bool proneHeld = Keyboard.current != null &&
            Keyboard.current.zKey.isPressed;

        PostureState desired = proneHeld
            ? PostureState.Prone
            : (crouchHeld ? PostureState.Crouching : PostureState.Standing);

        if (desired != CurrentPosture)
            TrySetPosture(desired);
    }

    private void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.IsDead)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 move = transform.right * moveInput.x +
                       transform.forward * moveInput.y;

        move.y = 0f;
        move.Normalize();

        float speed = CurrentPosture switch
        {
            PostureState.Crouching => crouchSpeed,
            PostureState.Prone => proneSpeed,
            _ => moveSpeed
        };

        float injuryMul = (playerHealth != null) ? playerHealth.MoveSpeedMultiplier : 1f;
        speed *= injuryMul;
        Vector3 velocity = move * speed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private (float height, Vector3 center) GetPostureDimensions(PostureState state)
    {
        switch (state)
        {
            case PostureState.Crouching:
                {
                    float height = Mathf.Max(capsule.radius * 2f, standingHeight * crouchHeightMultiplier);
                    float diff = standingHeight - height;
                    return (height, standingCenter - Vector3.up * (diff * 0.5f));
                }
            case PostureState.Prone:
                {
                    float height = Mathf.Max(capsule.radius * 2f, standingHeight * proneHeightMultiplier);
                    float diff = standingHeight - height;
                    return (height, standingCenter - Vector3.up * (diff * 0.5f));
                }
            default:
                return (standingHeight, standingCenter);
        }
    }

    private void TrySetPosture(PostureState desired)
    {
        var (targetHeight, targetCenter) = GetPostureDimensions(desired);

        // Если новое состояние ниже текущего — места всегда достаточно.
        if (!CanExpandTo(targetHeight, targetCenter))
            return;

        capsule.height = targetHeight;
        capsule.center = targetCenter;
        CurrentPosture = desired;
    }

    // Проверяет, хватит ли места над игроком, чтобы перейти
    // из текущей высоты в целевую (более высокую).
    private bool CanExpandTo(float targetHeight, Vector3 targetCenter)
    {
        float currentTop = capsule.center.y + capsule.height * 0.5f;
        float targetTop = targetCenter.y + targetHeight * 0.5f;

        // Переход в более низкое состояние — препятствий не бывает.
        if (targetTop <= currentTop + 0.001f)
            return true;

        float radius = Mathf.Max(0.01f, capsule.radius * 0.95f);

        Vector3 bottomLocal = new Vector3(capsule.center.x, currentTop - radius, capsule.center.z);
        Vector3 topLocal = new Vector3(targetCenter.x, targetTop - radius, targetCenter.z);

        Vector3 bottomWorld = transform.TransformPoint(bottomLocal);
        Vector3 topWorld = transform.TransformPoint(topLocal);

        Collider[] overlaps = Physics.OverlapCapsule(
            bottomWorld,
            topWorld,
            radius,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider other in overlaps)
        {
            if (other == capsule || other.transform.IsChildOf(transform))
                continue;

            return false;
        }

        return true;
    }
}