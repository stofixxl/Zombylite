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
    public CameraMode currentMode = CameraMode.ThirdPersonShoulder;

    [Header("Ссылки")]
    public Transform player;          // Сам игрок
    public Transform cameraPivot;     // CameraPivot
    public Camera mainCamera;         // Main Camera

    [Header("Чувствительность")]
    public float mouseSensitivity = 2f;

    [Header("Третье лицо (Shoulder)")]
    public Vector3 shoulderOffset = new Vector3(0.6f, 1.6f, -2.5f);

    [Header("Высокая камера (High Angle)")]
    public Vector3 highAngleOffset = new Vector3(0f, 8f, -6f);
    public float highAngleLookDown = 40f;

    [Header("Первое лицо")]
    public Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0f);

    private float yaw;   // Горизонтальный поворот
    private float pitch; // Вертикальный поворот

    private void Start()
    {
        // Скрываем и блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (player == null)
            player = transform.root;
    }

    private void LateUpdate()
    {
        HandleMouseLook();
        UpdateCameraPosition();
    }

    private void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * mouseSensitivity * 0.1f;
        pitch -= mouseDelta.y * mouseSensitivity * 0.1f;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        // Поворачиваем игрока только по горизонтали
        player.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void UpdateCameraPosition()
    {
        switch (currentMode)
        {
            case CameraMode.FirstPerson:
                cameraPivot.localPosition = firstPersonOffset;
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                mainCamera.transform.localPosition = Vector3.zero;
                mainCamera.transform.localRotation = Quaternion.identity;
                break;

            case CameraMode.ThirdPersonShoulder:
                cameraPivot.localPosition = Vector3.zero;
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                mainCamera.transform.localPosition = shoulderOffset;
                mainCamera.transform.localRotation = Quaternion.identity;
                break;

            case CameraMode.HighAngle:
                cameraPivot.localPosition = Vector3.zero;
                cameraPivot.localRotation = Quaternion.Euler(highAngleLookDown, 0f, 0f);
                mainCamera.transform.localPosition = highAngleOffset;
                mainCamera.transform.localRotation = Quaternion.identity;
                break;

            case CameraMode.FreeLook:
                cameraPivot.localPosition = Vector3.zero;
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                mainCamera.transform.localPosition = shoulderOffset;
                mainCamera.transform.localRotation = Quaternion.identity;
                break;
        }
    }
}