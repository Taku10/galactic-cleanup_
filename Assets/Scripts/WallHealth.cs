using UnityEngine;

public class WallHealth : MonoBehaviour
{
    public int maxHealth = 5;

    int currentHealth;
    bool destroyed;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetMaxHealth(int health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        destroyed = false;
    }

    public void TakeDamage(int amount)
    {
        if (destroyed)
            return;

        currentHealth -= amount;
        if (currentHealth > 0)
            return;

        destroyed = true;
        ExplosionVfx.PlayEnemy(transform.position);
        Destroy(gameObject);
    }
}
