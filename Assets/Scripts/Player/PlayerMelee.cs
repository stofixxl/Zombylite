using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [Header("Кулак (без оружия)")]
    [SerializeField] private int fistDamage = 10;
    [SerializeField] private float fistRange = 2f;
    [SerializeField] private float fistCooldown = 0.45f;

    [Header("Обнаружение цели")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float detectionRadius = 0.4f;

    public bool IsCombatActive { get; private set; }
    public bool IsAimingAtTarget { get; private set; }
    public bool HasTargetInReach { get; private set; }

    public event Action OnHitConfirmed;

    private Camera mainCamera;
    private PlayerHealth playerHealth;
    private PlayerEquipment playerEquipment;
    private float nextAttackTime;
    private IDamageable currentTarget;

    private void Awake()
    {
        mainCamera = Camera.main;
        playerHealth = GetComponent<PlayerHealth>();
        playerEquipment = GetComponent<PlayerEquipment>();
    }

    private void Update()
    {
        bool dead = playerHealth != null && playerHealth.IsDead;
        bool cursorFree = Cursor.lockState != CursorLockMode.Locked;

        IsCombatActive = !dead && !cursorFree;

        if (!IsCombatActive)
        {
            IsAimingAtTarget = false;
            HasTargetInReach = false;
            currentTarget = null;
            return;
        }

        UpdateTarget();

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (!HasTargetInReach)
            return;

        if (Time.time < nextAttackTime)
            return;

        ItemData weapon = playerEquipment != null ? playerEquipment.EquippedItem : null;

        float cooldown = weapon != null ? weapon.weaponCooldown : fistCooldown;
        nextAttackTime = Time.time + cooldown;

        int baseDamage = weapon != null ? weapon.weaponDamage : fistDamage;

        // Множитель от травм рук
        float mul = (playerHealth != null) ? playerHealth.MeleeDamageMultiplier : 1f;
        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * mul));

        string weaponName = weapon != null ? weapon.itemName : "Кулак";

        if (currentTarget != null && currentTarget.TakeDamage(damage))
        {
            Debug.Log($"Удар [{weaponName}]: {damage} урона.");
            OnHitConfirmed?.Invoke();

            // Тратим прочность только если есть экипированное оружие (slotId)
            if (playerEquipment != null && playerEquipment.EquippedSlotId >= 0 && InventoryManager.Instance != null)
            {
                bool broken;
                if (InventoryManager.Instance.DamageDurability(playerEquipment.EquippedSlotId, 1, out broken))
                {
                    if (broken)
                    {
                        Debug.Log($"Оружие [{weaponName}] СЛОМАЛОСЬ!");
                        playerEquipment.Unequip();
                    }
                }
            }
        }
    }

    private void UpdateTarget()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        ItemData weapon = playerEquipment != null ? playerEquipment.EquippedItem : null;
        float attackRange = weapon != null ? weapon.weaponRange : fistRange;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

        RaycastHit[] hits = Physics.SphereCastAll(
            ray.origin,
            detectionRadius,
            ray.direction,
            detectionRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        IDamageable found = null;
        float foundDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target == null)
                target = hit.collider.GetComponentInParent<IDamageable>();

            if (target != null)
            {
                found = target;
                foundDistance = hit.distance;
                break;
            }
        }

        currentTarget = found;
        IsAimingAtTarget = found != null;
        HasTargetInReach = found != null && foundDistance <= attackRange;
    }
}