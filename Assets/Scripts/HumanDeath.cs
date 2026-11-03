using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HumanDeath : MonoBehaviour
{
    public int maxLives = 5;
    public float hitFeedbackDuration = 1.2f;
    public float hitFlashInterval = 0.08f;
    public float hitShakeAmount = 0.12f;
    public GameObject explosionEffect;  // drag explosion prefab here
    public float restartDelay = 0.5f;   // delay before restart

    float lives;
    bool dead = false;
    bool hitInvulnerable;
    Coroutine hitFeedbackRoutine;

    void Start()
    {
        maxLives = 5;
        lives = maxLives;
        InGameUIStyler.Instance?.UpdateLives(lives, maxLives);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (dead) return;

        if (col.collider.CompareTag("Boss"))
        {
            BossKillAudio.PlayEvilLaugh();
            GamOverManger.ShowBossQuote();
            TakeDamage(maxLives, true);
            return;
        }

        if (col.collider.CompareTag("Enemy") || col.collider.CompareTag("Wall"))
        {
            TakeDamage(1f);
        }
    }

    public void TakeDamage(float amount, bool forceDeath = false)
    {
        if (dead) return;
        if (!forceDeath && hitInvulnerable) return;

        if (!forceDeath)
        {
            PlayerPowerups powerups = GetComponent<PlayerPowerups>();
            if (powerups != null && powerups.ConsumeShield())
            {
                StartHitFeedback();
                return;
            }
        }

        lives = forceDeath ? 0f : Mathf.Max(0f, lives - amount);
        InGameUIStyler.Instance?.UpdateLives(lives, maxLives);

        if (lives <= 0)
        {
            Die();
            return;
        }

        if (!forceDeath)
            StartHitFeedback();
    }

    public void AddLife(float amount)
    {
        if (dead) return;

        lives = Mathf.Min(maxLives, lives + amount);
        InGameUIStyler.Instance?.UpdateLives(lives, maxLives);
    }

    void StartHitFeedback()
    {
        if (hitFeedbackRoutine != null)
            StopCoroutine(hitFeedbackRoutine);

        hitFeedbackRoutine = StartCoroutine(HitFeedbackRoutine());
    }

    IEnumerator HitFeedbackRoutine()
    {
        hitInvulnerable = true;
        GameFeelController.PlayerHit();

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;

        Transform visual = renderers.Length > 0 ? renderers[0].transform : transform;
        Vector3 originalLocalPosition = visual.localPosition;

        float elapsed = 0f;
        bool dim = false;

        while (elapsed < hitFeedbackDuration && !dead)
        {
            dim = !dim;
            for (int i = 0; i < renderers.Length; i++)
            {
                Color color = originalColors[i];
                color.a = dim ? 0.35f : originalColors[i].a;
                renderers[i].color = color;
            }

            Vector2 shake = Random.insideUnitCircle * hitShakeAmount;
            visual.localPosition = originalLocalPosition + new Vector3(shake.x, shake.y, 0f);

            yield return new WaitForSeconds(hitFlashInterval);
            elapsed += hitFlashInterval;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = originalColors[i];
        }

        if (visual != null)
            visual.localPosition = originalLocalPosition;

        hitInvulnerable = false;
        hitFeedbackRoutine = null;
    }

    void Die()
    {
        dead = true;
        GameFeelController.PlayerHit();

        ExplosionVfx.PlayPlayer(transform.position);

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, transform.rotation);

        // hide visuals safely (sprite is likely on a child)
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        var move = GetComponent<HumanMove>();
        if (move != null) move.enabled = false;

        // Tell the manager: show restart + destroy walls
        if (GamOverManger.Instance != null)
            GamOverManger.Instance.GameOver();
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
