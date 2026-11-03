using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Rocket2D : MonoBehaviour
{
    public float speed = 11f;
    public int damage = 9;
    public float splashRadius = 2.4f;
    public float lifeTime = 3f;

    static Sprite rocketSprite;
    Rigidbody2D rb;
    bool exploded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Fire(Vector2 direction)
    {
        Vector2 normalized = direction.normalized;
        rb.linearVelocity = normalized * speed;

        if (normalized.sqrMagnitude > 0.01f)
            transform.up = normalized;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded)
            return;

        if (other.CompareTag("Player"))
            return;

        if (other.CompareTag("Enemy") || other.CompareTag("Boss") || other.CompareTag("Wall"))
            Explode();
    }

    void Explode()
    {
        exploded = true;
        ExplosionVfx.PlayEnemy(transform.position);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.CompareTag("Boss"))
            {
                BossHealth boss = hit.GetComponent<BossHealth>();
                if (boss == null) boss = hit.GetComponentInParent<BossHealth>();
                if (boss != null) boss.TakeDamage(damage);
            }
            else if (hit.CompareTag("Enemy"))
            {
                ZombieHealth enemy = hit.GetComponent<ZombieHealth>();
                if (enemy == null) enemy = hit.GetComponentInParent<ZombieHealth>();
                if (enemy != null) enemy.TakeDamage(damage);
            }
            else if (hit.CompareTag("Wall"))
            {
                WallHealth wall = hit.GetComponent<WallHealth>();
                if (wall == null) wall = hit.GetComponentInParent<WallHealth>();
                if (wall != null) wall.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    public static Sprite GetRocketSprite()
    {
        if (rocketSprite != null)
            return rocketSprite;

        Texture2D texture = new Texture2D(32, 48);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float nx = Mathf.Abs((x - 15.5f) / 15.5f);
                float ny = y / 47f;
                bool body = nx < Mathf.Lerp(0.34f, 0.12f, ny) && y > 7;
                bool flame = y <= 9 && nx < Mathf.Lerp(0.08f, 0.3f, y / 9f);
                Color color = new Color(1f, 1f, 1f, 0f);

                if (body)
                    color = ny > 0.8f ? new Color(1f, 0.92f, 0.35f, 1f) : Color.white;
                else if (flame)
                    color = new Color(1f, 0.28f, 0.06f, 1f);

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        rocketSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 32f);
        return rocketSprite;
    }
}
