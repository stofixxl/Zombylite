using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки")]
    public float interactRange = 2.5f;
    public LayerMask interactLayer;

    private PlayerInputActions inputActions;
    private Camera playerCamera;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        playerCamera = Camera.main;
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        // После смерти нельзя открывать двери, обыскивать шкафы
        // и подбирать предметы.
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            TryInteract();
    }

    private void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
                interactable.Interact();
        }
    }
}