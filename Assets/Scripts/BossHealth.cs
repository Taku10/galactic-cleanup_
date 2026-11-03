using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 20;
    int currentHealth;
    bool isDead;
    bool phaseTwoTriggered;

    public GameObject explosionPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        InGameUIStyler.Instance?.ShowBossHealth(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        InGameUIStyler.Instance?.UpdateBossHealth(currentHealth, maxHealth);

        if (!phaseTwoTriggered && currentHealth > 0 && currentHealth <= maxHealth / 2)
            TriggerPhaseTwo();

        if (currentHealth <= 0)
        {
            isDead = true;

            ExplosionVfx.PlayBoss(transform.position);
            GameFeelController.BossDeath();

            if (explosionPrefab != null)
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            if (LevelManager.Instance != null)
                LevelManager.Instance.BossDefeated();

            InGameUIStyler.Instance?.HideBossHealth();
            Destroy(gameObject);
        }
    }

    void TriggerPhaseTwo()
    {
        phaseTwoTriggered = true;
        GameFeelController.BossPhase();

        BossAttack attack = GetComponent<BossAttack>();
        if (attack != null)
            attack.EnterPhaseTwo();

        BossChase chase = GetComponent<BossChase>();
        if (chase != null)
            chase.EnterPhaseTwo();
    }
}
