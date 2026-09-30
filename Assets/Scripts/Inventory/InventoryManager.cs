using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [System.Serializable]
    public class InventorySlot
    {
        public int id;                 // уникальный ID слота (важно для экипировки)
        public ItemData item;
        public int amount;

        // Для оружия/инструментов (когда maxStack = 1).
        public int durability;

        public InventorySlot(int id, ItemData item, int amount, int durability)
        {
            this.id = id;
            this.item = item;
            this.amount = amount;
            this.durability = durability;
        }
    }

    [Header("Настройки")]
    public int maxSlots = 20;

    private List<InventorySlot> slots = new List<InventorySlot>();
    private int nextSlotId = 1;

    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        int stackSize = Mathf.Max(1, item.maxStack);

        // Проверка: хватит ли места под ВСЁ
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

        int remaining = amount;

        // Стакаем только если стак > 1
        if (stackSize > 1)
        {
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
        }

        // Создаём новые слоты
        while (remaining > 0)
        {
            int toAdd = Mathf.Min(stackSize, remaining);

            int durability = 0;
            if (item.itemType == ItemData.ItemType.Weapon || item.itemType == ItemData.ItemType.Tool)
                durability = GetRandomSpawnDurability(item);

            slots.Add(new InventorySlot(nextSlotId++, item, toAdd, durability));
            remaining -= toAdd;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        long totalAmount = 0;
        foreach (var slot in slots)
            if (slot.item == item)
                totalAmount += slot.amount;

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

    // НОВОЕ: попытаться получить внутренний слот по id
    public bool TryGetSlotById(int slotId, out InventorySlot slot)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].id == slotId)
            {
                slot = slots[i];
                return true;
            }
        }

        slot = null;
        return false;
    }

    // НОВОЕ: уронить прочность слота. Если сломался — слот удаляется.
    public bool DamageDurability(int slotId, int damageAmount, out bool broken)
    {
        broken = false;

        if (damageAmount <= 0)
            return false;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].id != slotId)
                continue;

            InventorySlot s = slots[i];

            // Если это не оружие/инструмент — durability не трогаем
            if (s.item == null || (s.item.itemType != ItemData.ItemType.Weapon && s.item.itemType != ItemData.ItemType.Tool))
                return false;

            // На всякий: durability у оружия не должен быть 0 при нормальной генерации
            if (s.durability <= 0)
                s.durability = Mathf.Max(1, s.item.maxDurability);

            s.durability -= damageAmount;

            if (s.durability <= 0)
            {
                // сломался
                broken = true;
                slots.RemoveAt(i);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    public List<InventorySlot> GetAllSlots()
    {
        // Копии, чтобы UI не менял инвентарь напрямую
        List<InventorySlot> result = new List<InventorySlot>(slots.Count);

        foreach (var slot in slots)
            result.Add(new InventorySlot(slot.id, slot.item, slot.amount, slot.durability));

        return result;
    }

    public void DebugInventory()
    {
        Debug.Log("=== ИНВЕНТАРЬ ===");
        foreach (var slot in slots)
        {
            string extra = "";
            if (slot.item != null && (slot.item.itemType == ItemData.ItemType.Weapon || slot.item.itemType == ItemData.ItemType.Tool))
                extra = $" (dur {slot.durability}/{slot.item.maxDurability})";

            Debug.Log($"{slot.item.itemName} x{slot.amount}{extra} [id={slot.id}]");
        }
    }

    private int GetRandomSpawnDurability(ItemData item)
    {
        int maxD = Mathf.Max(1, item.maxDurability);

        float min = Mathf.Clamp01(item.spawnConditionMin);
        float max = Mathf.Clamp01(item.spawnConditionMax);
        if (max < min) max = min;

        float k = Random.Range(min, max);
        int d = Mathf.RoundToInt(maxD * k);

        return Mathf.Clamp(d, 1, maxD);
    }
}