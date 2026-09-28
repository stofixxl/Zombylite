using System;
using UnityEngine;

public class PlayerNeeds : MonoBehaviour
{
    [Header("Сытость")]
    [Min(1)]
    [SerializeField] private int maxSatiety = 100;

    [SerializeField] private int currentSatiety = 100;

    [Header("Голод со временем")]
    [SerializeField] private bool hungerEnabled = true;

    [Min(1f)]
    [SerializeField] private float secondsPerSatietyPoint = 60f;

    private float hungerTimer;

    // UI сможет обновлять показатель после изменения сытости.
    public event Action OnSatietyChanged;

    public int CurrentSatiety => currentSatiety;
    public int MaxSatiety => maxSatiety;

    private void Awake()
    {
        maxSatiety = Mathf.Max(1, maxSatiety);
        currentSatiety = Mathf.Clamp(currentSatiety, 0, maxSatiety);
    }

    private void Update()
    {
        if (!hungerEnabled || currentSatiety <= 0)
            return;

        hungerTimer += Time.deltaTime;

        float interval = Mathf.Max(1f, secondsPerSatietyPoint);

        // Вычитаем по одному очку за каждый прошедший интервал.
        while (hungerTimer >= interval && currentSatiety > 0)
        {
            hungerTimer -= interval;
            currentSatiety--;
            OnSatietyChanged?.Invoke();
        }
    }

    // Возвращает true, только если сытость действительно увеличилась.
    public bool TryRestoreSatiety(int amount)
    {
        if (amount <= 0 || currentSatiety >= maxSatiety)
            return false;

        currentSatiety = Mathf.Min(currentSatiety + amount, maxSatiety);
        OnSatietyChanged?.Invoke();
        return true;
    }

    [ContextMenu("Тест: уменьшить сытость на 30")]
    private void TestDecreaseSatiety()
    {
        currentSatiety = Mathf.Max(0, currentSatiety - 30);
        OnSatietyChanged?.Invoke();
        Debug.Log($"Сытость: {currentSatiety}/{maxSatiety}");
    }

    private void OnValidate()
    {
        maxSatiety = Mathf.Max(1, maxSatiety);
        currentSatiety = Mathf.Clamp(currentSatiety, 0, maxSatiety);
        secondsPerSatietyPoint = Mathf.Max(1f, secondsPerSatietyPoint);
    }
}