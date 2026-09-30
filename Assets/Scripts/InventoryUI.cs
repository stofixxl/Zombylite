using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform content;          // Content внутри Scroll View
    [SerializeField] private InventorySlotUI slotPrefab;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private bool isOpen;

    private PlayerEquipment equipment;
    private PlayerItemUse itemUse;

    private void Start()
    {
        equipment = FindFirstObjectByType<PlayerEquipment>();
        itemUse = FindFirstObjectByType<PlayerItemUse>();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        UIInputBlock.IsUIOpen = false;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame)
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isOpen);

        UIInputBlock.IsUIOpen = isOpen;

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RefreshUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Вызывается из InventorySlotUI при клике
    public void OnSlotClicked(ItemData item, PointerEventData.InputButton button)
    {
        if (item == null) return;
        if (button != PointerEventData.InputButton.Left) return;

        // ЛКМ по оружию — экипировать/снять
        if (item.itemType == ItemData.ItemType.Weapon)
        {
            if (equipment == null) equipment = FindFirstObjectByType<PlayerEquipment>();
            if (equipment == null) return;

            if (equipment.EquippedItem == item)
                equipment.Unequip();
            else
                equipment.TryEquip(item);

            RefreshUI();
        }
        // ЛКМ по еде — использовать
        else if (item.itemType == ItemData.ItemType.Consumable)
        {
            if (itemUse == null) itemUse = FindFirstObjectByType<PlayerItemUse>();
            if (itemUse == null) return;

            itemUse.TryUseItem(item);
            RefreshUI();
        }
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
            newSlot.Bind(this, slot.item, slot.amount);

            // подсветка экипированного оружия
            if (equipment != null && equipment.EquippedItem != null && slot.item == equipment.EquippedItem)
                newSlot.SetSelected(true);

            uiSlots.Add(newSlot);
        }
    }
}