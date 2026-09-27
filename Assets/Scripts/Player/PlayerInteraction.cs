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
            // Проверяем, есть ли на объекте скрипт Door
            Door door = hit.collider.GetComponent<Door>();
            if (door != null)
            {
                door.ToggleDoor();
                Debug.Log("Дверь переключена");
            }
        }
    }
}