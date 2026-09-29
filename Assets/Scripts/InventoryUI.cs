using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private InventorySlotUI slotPrefab;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private bool isOpen;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
        }

        CreateSlots();
        RefreshUI();
        inventoryPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    private void Update()
    {
        // Новая Input System
        if (Keyboard.current == null) return;

        if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
            RefreshUI();
    }

    private void CreateSlots()
    {
        // Очищаем старые слоты
        foreach (Transform child in slotsContainer)
            Destroy(child.gameObject);

        uiSlots.Clear();

        int maxSlots = InventoryManager.Instance.maxSlots;

        for (int i = 0; i < maxSlots; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, slotsContainer);
            slot.SetEmpty();
            uiSlots.Add(slot);
        }
    }

    private void RefreshUI()
    {
        if (InventoryManager.Instance == null) return;

        var occupiedSlots = InventoryManager.Instance.GetAllSlots();

        // Сначала всё очищаем
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].SetEmpty();
        }

        // Заполняем занятыми слотами
        int index = 0;
        foreach (var slot in occupiedSlots)
        {
            if (index >= uiSlots.Count) break;

            uiSlots[index].SetItem(slot.item, slot.amount);
            index++;
        }
    }
}