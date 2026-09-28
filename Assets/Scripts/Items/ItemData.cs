using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int maxStack = 1;

    [Header("Тип предмета")]
    public ItemType itemType;

    public enum ItemType
    {
        Misc,
        Consumable,
        Weapon,
        Tool,
        Ammo
    }
}