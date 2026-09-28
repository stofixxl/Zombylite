using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки")]
    public float interactRange = 2.5f;          // Дистанция взаимодействия
    public LayerMask interactLayer;             // Пока можно оставить Everything

    private PlayerInputActions inputActions;
    private Camera playerCamera;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        playerCamera = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        // Пока используем Flashlight как временную кнопку E (потом сделаем отдельную)
        // Лучше добавим отдельное действие Interact
    }

    private void Update()
    {
        // Временно используем клавишу E напрямую (пока не добавили Interact в Input Actions)
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            // Сначала ищем на самом объекте
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            // Если не нашли — ищем на родителях
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}