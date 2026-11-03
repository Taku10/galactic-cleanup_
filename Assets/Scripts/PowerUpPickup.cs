using UnityEngine;

public enum PowerUpType
{
    Heal,
    SpeedBoost,
    RapidFire,
    Shield,
    RocketAmmo
}

[RequireComponent(typeof(Collider2D))]
public class PowerUpPickup : MonoBehaviour
{
    public PowerUpType type;
    public float lifeTime = 12f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Rotate(0f, 0f, 120f * Time.deltaTime);
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.08f;
        transform.localScale = Vector3.one * pulse;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerPowerups powerups = other.GetComponent<PlayerPowerups>();
        if (powerups == null)
            powerups = other.gameObject.AddComponent<PlayerPowerups>();

        powerups.Apply(type);
        Destroy(gameObject);
    }
}
