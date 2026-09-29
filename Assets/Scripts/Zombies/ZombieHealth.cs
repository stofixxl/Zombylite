using UnityEngine;

public class ZombieHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 40;
    [SerializeField] private int currentHealth = 40;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
            return false;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"Зомби получил {amount} урона. Здоровье: {currentHealth}/{maxHealth}");

        if (IsDead)
        {
            Debug.Log("Зомби убит.");
            gameObject.SetActive(false);
        }

        return true;
    }
}