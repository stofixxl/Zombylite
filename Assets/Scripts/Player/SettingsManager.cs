using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Настройки")]
    [Range(0.1f, 5f)]
    public float mouseSensitivity = 2f;

    [Range(0f, 1f)]
    public float masterVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        ApplyVolume();
    }

    public void ApplyVolume()
    {
        AudioListener.volume = masterVolume;
    }

    private void Update()
    {
        // R — сбросить на стандарт (для быстрой проверки)
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            mouseSensitivity = 2f;
            masterVolume = 1f;
            ApplyVolume();
            Debug.Log("Настройки сброшены. Чувствительность: " + mouseSensitivity +
                      ", Громкость: " + masterVolume);
        }
    }
}