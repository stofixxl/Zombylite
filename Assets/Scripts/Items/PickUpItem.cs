using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    public int amount = 1;

    public void Interact()
    {
        if (itemData == null) return;

        bool added = InventoryManager.Instance.AddItem(itemData, amount);
        if (added)
        {
            Destroy(gameObject);
        }
    }

    public string GetInteractionText()
    {
        if (itemData == null) return "Подобрать";
        return "Подобрать " + itemData.itemName;
    }
}