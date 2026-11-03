using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int maxHealth = 1;
    int currentHealth;
    bool isDead;

    public GameObject explosionPrefab;

    [Header("SFX")]
    public AudioClip deathClip;
    [Range(0f, 1f)] public float deathVolume = 0.7f;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            isDead = true;

            if (deathClip != null)
                AudioSource.PlayClipAtPoint(deathClip, transform.position, deathVolume);

            ExplosionVfx.PlayEnemy(transform.position);
            GameFeelController.EnemyDeath(transform.position);

            if (explosionPrefab != null)
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            PowerUpSpawner.TrySpawn(transform.position);
            PowerUpSpawner.TrySpawnRocketAmmo(transform.position);

            ScoreManager.Instance?.AddPoint(1);

            // tells LevelManager that one enemy was killed
            if (LevelManager.Instance != null)
                LevelManager.Instance.EnemyKilled();

            Destroy(gameObject);
        }
    }
}
