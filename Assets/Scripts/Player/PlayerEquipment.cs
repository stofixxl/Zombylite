using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public int EquippedSlotId { get; private set; } = -1;

    public ItemData EquippedItem
    {
        get
        {
            if (EquippedSlotId < 0) return null;
            if (InventoryManager.Instance == null) return null;

            if (InventoryManager.Instance.TryGetSlotById(EquippedSlotId, out var slot))
                return slot.item;

            return null;
        }
    }

    public System.Action OnEquipmentChanged;

    public bool TryEquipSlot(int slotId)
    {
        if (InventoryManager.Instance == null)
            return false;

        if (!InventoryManager.Instance.TryGetSlotById(slotId, out var slot) || slot == null || slot.item == null)
            return false;

        if (slot.item.itemType != ItemData.ItemType.Weapon)
        {
            Debug.Log($"{slot.item.itemName} нельзя экипировать: это не оружие.");
            return false;
        }

        EquippedSlotId = slotId;
        OnEquipmentChanged?.Invoke();
        Debug.Log($"Экипировано: {slot.item.itemName} (id={slotId})");
        return true;
    }

    public void Unequip()
    {
        if (EquippedSlotId == -1) return;

        EquippedSlotId = -1;
        OnEquipmentChanged?.Invoke();
        Debug.Log("Экипировано: Кулаки");
    }
}