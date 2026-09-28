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

    public enum ItemType
    {
        Misc,
        Consumable,
        Weapon,
        Tool,
        Ammo
    }
}