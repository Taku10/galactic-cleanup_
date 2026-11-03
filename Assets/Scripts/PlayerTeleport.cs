using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerTeleport : MonoBehaviour
{
    public float teleportDistance = 8f;
    public float cooldown = 2f;
    public float playerClearance = 1.1f;
    public float hostileClearance = 4f;
    public int searchAttempts = 28;

    Rigidbody2D rb;
    HumanMove move;
    float nextTeleportTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        move = GetComponent<HumanMove>();
    }

    void Update()
    {
        if (GamOverManger.IsGameOver)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.tKey.wasPressedThisFrame)
            return;

        if (Time.time < nextTeleportTime)
            return;

        if (TryFindSafePosition(out Vector2 safePosition))
        {
            rb.position = safePosition;
            rb.linearVelocity = Vector2.zero;
            transform.position = new Vector3(safePosition.x, safePosition.y, transform.position.z);
            nextTeleportTime = Time.time + cooldown;

            if (LevelManager.Instance != null)
                LevelManager.Instance.ShowBossAnnouncement("TELEPORTED!", 1.1f);
        }
    }

    bool TryFindSafePosition(out Vector2 safePosition)
    {
        Vector2 origin = rb.position;
        Vector2 preferredDirection = GetPreferredDirection();

        for (int i = 0; i < searchAttempts; i++)
        {
            float t = searchAttempts <= 1 ? 0f : i / (float)(searchAttempts - 1);
            float angle = (i == 0 ? 0f : 137.5f * i) + t * 35f;
            Vector2 direction = Rotate(preferredDirection, angle);
            float distance = Mathf.Lerp(teleportDistance, teleportDistance * 1.75f, t);
            Vector2 candidate = origin + direction * distance;

            if (IsSafe(candidate))
            {
                safePosition = candidate;
                return true;
            }
        }

        safePosition = origin;
        return false;
    }

    Vector2 GetPreferredDirection()
    {
        if (move != null && move.MoveInput.sqrMagnitude > 0.01f)
            return move.MoveInput.normalized;

        Vector2 awayFromDanger = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, hostileClearance * 2.5f);
        foreach (Collider2D collider in nearby)
        {
            if (collider == null || !IsHostile(collider))
                continue;

            Vector2 away = (Vector2)transform.position - (Vector2)collider.transform.position;
            float distance = Mathf.Max(0.2f, away.magnitude);
            awayFromDanger += away.normalized / distance;
        }

        if (awayFromDanger.sqrMagnitude > 0.01f)
            return awayFromDanger.normalized;

        return Random.insideUnitCircle.normalized;
    }

    bool IsSafe(Vector2 position)
    {
        Collider2D[] blockers = Physics2D.OverlapCircleAll(position, playerClearance);
        foreach (Collider2D blocker in blockers)
        {
            if (blocker == null || blocker.transform.IsChildOf(transform))
                continue;

            if (blocker.CompareTag("Wall") || blocker.CompareTag("Enemy") || blocker.CompareTag("Boss"))
                return false;
        }

        Collider2D[] threats = Physics2D.OverlapCircleAll(position, hostileClearance);
        foreach (Collider2D threat in threats)
        {
            if (threat == null || threat.transform.IsChildOf(transform))
                continue;

            if (IsHostile(threat))
                return false;
        }

        return true;
    }

    bool IsHostile(Collider2D collider)
    {
        return collider.CompareTag("Enemy") || collider.CompareTag("Boss");
    }

    Vector2 Rotate(Vector2 vector, float degrees)
    {
        if (vector.sqrMagnitude < 0.01f)
            vector = Vector2.right;

        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos).normalized;
    }
}
