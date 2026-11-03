using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LevelHazard : MonoBehaviour
{
    public float damage = 0.5f;
    public bool destroyOnPlayerHit;
    public bool explodeOnHit;
    public float rotateSpeed;

    void Update()
    {
        if (!Mathf.Approximately(rotateSpeed, 0f))
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        HumanDeath death = other.GetComponent<HumanDeath>();
        if (death != null)
            death.TakeDamage(damage);
        else
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);
        }

        if (explodeOnHit)
            ExplosionVfx.PlayEnemy(transform.position);

        if (destroyOnPlayerHit)
            Destroy(gameObject);
    }
}
