using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;

    public float spawnInterval = 2f;
    public float spawnRadius = 12f;
    public float minSpawnDistance = 6f;

    [Header("Enemy Music")]
    public AudioSource enemyMusicSource;
    public AudioClip enemyMusicClip;
    [Range(0f, 1f)] public float enemyMusicVolume = 0.65f;

    float timer;
    bool spawningEnabled = true;
    bool enemyMusicStarted;

    public void StopSpawning()
    {
        spawningEnabled = false;
        timer = 0f;
    }

    public void StartSpawning()
    {
        spawningEnabled = true;
        timer = 0f;
    }

    void Update()
    {
        if (!spawningEnabled)
            return;

        if (LevelManager.Instance != null && LevelManager.Instance.IsBossActive)
        {
            timer = 0f;
            return;
        }

        if (!player || !enemyPrefab) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;

            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.01f)
                direction = Vector2.right;

            Vector2 offset = direction * Random.Range(minSpawnDistance, spawnRadius);
            Vector3 spawnPos = player.position + new Vector3(offset.x, offset.y, 0);

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            StartEnemyMusic();
        }
    }

    void StartEnemyMusic()
    {
        if (enemyMusicStarted || enemyMusicClip == null)
            return;

        if (enemyMusicSource == null)
        {
            GameObject audioManager = GameObject.Find("AudioManager");
            if (audioManager != null)
                enemyMusicSource = audioManager.GetComponent<AudioSource>();
        }

        if (enemyMusicSource == null)
            enemyMusicSource = gameObject.AddComponent<AudioSource>();

        enemyMusicSource.clip = enemyMusicClip;
        enemyMusicSource.volume = enemyMusicVolume;
        enemyMusicSource.loop = true;
        enemyMusicSource.spatialBlend = 0f;
        enemyMusicSource.Play();

        enemyMusicStarted = true;
    }
}
