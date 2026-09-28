using UnityEngine;

public class LootContainer : MonoBehaviour, IInteractable
{
    [Header("Лут")]
    public ItemData[] possibleLoot;
    public int minItems = 1;
    public int maxItems = 2;

    private bool looted = false;

    public void Interact()
    {
        if (looted)
        {
            Debug.Log("Пусто");
            return;
        }

        looted = true;

        int count = Random.Range(minItems, maxItems + 1);

        for (int i = 0; i < count; i++)
        {
            if (possibleLoot.Length == 0) break;

            ItemData randomItem = possibleLoot[Random.Range(0, possibleLoot.Length)];
            InventoryManager.Instance.AddItem(randomItem);
            Debug.Log("Найден предмет: " + randomItem.itemName);
        }
    }

    public string GetInteractionText()
    {
        return looted ? "Пусто" : "Обыскать";
    }
}