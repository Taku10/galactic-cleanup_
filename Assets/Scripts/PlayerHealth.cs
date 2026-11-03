using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public float hitFeedbackDuration = 1.2f;
    public float hitFlashInterval = 0.08f;
    public float hitShakeAmount = 0.12f;
    float currentHealth;
    bool dead;
    bool hitInvulnerable;
    Coroutine hitFeedbackRoutine;

    public TMP_Text healthText;

    void Start()
    {
        maxHealth = 5;
        currentHealth = maxHealth;
        UpdateUI();
        InGameUIStyler.Instance?.UpdateLives(currentHealth, maxHealth);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (dead) return;

        if (col.collider.CompareTag("Boss"))
        {
            BossKillAudio.PlayEvilLaugh();
            GamOverManger.ShowBossQuote();
            Die();
            return;
        }

        if (col.collider.CompareTag("Enemy") || col.collider.CompareTag("Wall"))
        {
            TakeDamage(1f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (dead) return;
        if (hitInvulnerable) return;

        PlayerPowerups powerups = GetComponent<PlayerPowerups>();
        if (powerups != null && powerups.ConsumeShield())
        {
            StartHitFeedback();
            return;
        }

        currentHealth -= amount;
        UpdateUI();
        InGameUIStyler.Instance?.UpdateLives(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartHitFeedback();
    }

    public void AddHealth(float amount)
    {
        if (dead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
        InGameUIStyler.Instance?.UpdateLives(currentHealth, maxHealth);
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

    void UpdateUI()
    {
        if (healthText != null)
            healthText.text = "LIVES\n" + FormatHealth(currentHealth) + " / " + FormatHealth(maxHealth);
    }

    string FormatHealth(float value)
    {
        float clamped = Mathf.Max(0f, value);
        return Mathf.Approximately(clamped, Mathf.Round(clamped))
            ? Mathf.RoundToInt(clamped).ToString()
            : clamped.ToString("0.0");
    }

    void Die()
    {
        if (dead) return;
        dead = true;
        GameFeelController.PlayerHit();

        ExplosionVfx.PlayPlayer(transform.position);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        var move = GetComponent<HumanMove>();
        if (move != null) move.enabled = false;

        if (GamOverManger.Instance != null)
            GamOverManger.Instance.GameOver();
        else
            Destroy(gameObject);
    }
}
