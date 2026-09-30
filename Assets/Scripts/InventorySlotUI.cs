using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image backgroundImage;

    private InventoryUI owner;
    private int slotId = -1;

    private void Awake() => FindComponents();
    private void OnValidate() => FindComponents();

    private void FindComponents()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (iconImage == null)
        {
            Transform t = transform.Find("Icon");
            if (t != null) iconImage = t.GetComponent<Image>();
        }

        if (amountText == null)
        {
            Transform t = transform.Find("Amount");
            if (t != null) amountText = t.GetComponent<TextMeshProUGUI>();
        }

        if (amountText == null) amountText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (iconImage == null) iconImage = GetComponentInChildren<Image>(true);
    }

    // ВАЖНО: теперь передаём slotId
    public void Bind(InventoryUI owner, int slotId, ItemData item, int amount, bool selected)
    {
        this.owner = owner;
        this.slotId = slotId;

        if (backgroundImage != null)
            backgroundImage.raycastTarget = true;

        SetItem(item, amount);
        SetSelected(selected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner == null || slotId < 0) return;
        owner.OnSlotClicked(slotId, eventData.button);
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage == null) return;

        backgroundImage.color = selected
            ? new Color(0.2f, 0.6f, 1f, 0.6f)
            : Color.white;
    }

    public void SetEmpty()
    {
        slotId = -1;

        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }

        if (amountText != null)
            amountText.text = "";
    }

    public void SetItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            SetEmpty();
            return;
        }

        if (iconImage != null)
        {
            if (item.icon != null)
            {
                iconImage.sprite = item.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        if (amountText != null)
            amountText.text = amount > 1 ? amount.ToString() : "";
    }
}