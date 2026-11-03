using System.Collections;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Throw Attack")]
    public float throwInterval = 2.4f;
    public float throwSpeed = 9f;
    public float thrownObjectSize = 0.7f;
    public Color thrownObjectColor = new Color(0.8f, 0.08f, 0.13f, 1f);

    [Header("Gun Freeze")]
    public float freezeInterval = 8f;
    public float freezeDuration = 5f;

    Transform player;
    float nextThrowTime;
    float nextFreezeTime;
    Sprite projectileSprite;
    bool phaseTwo;

    void Start()
    {
        ResolvePlayer();
        nextThrowTime = Time.time + 1.5f;
        nextFreezeTime = Time.time + 4f;
    }

    void Update()
    {
        if (GamOverManger.IsGameOver)
            return;

        if (player == null)
            ResolvePlayer();

        if (player == null)
            return;

        if (Time.time >= nextThrowTime)
        {
            ThrowObjectAtPlayer();
            nextThrowTime = Time.time + throwInterval;
        }

        if (Time.time >= nextFreezeTime)
        {
            FreezePlayerGuns();
            nextFreezeTime = Time.time + freezeInterval;
        }
    }

    void ThrowObjectAtPlayer()
    {
        if (player == null)
            ResolvePlayer();

        if (player == null)
            return;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        GameObject thrown = new GameObject("BossThrownObject");
        thrown.transform.position = transform.position + (Vector3)(direction * 1.6f);
        thrown.transform.localScale = Vector3.one * thrownObjectSize;

        SpriteRenderer renderer = thrown.AddComponent<SpriteRenderer>();
        renderer.sprite = GetProjectileSprite();
        renderer.color = thrownObjectColor;
        renderer.sortingOrder = 35;

        Rigidbody2D rb = thrown.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = direction * throwSpeed;

        CircleCollider2D collider = thrown.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        thrown.AddComponent<BossThrownObject>();
    }

    public void EnterPhaseTwo()
    {
        if (phaseTwo)
            return;

        phaseTwo = true;
        throwInterval = Mathf.Max(0.85f, throwInterval * 0.55f);
        throwSpeed *= 1.25f;
        thrownObjectSize *= 1.15f;
        freezeInterval = Mathf.Max(4.5f, freezeInterval * 0.7f);
        thrownObjectColor = new Color(1f, 0.22f, 0.06f, 1f);

        StartCoroutine(PhaseTwoBurst());
    }

    IEnumerator PhaseTwoBurst()
    {
        for (int i = 0; i < 4; i++)
        {
            ThrowObjectAtPlayer();
            yield return new WaitForSeconds(0.18f);
        }
    }

    void FreezePlayerGuns()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.ShowBossAnnouncement("BOSS:\nYOUR GUNS ARE FROZEN NOW HAHA!", 2f);

        Shoot2D[] shooters = FindObjectsByType<Shoot2D>(FindObjectsSortMode.None);
        foreach (Shoot2D shooter in shooters)
        {
            if (shooter != null && shooter.enabled)
                shooter.FreezeForSeconds(freezeDuration);
        }
    }

    void ResolvePlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    Sprite GetProjectileSprite()
    {
        if (projectileSprite != null)
            return projectileSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        projectileSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        return projectileSprite;
    }
}
