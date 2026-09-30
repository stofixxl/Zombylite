using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BodyPartHitbox : MonoBehaviour
{
    [SerializeField] private PlayerHealth.BodyPartType bodyPart;
    [SerializeField] private PlayerHealth playerHealth;

    public PlayerHealth.BodyPartType BodyPart => bodyPart;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();

        // важно: хитбокс должен быть Trigger
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    public bool ApplyDamage(int amount)
    {
        if (playerHealth == null) return false;
        return playerHealth.TakeDamage(bodyPart, amount);
    }
}