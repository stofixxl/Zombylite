using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;

    [Min(1)]
    public int maxStack = 1;

    [Header("Тип предмета")]
    public ItemType itemType;

    [Header("Расходуемый предмет")]
    [Min(0)]
    public int satietyRestore = 0;

    [Header("Оружие")]
    [Min(0)]
    public int weaponDamage = 0;

    [Min(0f)]
    public float weaponRange = 1.6f;

    [Min(0f)]
    public float weaponCooldown = 0.5f;

    [Header("Прочность (для оружия/инструментов)")]
    [Min(1)]
    public int maxDurability = 100;

    [Tooltip("Минимальное состояние предмета при спавне (0..1). Например 0.3 = 30% прочности.")]
    [Range(0f, 1f)]
    public float spawnConditionMin = 0.5f;

    [Tooltip("Максимальное состояние предмета при спавне (0..1). Например 0.9 = 90% прочности.")]
    [Range(0f, 1f)]
    public float spawnConditionMax = 1.0f;

    public enum ItemType
    {
        Misc,
        Consumable,
        Weapon,
        Tool,
        Ammo
    }
}