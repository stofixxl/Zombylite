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

    [Header("Урон от голода (при сытости 0)")]
    [Min(1f)]
    [SerializeField] private float secondsPerStarvationDamage = 10f;

    [Min(1)]
    [SerializeField] private int starvationDamage = 1;

    private float hungerTimer;
    private float starvationTimer;
    private PlayerHealth playerHealth;

    // UI сможет обновлять показатель после изменения сытости.
    public event Action OnSatietyChanged;

    public int CurrentSatiety => currentSatiety;
    public int MaxSatiety => maxSatiety;
    public bool IsStarving => currentSatiety <= 0;

    private void Awake()
    {
        maxSatiety = Mathf.Max(1, maxSatiety);
        currentSatiety = Mathf.Clamp(currentSatiety, 0, maxSatiety);

        // Здоровье на том же объекте Player. Если его нет — голод просто не будет ранить.
        playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null)
            Debug.LogWarning("PlayerNeeds: на игроке нет PlayerHealth, урон от голода отключён.");
    }

    private void Update()
    {
        if (!hungerEnabled)
            return;

        // Если игрок мёртв — ничего не считаем.
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (currentSatiety > 0)
            UpdateHunger();
        else
            UpdateStarvation();
    }

    private void UpdateHunger()
    {
        // Пока есть сытость, таймер голодного урона не копится.
        starvationTimer = 0f;

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

    private void UpdateStarvation()
    {
        if (playerHealth == null)
            return;

        starvationTimer += Time.deltaTime;

        float interval = Mathf.Max(1f, secondsPerStarvationDamage);

        while (starvationTimer >= interval)
        {
            starvationTimer -= interval;
            playerHealth.TakeDamage(starvationDamage);

            if (playerHealth.IsDead)
                break;
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

    [ContextMenu("Тест: обнулить сытость")]
    private void TestSetSatietyZero()
    {
        currentSatiety = 0;
        OnSatietyChanged?.Invoke();
        Debug.Log($"Сытость: {currentSatiety}/{maxSatiety} — начался голод.");
    }

    private void OnValidate()
    {
        maxSatiety = Mathf.Max(1, maxSatiety);
        currentSatiety = Mathf.Clamp(currentSatiety, 0, maxSatiety);
        secondsPerSatietyPoint = Mathf.Max(1f, secondsPerSatietyPoint);
        secondsPerStarvationDamage = Mathf.Max(1f, secondsPerStarvationDamage);
        starvationDamage = Mathf.Max(1, starvationDamage);
    }
}