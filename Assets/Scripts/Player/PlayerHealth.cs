using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Здоровье")]
    [Min(1)]
    [SerializeField] private int maxHealth = 100;

    [SerializeField] private int currentHealth = 100;

    public event Action OnHealthChanged;
    public event Action OnDied;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    // Возвращает true, если урон действительно был нанесён.
    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
            return false;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke();

        if (IsDead)
        {
            Debug.Log("Игрок погиб.");
            OnDied?.Invoke();
        }

        return true;
    }

    // Возвращает true, если здоровье действительно восстановилось.
    public bool Heal(int amount)
    {
        if (amount <= 0 || IsDead || currentHealth >= maxHealth)
            return false;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke();
        return true;
    }

    [ContextMenu("Тест: получить 25 урона")]
    private void TestTakeDamage()
    {
        TakeDamage(25);
        Debug.Log($"Здоровье: {currentHealth}/{maxHealth}");
    }

    [ContextMenu("Тест: вылечить 10 здоровья")]
    private void TestHeal()
    {
        Heal(10);
        Debug.Log($"Здоровье: {currentHealth}/{maxHealth}");
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}