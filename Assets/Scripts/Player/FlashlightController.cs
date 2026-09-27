using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [Header("Ссылки")]
    public Light flashlight;

    private PlayerInputActions inputActions;
    private bool isOn = true;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Flashlight.performed += ctx => ToggleFlashlight();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Start()
    {
        if (flashlight != null)
            flashlight.enabled = isOn;
    }

    private void ToggleFlashlight()
    {
        isOn = !isOn;
        if (flashlight != null)
            flashlight.enabled = isOn;
    }
}