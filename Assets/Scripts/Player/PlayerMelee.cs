using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float attackRadius = 0.35f;
    [SerializeField] private float attackCooldown = 0.45f;

    private Camera mainCamera;
    private PlayerHealth playerHealth;
    private float nextAttackTime;

    private void Awake()
    {
        mainCamera = Camera.main;
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;
        TryAttack();
    }

    private void TryAttack()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 origin = transform.position + Vector3.up * 1.1f;
        Vector3 direction = mainCamera != null
            ? mainCamera.transform.forward
            : transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            attackRadius,
            direction,
            attackRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
                continue;

            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if (target == null)
                target = hit.collider.GetComponentInParent<IDamageable>();

            if (target != null)
            {
                target.TakeDamage(damage);
                return;
            }

            Debug.Log("Удар попал в " + hit.collider.name + ", но это не цель.");
            return;
        }

        Debug.Log("Промах.");
    }
}