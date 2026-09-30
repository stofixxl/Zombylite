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
        equipment = FindAnyObjectByType<PlayerEquipment>();
        itemUse = FindAnyObjectByType<PlayerItemUse>();

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
    public void OnSlotClicked(int slotId, PointerEventData.InputButton button)
    {
        if (button != PointerEventData.InputButton.Left)
            return;

        if (InventoryManager.Instance == null)
            return;

        if (!InventoryManager.Instance.TryGetSlotById(slotId, out var slot) || slot == null || slot.item == null)
            return;

        ItemData item = slot.item;

        // Оружие: экипировать/снять по slotId
        if (item.itemType == ItemData.ItemType.Weapon)
        {
            if (equipment == null) equipment = FindAnyObjectByType<PlayerEquipment>();
            if (equipment == null) return;

            if (equipment.EquippedSlotId == slotId)
                equipment.Unequip();
            else
                equipment.TryEquipSlot(slotId);

            RefreshUI();
        }
        // Еда: использовать по ItemData (стаки)
        else if (item.itemType == ItemData.ItemType.Consumable)
        {
            if (itemUse == null) itemUse = FindAnyObjectByType<PlayerItemUse>();
            if (itemUse == null) return;

            itemUse.TryUseItem(item);
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (InventoryManager.Instance == null) return;

        if (equipment == null) equipment = FindAnyObjectByType<PlayerEquipment>();

        // Удаляем старые UI-слоты
        foreach (var slot in uiSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        uiSlots.Clear();

        var occupiedSlots = InventoryManager.Instance.GetAllSlots();

        foreach (var slot in occupiedSlots)
        {
            bool selected = (equipment != null && equipment.EquippedSlotId == slot.id);

            InventorySlotUI newSlot = Instantiate(slotPrefab, content);
            newSlot.Bind(this, slot.id, slot.item, slot.amount, selected);

            uiSlots.Add(newSlot);
        }
    }
}