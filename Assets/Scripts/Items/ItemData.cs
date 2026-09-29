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

    public enum ItemType
    {
        Misc,
        Consumable,
        Weapon,
        Tool,
        Ammo
    }
}