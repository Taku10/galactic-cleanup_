using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    public float damage = 0.5f;
    public float lifeTime = 4f;

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

        HumanDeath death = other.GetComponent<HumanDeath>();
        if (death != null)
        {
            death.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }
}
