using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public static CameraFollow2D Instance { get; private set; }

    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float smoothTime = 0.15f;
    public bool snapOnStart = true;

    Vector3 velocity;
    float shakeTimer;
    float shakeDuration;
    float shakeStrength;

    void Awake()
    {
        Instance = this;
        ResolveTarget();
    }

    void Start()
    {
        if (snapOnStart && target != null)
            transform.position = GetTargetPosition();
    }

    void LateUpdate()
    {
        if (target == null)
            ResolveTarget();

        if (target == null)
            return;

        Vector3 targetPosition = GetTargetPosition();
        Vector3 shakeOffset = GetShakeOffset();
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition + shakeOffset, ref velocity, smoothTime);
    }

    public static void Shake(float strength, float duration)
    {
        if (Instance != null)
            Instance.StartShake(strength, duration);
    }

    void StartShake(float strength, float duration)
    {
        shakeStrength = Mathf.Max(shakeStrength, strength);
        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeTimer = Mathf.Max(shakeTimer, duration);
    }

    Vector3 GetShakeOffset()
    {
        if (shakeTimer <= 0f)
            return Vector3.zero;

        shakeTimer -= Time.unscaledDeltaTime;
        float percent = shakeDuration > 0f ? Mathf.Clamp01(shakeTimer / shakeDuration) : 0f;
        Vector2 offset2D = Random.insideUnitCircle * shakeStrength * percent;
        return new Vector3(offset2D.x, offset2D.y, 0f);
    }

    Vector3 GetTargetPosition()
    {
        return new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);
    }

    void ResolveTarget()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
            return;
        }

        HumanMove humanMove = FindFirstObjectByType<HumanMove>();
        if (humanMove != null)
            target = humanMove.transform;
    }
}
