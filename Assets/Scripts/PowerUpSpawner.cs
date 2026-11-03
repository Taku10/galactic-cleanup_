using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    public float dropChance = 0.14f;
    public float rocketAmmoDropChance = 0.22f;
    public float spawnJitter = 0.45f;

    Sprite pickupSprite;

    void Awake()
    {
        Instance = this;
    }

    public static void TrySpawn(Vector3 position)
    {
        if (Instance == null)
        {
            GameObject spawnerObject = new GameObject("PowerUpSpawner");
            Instance = spawnerObject.AddComponent<PowerUpSpawner>();
        }

        Instance.TrySpawnInternal(position);
    }

    public static void TrySpawnRocketAmmo(Vector3 position)
    {
        if (Instance == null)
        {
            GameObject spawnerObject = new GameObject("PowerUpSpawner");
            Instance = spawnerObject.AddComponent<PowerUpSpawner>();
        }

        Instance.TrySpawnRocketAmmoInternal(position);
    }

    void TrySpawnInternal(Vector3 position)
    {
        if (Random.value > dropChance)
            return;

        PowerUpType type = (PowerUpType)Random.Range(0, 4);
        Spawn(type, position + (Vector3)(Random.insideUnitCircle * spawnJitter));
    }

    void TrySpawnRocketAmmoInternal(Vector3 position)
    {
        if (Random.value > rocketAmmoDropChance)
            return;

        Spawn(PowerUpType.RocketAmmo, position + (Vector3)(Random.insideUnitCircle * spawnJitter));
    }

    void Spawn(PowerUpType type, Vector3 position)
    {
        GameObject pickup = new GameObject(type + "PowerUp");
        pickup.transform.position = position;
        pickup.transform.localScale = Vector3.one * 0.8f;

        SpriteRenderer renderer = pickup.AddComponent<SpriteRenderer>();
        renderer.sprite = GetPickupSprite();
        renderer.color = GetColor(type);
        renderer.sortingOrder = 45;

        CircleCollider2D collider = pickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        Rigidbody2D rb = pickup.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        PowerUpPickup powerUp = pickup.AddComponent<PowerUpPickup>();
        powerUp.type = type;
    }

    Color GetColor(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Heal:
                return new Color(0.15f, 1f, 0.35f, 1f);
            case PowerUpType.SpeedBoost:
                return new Color(0.1f, 0.75f, 1f, 1f);
            case PowerUpType.RapidFire:
                return new Color(1f, 0.75f, 0.12f, 1f);
            case PowerUpType.Shield:
                return new Color(0.72f, 0.35f, 1f, 1f);
            case PowerUpType.RocketAmmo:
                return new Color(1f, 0.32f, 0.06f, 1f);
            default:
                return Color.white;
        }
    }

    Sprite GetPickupSprite()
    {
        if (pickupSprite != null)
            return pickupSprite;

        Texture2D texture = new Texture2D(32, 32);
        Vector2 center = new Vector2(15.5f, 15.5f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance < 14f ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        pickupSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 32f);
        return pickupSprite;
    }
}
