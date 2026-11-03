using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class HumanMove : MonoBehaviour
{
    public Vector2 MoveInput => move;
    public float speed = 6f;
    Rigidbody2D rb;
    Vector2 move;
    bool warnedMissingKeyboard;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (GetComponent<PlayerTeleport>() == null)
            gameObject.AddComponent<PlayerTeleport>();
    }

    void OnEnable()
    {
        if (rb != null)
            rb.simulated = true;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null)
        {
            if (!warnedMissingKeyboard)
            {
                Debug.LogWarning("HumanMove: No keyboard detected by the Input System.");
                warnedMissingKeyboard = true;
            }

            move = Vector2.zero;
            return;
        }

        warnedMissingKeyboard = false;

        float h =
            (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) -
            (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
        float v =
            (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) -
            (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
        move = new Vector2(h, v).normalized;
    }

    void FixedUpdate()
    {
        Vector2 delta = move * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + delta);
    }

    void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
