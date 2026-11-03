using UnityEngine;
using UnityEngine.InputSystem;

public class Shoot2D : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip shootClip;
    [Range(0f, 1f)] public float shootVolume = 0.6f;
    public Bullet2D bulletPrefab;
    public Transform firePoint;
    public float fireRate = 8f;

    [Header("Rockets")]
    public int startingRocketAmmo = 3;
    public int maxRocketAmmo = 8;
    public float rocketFireCooldown = 0.75f;
    public float rocketSpeed = 11f;
    public int rocketDamage = 9;
    public float rocketSplashRadius = 2.4f;
    public float rocketSize = 0.75f;
    public Color rocketColor = new Color(1f, 0.36f, 0.08f, 1f);

    [Header("Spread / Recoil")]
    public float baseSpread = 0.5f;          // degrees
    public float maxSpread = 8f;             // degrees
    public float spreadIncreasePerShot = 0.8f;
    public float spreadDecayPerSec = 6f;

    public float CurrentSpread { get; private set; }

    float nextTime;
    float nextRocketTime;
    float frozenUntil;
    int rocketAmmo;

    public bool IsFrozen => Time.time < frozenUntil;

    public void FreezeForSeconds(float seconds)
    {
        frozenUntil = Mathf.Max(frozenUntil, Time.time + seconds);
    }

    void Start()
    {
        CurrentSpread = baseSpread;
        rocketAmmo = Mathf.Clamp(startingRocketAmmo, 0, maxRocketAmmo);
        InGameUIStyler.Instance?.UpdateRockets(rocketAmmo, maxRocketAmmo);
    }

    void Update()
    {
        if (GamOverManger.IsGameOver) return;

        // decay spread back down over time
        CurrentSpread = Mathf.MoveTowards(CurrentSpread, baseSpread, spreadDecayPerSec * Time.deltaTime);

        if (IsFrozen)
            return;

        if (WantsRocketShot() && Time.time >= nextRocketTime)
        {
            nextRocketTime = Time.time + rocketFireCooldown;
            ShootRocket();
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed && Time.time >= nextTime)
        {
            nextTime = Time.time + (1f / fireRate);
            Shoot();
        }
    }

    public void AddRocketAmmo(int amount)
    {
        rocketAmmo = Mathf.Clamp(rocketAmmo + amount, 0, maxRocketAmmo);
        InGameUIStyler.Instance?.UpdateRockets(rocketAmmo, maxRocketAmmo);
    }

    bool WantsRocketShot()
    {
        bool mouseRocket = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool keyboardRocket = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        return mouseRocket || keyboardRocket;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // add recoil
        CurrentSpread = Mathf.Min(maxSpread, CurrentSpread + spreadIncreasePerShot);

        // random angle offset
        float angleOffset = Random.Range(-CurrentSpread, CurrentSpread);

        Vector2 dir = firePoint.up;
        dir = Quaternion.Euler(0f, 0f, angleOffset) * dir;

        Bullet2D bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.Fire(dir);

        if (audioSource != null && shootClip != null)
            audioSource.PlayOneShot(shootClip, shootVolume);
    }

    void ShootRocket()
    {
        if (firePoint == null || rocketAmmo <= 0)
            return;

        rocketAmmo--;
        InGameUIStyler.Instance?.UpdateRockets(rocketAmmo, maxRocketAmmo);

        GameObject rocketObject = new GameObject("PlayerRocket");
        rocketObject.transform.position = firePoint.position;
        rocketObject.transform.rotation = firePoint.rotation;
        rocketObject.transform.localScale = Vector3.one * rocketSize;

        SpriteRenderer renderer = rocketObject.AddComponent<SpriteRenderer>();
        renderer.sprite = Rocket2D.GetRocketSprite();
        renderer.color = rocketColor;
        renderer.sortingOrder = 34;

        Rigidbody2D rocketBody = rocketObject.AddComponent<Rigidbody2D>();
        rocketBody.gravityScale = 0f;
        rocketBody.freezeRotation = true;
        rocketBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D collider = rocketObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.45f;

        Rocket2D rocket = rocketObject.AddComponent<Rocket2D>();
        rocket.damage = rocketDamage;
        rocket.speed = rocketSpeed;
        rocket.splashRadius = rocketSplashRadius;
        rocket.Fire(firePoint.up);

        if (audioSource != null && shootClip != null)
            audioSource.PlayOneShot(shootClip, shootVolume);
    }
}
