using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossThrownObject : MonoBehaviour
{
    public float lifeTime = 6f;

    bool triggered;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player"))
            return;

        triggered = true;
        DamagePlayer(other.gameObject);
        Destroy(gameObject);
    }

    void DamagePlayer(GameObject player)
    {
        HumanDeath death = player.GetComponent<HumanDeath>();
        if (death != null)
        {
            death.TakeDamage(1f);
            return;
        }

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(1f);
    }
}
