using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    // Текущий экипированный предмет. null — голые руки.
    public ItemData EquippedItem { get; private set; }

    // UI и другие скрипты смогут подписаться на смену оружия.
    public System.Action OnEquipmentChanged;

    // Экипировать предмет из инвентаря.
    // Если передать null — снять оружие.
    public bool TryEquip(ItemData item)
    {
        if (item != null && item.itemType != ItemData.ItemType.Weapon)
        {
            Debug.Log($"{item.itemName} нельзя экипировать: это не оружие.");
            return false;
        }

        EquippedItem = item;
        OnEquipmentChanged?.Invoke();

        string name = item != null ? item.itemName : "Кулаки";
        Debug.Log($"Экипировано: {name}");
        return true;
    }

    // Снять оружие.
    public void Unequip()
    {
        TryEquip(null);
    }
    [Header("Только для теста через Inspector")]
    [SerializeField] private ItemData testEquipItem;

    [ContextMenu("Тест: экипировать предмет")]
    private void TestEquip()
    {
        if (testEquipItem == null)
        {
            Debug.LogWarning("Не назначен Test Equip Item.");
            return;
        }

        TryEquip(testEquipItem);
    }
    [ContextMenu("Тест: снять оружие")]
    private void TestUnequip()
    {
        Unequip();
    }
}