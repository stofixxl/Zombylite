using UnityEngine;

[RequireComponent(typeof(PlayerNeeds))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerItemUse : MonoBehaviour
{
    [Header("Только для теста через Inspector")]
    [SerializeField] private ItemData testItem;

    private PlayerNeeds playerNeeds;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerNeeds = GetComponent<PlayerNeeds>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    // Позже этот метод сможет вызвать кнопка UI.
    // true — предмет успешно использован и потрачен.
    public bool TryUseItem(ItemData item)
    {
        if (playerHealth.IsDead)
            return false;

        if (item == null || item.itemType != ItemData.ItemType.Consumable)
            return false;

        if (item.satietyRestore <= 0)
            return false;

        // Не тратим еду, если сытость уже полная.
        if (playerNeeds.CurrentSatiety >= playerNeeds.MaxSatiety)
            return false;

        InventoryManager inventory = InventoryManager.Instance;

        if (inventory == null)
        {
            Debug.LogWarning("Не найден InventoryManager на сцене.");
            return false;
        }

        // Сначала тратим предмет. Если его нет — сытость не меняется.
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
            Debug.Log($"Не удалось использовать: {testItem.itemName}. Проверь здоровье, сытость и наличие предмета в инвентаре.");
    }
}