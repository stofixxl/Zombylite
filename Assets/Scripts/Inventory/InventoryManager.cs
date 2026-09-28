using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int amount;

        public InventorySlot(ItemData item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }
    }

    [Header("Настройки")]
    public int maxSlots = 20;

    private List<InventorySlot> slots = new List<InventorySlot>();

    // UI сможет подписаться на это событие и обновляться после изменений.
    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        int stackSize = Mathf.Max(1, item.maxStack);

        // Сначала проверяем, поместится ли ВСЁ количество.
        // Пока ничего в инвентаре не меняем.
        long freeSpace = 0;

        foreach (var slot in slots)
        {
            if (slot.item == item)
                freeSpace += Mathf.Max(0, stackSize - slot.amount);
        }

        int freeSlots = Mathf.Max(0, maxSlots - slots.Count);
        freeSpace += (long)freeSlots * stackSize;

        if (freeSpace < amount)
            return false;

        // Теперь точно знаем, что места хватает.
        int remaining = amount;

        foreach (var slot in slots)
        {
            if (slot.item != item || slot.amount >= stackSize)
                continue;

            int toAdd = Mathf.Min(stackSize - slot.amount, remaining);
            slot.amount += toAdd;
            remaining -= toAdd;

            if (remaining == 0)
                break;
        }

        while (remaining > 0)
        {
            int toAdd = Mathf.Min(stackSize, remaining);
            slots.Add(new InventorySlot(item, toAdd));
            remaining -= toAdd;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        // Проверяем количество ДО удаления.
        long totalAmount = 0;

        foreach (var slot in slots)
        {
            if (slot.item == item)
                totalAmount += slot.amount;
        }

        if (totalAmount < amount)
            return false;

        int remaining = amount;

        for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (slots[i].item != item)
                continue;

            int toRemove = Mathf.Min(slots[i].amount, remaining);
            slots[i].amount -= toRemove;
            remaining -= toRemove;

            if (slots[i].amount == 0)
                slots.RemoveAt(i);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public List<InventorySlot> GetAllSlots()
    {
        // Возвращаем копии, чтобы UI не мог случайно изменить
        // содержимое инвентаря в обход AddItem и RemoveItem.
        List<InventorySlot> result = new List<InventorySlot>();

        foreach (var slot in slots)
            result.Add(new InventorySlot(slot.item, slot.amount));

        return result;
    }
    [ContextMenu("Показать инвентарь в Console")]
    public void DebugInventory()
    {
        Debug.Log("=== ИНВЕНТАРЬ ===");

        foreach (var slot in slots)
            Debug.Log($"{slot.item.itemName} x{slot.amount}");
    }
}