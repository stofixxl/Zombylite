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

    // Событие, на которое потом подпишется UI
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
        if (item == null) return false;

        // Сначала пытаемся добавить в существующий стак
        if (item.maxStack > 1)
        {
            foreach (var slot in slots)
            {
                if (slot.item == item && slot.amount < item.maxStack)
                {
                    int canAdd = item.maxStack - slot.amount;
                    int toAdd = Mathf.Min(canAdd, amount);
                    slot.amount += toAdd;
                    amount -= toAdd;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        // Если ещё остались предметы — создаём новые слоты
        while (amount > 0 && slots.Count < maxSlots)
        {
            int toAdd = Mathf.Min(item.maxStack, amount);
            slots.Add(new InventorySlot(item, toAdd));
            amount -= toAdd;
        }

        OnInventoryChanged?.Invoke();
        return amount <= 0;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i].item == item)
            {
                if (slots[i].amount > amount)
                {
                    slots[i].amount -= amount;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
                else
                {
                    amount -= slots[i].amount;
                    slots.RemoveAt(i);
                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public List<InventorySlot> GetAllSlots()
    {
        return slots;
    }

    public void DebugInventory()
    {
        Debug.Log("=== ИНВЕНТАРЬ ===");
        foreach (var slot in slots)
        {
            Debug.Log($"{slot.item.itemName} x{slot.amount}");
        }
    }
}