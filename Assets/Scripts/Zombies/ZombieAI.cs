using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float detectionRange = 10f;

    private void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(transform.position,
                             player.position);

        if (distance <= detectionRange)
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    speed * Time.deltaTime);
        }
    }
}