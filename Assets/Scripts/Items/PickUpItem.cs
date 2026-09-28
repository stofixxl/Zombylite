using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public string itemName = "Консерва";

    public void Interact()
    {
        InventoryManager.Instance.AddItem(itemName);

        Destroy(gameObject);
    }

    public string GetInteractionText()
    {
        return "Подобрать " + itemName;
    }
}