using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        ThirdPersonShoulder,
        HighAngle,
        FreeLook
    }

    [Header("Настройки")]
    public CameraMode currentMode = CameraMode.FirstPerson;

    [Header("Ссылки")]
    public Transform player;
    public Transform cameraPivot;
    public Camera mainCamera;

    [Header("Чувствительность")]
    public float mouseSensitivity = 2f;

    [Header("Ограничение взгляда вверх/вниз")]
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Третье лицо (Shoulder)")]
    public Vector3 shoulderOffset = new Vector3(0.6f, 1.6f, -2.5f);

    [Header("Высокая камера (High Angle)")]
    public Vector3 highAngleOffset = new Vector3(0f, 8f, -6f);
    public float highAngleLookDown = 40f;

    [Header("Первое лицо")]
    public Vector3 firstPersonOffset = new Vector3(0f, 0.6f, 0f);
    public Vector3 crouchFirstPersonOffset = new Vector3(0f, 0.05f, 0f);
    public Vector3 proneFirstPersonOffset = new Vector3(0f, -0.3f, 0f);

    [Header("Камера при приседании")]
    public float crouchThirdPersonCameraDrop = 0.5f;
    public float proneThirdPersonCameraDrop = 0.85f;
    public float cameraPositionSpeed = 5f;

    private float yaw;
    private float pitch;

    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;

    private void Start()
    {
        if (player == null)
            player = transform.root;

        playerHealth = player.GetComponent<PlayerHealth>();
        playerMovement = player.GetComponent<PlayerMovement>();

        yaw = player.eulerAngles.y;
        LockCursor();
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.vKey.wasPressedThisFrame)
                NextMode();

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                UnlockCursor();
        }

        if (!UIInputBlock.IsUIOpen &&
            Cursor.lockState != CursorLockMode.Locked &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            LockCursor();
        }
    }

    private void LateUpdate()
    {
        HandleMouseLook();
        UpdateCameraPosition();
    }

    private void HandleMouseLook()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (Cursor.lockState != CursorLockMode.Locked ||
            Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        if (SettingsManager.Instance != null)
            mouseSensitivity = SettingsManager.Instance.mouseSensitivity;
        yaw += mouseDelta.x * mouseSensitivity * 0.1f;
        pitch -= mouseDelta.y * mouseSensitivity * 0.1f;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        player.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void UpdateCameraPosition()
    {
        bool crouching = playerMovement != null &&
                 playerMovement.IsCrouching;

        bool prone = playerMovement != null &&
                     playerMovement.IsProne;

        Vector3 targetPivotPosition;

        switch (currentMode)
        {
            case CameraMode.FirstPerson:
                targetPivotPosition = prone
                    ? proneFirstPersonOffset
                    : (crouching ? crouchFirstPersonOffset : firstPersonOffset);

                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                mainCamera.transform.localPosition = Vector3.zero;
                mainCamera.transform.localRotation = Quaternion.identity;
                break;

            case CameraMode.ThirdPersonShoulder:
            case CameraMode.FreeLook:
                targetPivotPosition = prone
                    ? Vector3.down * proneThirdPersonCameraDrop
                    : (crouching ? Vector3.down * crouchThirdPersonCameraDrop : Vector3.zero);

                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                mainCamera.transform.localPosition = shoulderOffset;
                mainCamera.transform.localRotation = Quaternion.identity;
                break;

            case CameraMode.HighAngle:
                targetPivotPosition = Vector3.zero;

                cameraPivot.localRotation =
                    Quaternion.Euler(highAngleLookDown, 0f, 0f);

                mainCamera.transform.localPosition = highAngleOffset;
                mainCamera.transform.localRotation = Quaternion.identity;
                break;

            default:
                return;
        }

        cameraPivot.localPosition = Vector3.MoveTowards(
            cameraPivot.localPosition,
            targetPivotPosition,
            Mathf.Max(0f, cameraPositionSpeed) * Time.deltaTime
        );
    }

    private void NextMode()
    {
        int count = System.Enum.GetValues(typeof(CameraMode)).Length;
        currentMode = (CameraMode)(((int)currentMode + 1) % count);

        Debug.Log("Режим камеры: " + currentMode);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}