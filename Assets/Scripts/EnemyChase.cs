using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : MonoBehaviour
{
    public float baseSpeed = 2.5f;
    public static float globalSpeedBonus = 0f;  // used for difficulty scaling

    [Header("Ranged Attack")]
    public float preferredShootDistance = 8f;
    public float tooCloseDistance = 4.8f;
    public float shootInterval = 2.2f;
    public float projectileSpeed = 8f;
    public float projectileDamage = 0.5f;
    public float projectileSize = 0.35f;
    public Color projectileColor = new Color(0.25f, 0.9f, 1f, 1f);
    public Color shootFlashColor = new Color(0.35f, 1f, 1f, 1f);

    Rigidbody2D rb;
    Transform player;
    SpriteRenderer spriteRenderer;
    Sprite projectileSprite;
    Vector3 originalScale;
    Color originalColor;
    float nextShootTime;
    bool animatingShot;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
        nextShootTime = Time.time + Random.Range(0.7f, shootInterval);
    }

    void FixedUpdate()
    {
        if (GamOverManger.IsGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!player) return;

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        Vector2 dir = toPlayer.normalized;
        float distance = toPlayer.magnitude;

        float finalSpeed = baseSpeed + globalSpeedBonus;
        Vector2 velocity = Vector2.zero;

        if (distance > preferredShootDistance)
            velocity = dir * finalSpeed;
        else if (distance < tooCloseDistance)
            velocity = -dir * finalSpeed * 0.65f;

        rb.linearVelocity = velocity;
        FacePlayer(dir);
    }

    void Update()
    {
        if (!player || GamOverManger.IsGameOver)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= preferredShootDistance + 0.75f && Time.time >= nextShootTime)
        {
            StartCoroutine(ShootRoutine());
            nextShootTime = Time.time + shootInterval + Random.Range(-0.25f, 0.35f);
        }
    }

    IEnumerator ShootRoutine()
    {
        if (animatingShot)
            yield break;

        animatingShot = true;

        if (spriteRenderer != null)
            spriteRenderer.color = shootFlashColor;

        transform.localScale = originalScale * 1.18f;
        yield return new WaitForSeconds(0.12f);

        transform.localScale = originalScale * 0.92f;
        yield return new WaitForSeconds(0.08f);

        transform.localScale = originalScale;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        FireProjectile();
        animatingShot = false;
    }

    void FireProjectile()
    {
        if (player == null)
            return;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        GameObject projectile = new GameObject("EnemyProjectile");
        projectile.transform.position = transform.position + (Vector3)(direction * 0.75f);
        projectile.transform.localScale = Vector3.one * projectileSize;

        SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
        renderer.sprite = GetProjectileSprite();
        renderer.color = projectileColor;
        renderer.sortingOrder = 32;

        Rigidbody2D projectileBody = projectile.AddComponent<Rigidbody2D>();
        projectileBody.gravityScale = 0f;
        projectileBody.freezeRotation = true;
        projectileBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        projectileBody.linearVelocity = direction * projectileSpeed;

        CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        EnemyProjectile enemyProjectile = projectile.AddComponent<EnemyProjectile>();
        enemyProjectile.damage = projectileDamage;
    }

    void FacePlayer(Vector2 direction)
    {
        if (spriteRenderer == null || Mathf.Abs(direction.x) < 0.01f)
            return;

        spriteRenderer.flipX = direction.x < 0f;
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
