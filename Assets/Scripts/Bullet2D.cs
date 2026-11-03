using UnityEngine;
public class Bullet2D : MonoBehaviour
{
    public static System.Action OnEnemyHit;

    public float speed = 14f;
    public int damage = 1;
    public float lifeTime = 2f;
  

    Rigidbody2D rb;

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
        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Enemy"))
    {
        ZombieHealth hp = other.GetComponent<ZombieHealth>();
        if (hp == null) hp = other.GetComponentInParent<ZombieHealth>();

        if (hp != null)
            hp.TakeDamage(damage);

        Destroy(gameObject);
    }
    else if (other.CompareTag("Boss"))
    {
        BossHealth boss = other.GetComponent<BossHealth>();
        if (boss == null) boss = other.GetComponentInParent<BossHealth>();

        if (boss != null)
            boss.TakeDamage(damage);

        Destroy(gameObject);
    }
    else if (other.CompareTag("Wall"))
    {
        WallHealth wall = other.GetComponent<WallHealth>();
        if (wall == null) wall = other.GetComponentInParent<WallHealth>();

        if (wall != null)
            wall.TakeDamage(damage);

        Destroy(gameObject);
    }
}
}
