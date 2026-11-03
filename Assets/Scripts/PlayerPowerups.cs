using System.Collections;
using UnityEngine;

public class PlayerPowerups : MonoBehaviour
{
    public float speedBoostMultiplier = 1.45f;
    public float rapidFireMultiplier = 1.75f;
    public float powerUpDuration = 8f;

    Coroutine speedRoutine;
    Coroutine rapidFireRoutine;
    Coroutine shieldRoutine;
    HumanMove boostedMove;
    float originalSpeed;
    bool speedBoostActive;
    Shoot2D[] boostedShooters;
    float[] originalFireRates;
    bool rapidFireActive;
    float shieldUntil;
    GameObject shieldVisual;
    Sprite shieldSprite;

    public void Apply(PowerUpType type)
    {
        GameFeelController.PowerUpCollected();

        switch (type)
        {
            case PowerUpType.Heal:
                HumanDeath death = GetComponent<HumanDeath>();
                if (death != null)
                    death.AddLife(1);

                PlayerHealth health = GetComponent<PlayerHealth>();
                if (health != null)
                    health.AddHealth(1);
                break;
            case PowerUpType.SpeedBoost:
                RestartRoutine(ref speedRoutine, SpeedBoostRoutine());
                break;
            case PowerUpType.RapidFire:
                RestartRoutine(ref rapidFireRoutine, RapidFireRoutine());
                break;
            case PowerUpType.Shield:
                RestartRoutine(ref shieldRoutine, ShieldRoutine());
                break;
            case PowerUpType.RocketAmmo:
                Shoot2D[] shooters = GetComponentsInChildren<Shoot2D>();
                foreach (Shoot2D shooter in shooters)
                {
                    if (shooter != null)
                        shooter.AddRocketAmmo(2);
                }
                break;
        }
    }

    public bool ConsumeShield()
    {
        if (Time.time >= shieldUntil)
            return false;

        GameFeelController.PlayerHit();
        return true;
    }

    void RestartRoutine(ref Coroutine routine, IEnumerator nextRoutine)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(nextRoutine);
    }

    IEnumerator SpeedBoostRoutine()
    {
        HumanMove move = GetComponent<HumanMove>();
        if (move == null)
            yield break;

        if (!speedBoostActive)
        {
            boostedMove = move;
            originalSpeed = move.speed;
            move.speed = originalSpeed * speedBoostMultiplier;
            speedBoostActive = true;
        }

        yield return new WaitForSeconds(powerUpDuration);

        if (boostedMove != null)
            boostedMove.speed = originalSpeed;

        speedBoostActive = false;
        boostedMove = null;
        speedRoutine = null;
    }

    IEnumerator RapidFireRoutine()
    {
        if (!rapidFireActive)
        {
            boostedShooters = GetComponentsInChildren<Shoot2D>();
            originalFireRates = new float[boostedShooters.Length];

            for (int i = 0; i < boostedShooters.Length; i++)
            {
                if (boostedShooters[i] == null)
                    continue;

                originalFireRates[i] = boostedShooters[i].fireRate;
                boostedShooters[i].fireRate *= rapidFireMultiplier;
            }

            rapidFireActive = true;
        }

        yield return new WaitForSeconds(powerUpDuration);

        if (boostedShooters != null && originalFireRates != null)
        {
            for (int i = 0; i < boostedShooters.Length; i++)
            {
                if (boostedShooters[i] != null)
                    boostedShooters[i].fireRate = originalFireRates[i];
            }
        }

        rapidFireActive = false;
        boostedShooters = null;
        originalFireRates = null;
        rapidFireRoutine = null;
    }

    IEnumerator ShieldRoutine()
    {
        shieldUntil = Time.time + powerUpDuration;
        ShowShieldVisual();

        while (Time.time < shieldUntil)
            yield return null;

        HideShieldVisual();
        shieldRoutine = null;
    }

    void ShowShieldVisual()
    {
        if (shieldVisual == null)
        {
            shieldVisual = new GameObject("ActiveShieldVisual");
            shieldVisual.transform.SetParent(transform, false);
            shieldVisual.transform.localPosition = Vector3.zero;
            shieldVisual.transform.localScale = Vector3.one * 1.65f;

            SpriteRenderer renderer = shieldVisual.AddComponent<SpriteRenderer>();
            renderer.sprite = GetShieldSprite();
            renderer.color = new Color(0.28f, 0.85f, 1f, 0.38f);
            renderer.sortingOrder = 80;
        }

        shieldVisual.SetActive(true);
    }

    void HideShieldVisual()
    {
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    Sprite GetShieldSprite()
    {
        if (shieldSprite != null)
            return shieldSprite;

        Texture2D texture = new Texture2D(64, 64);
        Vector2 center = new Vector2(31.5f, 31.5f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float ring = Mathf.InverseLerp(31.5f, 26f, distance);
                float alpha = distance <= 31f ? Mathf.Clamp01(ring) * 0.7f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        shieldSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 32f);
        return shieldSprite;
    }
}
