using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public enum BodyPartType
    {
        Head,
        Torso,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg
    }

    [Serializable]
    public class BodyPartState
    {
        public BodyPartType part;
        public int maxHp;
        public int hp;

        public float Ratio => maxHp <= 0 ? 0f : (float)hp / maxHp;

        public BodyPartState(BodyPartType part, int maxHp, int hp)
        {
            this.part = part;
            this.maxHp = maxHp;
            this.hp = hp;
        }
    }

    [Header("Общее здоровье (как раньше)")]
    [Min(1)]
    [SerializeField] private int maxHealth = 100;

    [SerializeField] private int currentHealth = 100;

    [Header("Здоровье по частям тела (MVP)")]
    [SerializeField] private BodyPartState head;
    [SerializeField] private BodyPartState torso;
    [SerializeField] private BodyPartState leftArm;
    [SerializeField] private BodyPartState rightArm;
    [SerializeField] private BodyPartState leftLeg;
    [SerializeField] private BodyPartState rightLeg;

    public event Action OnHealthChanged;
    public event Action OnDied;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0 || head.hp <= 0 || torso.hp <= 0;

    // Эффекты от травм
    public float MoveSpeedMultiplier => CalculateMoveMultiplier();
    public float MeleeDamageMultiplier => CalculateMeleeMultiplier();

    private bool diedInvoked;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        EnsurePartsInitialized();
        RecalculateTotalsAndDeathCheck();
    }

    private void EnsurePartsInitialized()
    {
        // Если части тела не созданы (после замены скрипта) — создаём из maxHealth
        bool needInit =
            head == null || torso == null ||
            leftArm == null || rightArm == null ||
            leftLeg == null || rightLeg == null ||
            head.maxHp <= 0 || torso.maxHp <= 0;

        if (!needInit)
            return;

        float k = (maxHealth <= 0) ? 1f : (float)currentHealth / maxHealth;

        // Распределение как в “100 total”:
        // Head 10, Torso 40, Arms 15+15, Legs 10+10 = 100
        int headMax = Mathf.RoundToInt(maxHealth * 0.10f);
        int torsoMax = Mathf.RoundToInt(maxHealth * 0.40f);
        int armMax = Mathf.RoundToInt(maxHealth * 0.15f);
        int legMax = Mathf.RoundToInt(maxHealth * 0.10f);

        // Из-за округления может не совпасть сумма — поправим торс
        int sum = headMax + torsoMax + armMax + armMax + legMax + legMax;
        int diff = maxHealth - sum;
        torsoMax += diff;

        head = new BodyPartState(BodyPartType.Head, headMax, Mathf.Clamp(Mathf.RoundToInt(headMax * k), 0, headMax));
        torso = new BodyPartState(BodyPartType.Torso, torsoMax, Mathf.Clamp(Mathf.RoundToInt(torsoMax * k), 0, torsoMax));
        leftArm = new BodyPartState(BodyPartType.LeftArm, armMax, Mathf.Clamp(Mathf.RoundToInt(armMax * k), 0, armMax));
        rightArm = new BodyPartState(BodyPartType.RightArm, armMax, Mathf.Clamp(Mathf.RoundToInt(armMax * k), 0, armMax));
        leftLeg = new BodyPartState(BodyPartType.LeftLeg, legMax, Mathf.Clamp(Mathf.RoundToInt(legMax * k), 0, legMax));
        rightLeg = new BodyPartState(BodyPartType.RightLeg, legMax, Mathf.Clamp(Mathf.RoundToInt(legMax * k), 0, legMax));
    }

    public bool TakeDamage(int amount)
    {
        // Для совместимости: если кто-то вызывает TakeDamage(5) — это урон в торс.
        return TakeDamage(BodyPartType.Torso, amount);
    }

    public bool TakeDamage(BodyPartType part, int amount)
    {
        if (amount <= 0 || IsDead)
            return false;

        BodyPartState p = GetPart(part);
        if (p == null)
            return false;

        p.hp = Mathf.Max(0, p.hp - amount);

        RecalculateTotalsAndDeathCheck();
        OnHealthChanged?.Invoke();
        return true;
    }

    public bool Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return false;

        // MVP: лечим самую повреждённую часть тела, пока не закончится amount
        int remaining = amount;
        while (remaining > 0)
        {
            BodyPartState mostDamaged = GetMostDamagedPart();
            if (mostDamaged == null) break;

            int missing = mostDamaged.maxHp - mostDamaged.hp;
            if (missing <= 0) break;

            int add = Mathf.Min(missing, remaining);
            mostDamaged.hp += add;
            remaining -= add;
        }

        bool changed = remaining != amount;

        if (changed)
        {
            RecalculateTotalsAndDeathCheck();
            OnHealthChanged?.Invoke();
        }

        return changed;
    }

    private BodyPartState GetMostDamagedPart()
    {
        BodyPartState[] parts = { head, torso, leftArm, rightArm, leftLeg, rightLeg };

        BodyPartState best = null;
        int bestMissing = 0;

        foreach (var p in parts)
        {
            if (p == null) continue;
            int missing = p.maxHp - p.hp;
            if (missing > bestMissing)
            {
                bestMissing = missing;
                best = p;
            }
        }

        return best;
    }

    private BodyPartState GetPart(BodyPartType part)
    {
        return part switch
        {
            BodyPartType.Head => head,
            BodyPartType.Torso => torso,
            BodyPartType.LeftArm => leftArm,
            BodyPartType.RightArm => rightArm,
            BodyPartType.LeftLeg => leftLeg,
            BodyPartType.RightLeg => rightLeg,
            _ => torso
        };
    }

    private void RecalculateTotalsAndDeathCheck()
    {
        currentHealth =
            SafeHp(head) + SafeHp(torso) +
            SafeHp(leftArm) + SafeHp(rightArm) +
            SafeHp(leftLeg) + SafeHp(rightLeg);

        // maxHealth оставляем тем, что задано в инспекторе (оно — “бюджет”),
        // но если вдруг у частей получилась другая сумма — можно пересчитать.
        // Для MVP оставим как есть.

        if (IsDead && !diedInvoked)
        {
            diedInvoked = true;
            Debug.Log("Игрок погиб (части тела).");
            OnDied?.Invoke();
        }
    }

    private int SafeHp(BodyPartState p) => p != null ? p.hp : 0;

    private float CalculateMoveMultiplier()
    {
        // Берём худшую ногу (если одна нога сильно ранена — ты хромаешь)
        float left = leftLeg != null ? leftLeg.Ratio : 1f;
        float right = rightLeg != null ? rightLeg.Ratio : 1f;
        float r = Mathf.Min(left, right);

        if (r > 0.7f) return 1f;
        if (r > 0.4f) return 0.75f;
        if (r > 0.2f) return 0.55f;
        return 0.4f;
    }

    private float CalculateMeleeMultiplier()
    {
        // Худшая рука влияет сильнее
        float left = leftArm != null ? leftArm.Ratio : 1f;
        float right = rightArm != null ? rightArm.Ratio : 1f;
        float r = Mathf.Min(left, right);

        if (r > 0.7f) return 1f;
        if (r > 0.4f) return 0.85f;
        if (r > 0.2f) return 0.7f;
        return 0.6f;
    }

    // ТЕСТЫ В INSPECTOR (через ⋮ на компоненте PlayerHealth)
    [ContextMenu("Тест: урон в левую ногу (5)")]
    private void TestDamageLeftLeg() => TakeDamage(BodyPartType.LeftLeg, 5);

    [ContextMenu("Тест: урон в правую руку (5)")]
    private void TestDamageRightArm() => TakeDamage(BodyPartType.RightArm, 5);

    [ContextMenu("Тест: урон в голову (5)")]
    private void TestDamageHead() => TakeDamage(BodyPartType.Head, 5);

    [ContextMenu("Тест: вывести множители (скорость/урон)")]
    private void TestPrintMultipliers()
    {
        Debug.Log($"MoveSpeedMultiplier={MoveSpeedMultiplier:0.00}, MeleeDamageMultiplier={MeleeDamageMultiplier:0.00}");
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}