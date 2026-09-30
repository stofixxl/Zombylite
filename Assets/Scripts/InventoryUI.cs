using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform content;          // это Content внутри Scroll View
    [SerializeField] private InventorySlotUI slotPrefab;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private bool isOpen;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
        }

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

    private void RefreshUI()
    {
        if (InventoryManager.Instance == null) return;

        // Удаляем старые UI-слоты
        foreach (var slot in uiSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        uiSlots.Clear();

        // Получаем только занятые слоты
        var occupiedSlots = InventoryManager.Instance.GetAllSlots();

        // Создаём UI только для существующих предметов
        foreach (var slot in occupiedSlots)
        {
            InventorySlotUI newSlot = Instantiate(slotPrefab, content);
            newSlot.SetItem(slot.item, slot.amount);
            uiSlots.Add(newSlot);
        }
    }
}