using System.Collections.Generic;
using UnityEngine;

public class LootContainer : MonoBehaviour, IInteractable
{
    [Header("Лут")]
    public ItemData[] possibleLoot;
    public int minItems = 1;
    public int maxItems = 2;

    // Содержимое определяется только при первом обыске.
    private bool lootGenerated = false;
    private readonly List<ItemData> remainingLoot = new List<ItemData>();

    public void Interact()
    {
        if (!lootGenerated)
            GenerateLoot();

        if (remainingLoot.Count == 0)
        {
            Debug.Log("Пусто");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("Не найден InventoryManager на сцене.");
            return;
        }

        // Идём с конца списка, чтобы безопасно удалять подобранные предметы.
        for (int i = remainingLoot.Count - 1; i >= 0; i--)
        {
            ItemData item = remainingLoot[i];

            if (InventoryManager.Instance.AddItem(item))
            {
                Debug.Log("Найден предмет: " + item.itemName);
                remainingLoot.RemoveAt(i);
            }
        }

        if (remainingLoot.Count > 0)
            Debug.Log("Не всё поместилось в инвентарь. Оставшиеся предметы можно забрать позже.");
    }

    public string GetInteractionText()
    {
        if (lootGenerated && remainingLoot.Count == 0)
            return "Пусто";

        return "Обыскать";
    }

    private void GenerateLoot()
    {
        lootGenerated = true;

        if (possibleLoot == null || possibleLoot.Length == 0)
            return;

        // Пропускаем пустые элементы массива в Inspector.
        List<ItemData> validLoot = new List<ItemData>();

        foreach (ItemData item in possibleLoot)
        {
            if (item != null)
                validLoot.Add(item);
        }

        if (validLoot.Count == 0)
            return;

        int minimum = Mathf.Max(0, minItems);
        int maximum = Mathf.Max(minimum, maxItems);
        int count = Random.Range(minimum, maximum + 1);

        for (int i = 0; i < count; i++)
        {
            ItemData randomItem = validLoot[Random.Range(0, validLoot.Count)];
            remainingLoot.Add(randomItem);
        }
    }
}