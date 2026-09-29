using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
[RequireComponent(typeof(Image))]
public class CombatCrosshairUI : MonoBehaviour
{
    [SerializeField] private PlayerMelee playerMelee;

    [Header("Цвета")]
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color distantTargetColor = Color.yellow;
    [SerializeField] private Color reachableTargetColor = Color.green;
    [SerializeField] private Color hitColor = Color.red;

    [Header("Подтверждение попадания")]
    [Min(0.01f)]
    [SerializeField] private float hitFlashDuration = 0.15f;

    private Image crosshairImage;
    private Vector2 normalSize;
    private float hitFlashUntil;

    private void Awake()
    {
        crosshairImage = GetComponent<Image>();
        crosshairImage.raycastTarget = false;
        normalSize = crosshairImage.rectTransform.sizeDelta;
    }

    private void OnEnable()
    {
        if (playerMelee != null)
            playerMelee.OnHitConfirmed += ShowHit;
    }

    private void OnDisable()
    {
        if (playerMelee != null)
            playerMelee.OnHitConfirmed -= ShowHit;

        hitFlashUntil = 0f;
    }

    private void LateUpdate()
    {
        bool visible = playerMelee != null && playerMelee.IsCombatActive;
        crosshairImage.enabled = visible;

        if (!visible)
        {
            hitFlashUntil = 0f;
            return;
        }

        bool showingHit = Time.unscaledTime < hitFlashUntil;

        if (showingHit)
            crosshairImage.color = hitColor;
        else if (playerMelee.HasTargetInReach)
            crosshairImage.color = reachableTargetColor;
        else if (playerMelee.IsAimingAtTarget)
            crosshairImage.color = distantTargetColor;
        else
            crosshairImage.color = idleColor;

        crosshairImage.rectTransform.sizeDelta =
            showingHit ? normalSize * 2f : normalSize;
    }

    private void ShowHit()
    {
        hitFlashUntil = Time.unscaledTime + hitFlashDuration;
    }
}