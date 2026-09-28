using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private Dictionary<string, int> items =
        new Dictionary<string, int>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddItem(string itemName)
    {
        if (items.ContainsKey(itemName))
            items[itemName]++;
        else
            items[itemName] = 1;

        DebugInventory();
    }

    private void DebugInventory()
    {
        Debug.Log("===== ИНВЕНТАРЬ =====");

        foreach (var item in items)
        {
            Debug.Log(item.Key + " x" + item.Value);
        }
    }
}