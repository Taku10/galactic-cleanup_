using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CrosshairController2D : MonoBehaviour
{
    [Header("References")]
    public Transform player;            // drag player/ship here
    public Rigidbody2D playerRb;        // drag player RB here
    public Shoot2D shooter;             // drag Shoot2D here
    public SpriteRenderer crosshairSR;  // drag Crosshair SpriteRenderer
    public SpriteRenderer hitMarkerSR;  // drag HitMarker SpriteRenderer

    [Header("Hover Detection")]
    public LayerMask enemyMask;         // set Enemy layer here
    public Color normalColor = Color.white;
    public Color hoverColor = Color.red;

    [Header("Crosshair Scaling")]
    public float baseScale = 1f;
    public float moveScaleBoost = 0.25f;
    public float spreadScaleBoost = 0.08f;  // scale per degree spread
    public float smooth = 15f;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (!crosshairSR) crosshairSR = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        FollowMouse();
        HoverColor();
        ScaleByMoveAndSpread();
    }

    void FollowMouse()
    {
        if (cam == null || Mouse.current == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 screenPoint = new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z);
        Vector3 mouseWorld = cam.ScreenToWorldPoint(screenPoint);
        mouseWorld.z = 0f;

        transform.position = mouseWorld;
        transform.rotation = Quaternion.identity;
    }

    void HoverColor()
    {
        // checks if crosshair is over an enemy collider
        Collider2D hit = Physics2D.OverlapPoint(transform.position, enemyMask);
        crosshairSR.color = (hit != null) ? hoverColor : normalColor;
    }

    void ScaleByMoveAndSpread()
    {
        float moveAmount = (playerRb != null) ? playerRb.linearVelocity.magnitude : 0f;
        float moveBoost = Mathf.Clamp01(moveAmount / 6f) * moveScaleBoost;

        float spread = (shooter != null) ? shooter.CurrentSpread : 0f;
        float spreadBoost = spread * spreadScaleBoost;

        float target = baseScale + moveBoost + spreadBoost;

        Vector3 targetScale = Vector3.one * target;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, smooth * Time.deltaTime);
    }

    public void ShowHitMarker(float duration = 0.06f)
    {
        if (hitMarkerSR == null) return;
        StopAllCoroutines();
        StartCoroutine(HitFlash(duration));
    }

    IEnumerator HitFlash(float duration)
    {
        hitMarkerSR.enabled = true;
        yield return new WaitForSeconds(duration);
        hitMarkerSR.enabled = false;
    }

    void OnEnable()
{
    Bullet2D.OnEnemyHit += HandleHit;
}

void OnDisable()
{
    Bullet2D.OnEnemyHit -= HandleHit;
}

void HandleHit()
{
    ShowHitMarker();
}
}