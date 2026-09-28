using UnityEngine;

[RequireComponent(typeof(PlayerNeeds))]
public class PlayerItemUse : MonoBehaviour
{
    [Header("Только для теста через Inspector")]
    [SerializeField] private ItemData testItem;

    private PlayerNeeds playerNeeds;

    private void Awake()
    {
        playerNeeds = GetComponent<PlayerNeeds>();
    }

    // Позже UI сможет вызвать этот метод для выбранного предмета.
    // true означает, что предмет использован и удалён из инвентаря.
    public bool TryUseItem(ItemData item)
    {
        if (item == null || item.itemType != ItemData.ItemType.Consumable)
            return false;

        if (item.satietyRestore <= 0)
            return false;

        // Не тратим консервы, если игрок уже сыт.
        if (playerNeeds.CurrentSatiety >= playerNeeds.MaxSatiety)
            return false;

        InventoryManager inventory = InventoryManager.Instance;

        if (inventory == null)
        {
            Debug.LogWarning("Не найден InventoryManager на сцене.");
            return false;
        }

        // Если предмета нет в инвентаре, RemoveItem вернёт false.
        // Сытость в таком случае не меняется.
        if (!inventory.RemoveItem(item, 1))
            return false;

        playerNeeds.TryRestoreSatiety(item.satietyRestore);
        return true;
    }

    [ContextMenu("Тест: использовать предмет")]
    private void TestUseItem()
    {
        if (testItem == null)
        {
            Debug.LogWarning("В Player Item Use не назначен Test Item.");
            return;
        }

        bool used = TryUseItem(testItem);

        if (used)
            Debug.Log($"Использован предмет: {testItem.itemName}. Сытость: {playerNeeds.CurrentSatiety}/{playerNeeds.MaxSatiety}");
        else
            Debug.Log($"Не удалось использовать: {testItem.itemName}. Проверь сытость и наличие предмета в инвентаре.");
    }
}