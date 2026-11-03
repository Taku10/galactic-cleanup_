using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossChase : MonoBehaviour
{
    public float speed = 3.5f;

    Rigidbody2D rb;
    Transform player;
    SpriteRenderer spriteRenderer;
    bool phaseTwo;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        ResolvePlayer();
    }

    void FixedUpdate()
    {
        if (GamOverManger.IsGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (player == null)
            ResolvePlayer();

        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = direction * speed;
        FacePlayer(direction);
    }

    void FacePlayer(Vector2 direction)
    {
        if (spriteRenderer == null || Mathf.Abs(direction.x) < 0.01f)
            return;

        spriteRenderer.flipX = direction.x < 0f;
    }

    void ResolvePlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    public void EnterPhaseTwo()
    {
        if (phaseTwo)
            return;

        phaseTwo = true;
        speed *= 1.35f;
    }
}
