using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private void Awake()
    {
        FindComponents();
    }

    private void OnValidate()
    {
        FindComponents();
    }

    private void FindComponents()
    {
        // Ищем Image для иконки
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
                iconImage = iconTransform.GetComponent<Image>();
        }

        // Ищем TextMeshPro для количества
        if (amountText == null)
        {
            Transform amountTransform = transform.Find("Amount");
            if (amountTransform != null)
                amountText = amountTransform.GetComponent<TextMeshProUGUI>();
        }

        // Если всё ещё не нашли — ищем по всем детям
        if (amountText == null)
            amountText = GetComponentInChildren<TextMeshProUGUI>();

        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>();
    }

    public void SetEmpty()
    {
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
        {
            amountText.text = amount > 1 ? amount.ToString() : "";
        }
    }
}