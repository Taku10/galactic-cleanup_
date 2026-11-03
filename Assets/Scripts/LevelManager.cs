using UnityEngine;
using TMPro;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Levels")]
    public int currentLevel = 1;
    public int maxLevel = 5;
    public int killsThisLevel = 0;

    public int level1KillsToBoss = 6;
    public int level2KillsToBoss = 10;
    public int level3KillsToBoss = 14;
    public int level4KillsToBoss = 18;
    public int level5KillsToBoss = 22;

    [Header("Boss")]
    public GameObject bossPrefab;
    public Transform player;
    public float bossSpawnDistance = 10f;

    [Header("Boss Audio")]
    public AudioClip bossIntroClip;
    public AudioSource bossIntroSource;
    [Range(0f, 1f)] public float bossIntroVolume = 0.9f;

    [Header("Game Audio")]
    public AudioSource gameMusicSource;

    [Header("UI")]
    public TMP_Text levelMessageText;

    [Header("Environment")]
    public GameObject wallsRoot;

    bool bossSpawned = false;
    EnemySpawner[] enemySpawners;
    LevelObstacleController obstacleController;
    bool gameMusicPausedForBoss;

    public bool IsBossActive => bossSpawned;

    void Awake()
    {
        Instance = this;
        maxLevel = 5;
        currentLevel = Mathf.Clamp(currentLevel, 1, maxLevel);
        ApplyDefaultLevelTargets();
        enemySpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        obstacleController = GetComponent<LevelObstacleController>();
        if (obstacleController == null)
            obstacleController = gameObject.AddComponent<LevelObstacleController>();

        ResolveGameMusicSource();
        ResolvePlayerTransform();
    }

    void Start()
    {
        // Hide walls at start
        if (wallsRoot != null)
            wallsRoot.SetActive(false);

        ApplyLevelSetup();
    }

    public void EnemyKilled()
    {
        if (bossSpawned) return;

        killsThisLevel++;

        if (killsThisLevel >= GetKillsNeeded())
        {
            SpawnBoss();
        }
    }

    int GetKillsNeeded()
    {
        if (currentLevel == 1) return level1KillsToBoss;
        if (currentLevel == 2) return level2KillsToBoss;
        if (currentLevel == 3) return level3KillsToBoss;
        if (currentLevel == 4) return level4KillsToBoss;
        return level5KillsToBoss;
    }

    void ApplyDefaultLevelTargets()
    {
        level1KillsToBoss = 6;
        level2KillsToBoss = 10;
        level3KillsToBoss = 14;
        level4KillsToBoss = 18;
        level5KillsToBoss = 22;
    }

    void SpawnBoss()
    {
        if (bossSpawned) return;

        Transform playerTransform = ResolvePlayerTransform();
        if (playerTransform == null)
        {
            Debug.LogError("LevelManager: Cannot spawn boss because no Player transform was found. Assign Player in the Inspector or tag your player object as Player.");
            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogError("LevelManager: Cannot spawn boss because bossPrefab is not assigned.");
            return;
        }

        // Remove all small enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        bossSpawned = true;
        SetEnemySpawnersEnabled(false);
        Vector3 spawnPos = GetBossSpawnPosition(playerTransform);

        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        BossHealth bossHealth = boss.GetComponent<BossHealth>();
        if (bossHealth != null)
            bossHealth.maxHealth += (currentLevel - 1) * 6;

        Rigidbody2D bossRigidbody = boss.GetComponent<Rigidbody2D>();
        if (bossRigidbody != null)
        {
            bossRigidbody.gravityScale = 0f;
            bossRigidbody.linearVelocity = Vector2.zero;
            bossRigidbody.angularVelocity = 0f;
            bossRigidbody.freezeRotation = true;
        }

        // Optional intro animation
        BossIntro intro = boss.GetComponent<BossIntro>();
        if (intro != null)
            intro.PlayIntro();

        PauseGameAudioForBossIntro();
        PlayBossIntroSound();
        ShowLevelMessage("LEVEL " + currentLevel + "\nBOSS INCOMING!", 2.5f);
    }

    void PauseGameAudioForBossIntro()
    {
        ResolveGameMusicSource();

        if (gameMusicSource == null || !gameMusicSource.isPlaying)
            return;

        gameMusicSource.Pause();
        gameMusicPausedForBoss = true;
    }

    void PlayBossIntroSound()
    {
        if (bossIntroClip == null)
            bossIntroClip = Resources.Load<AudioClip>("boss_intro");

        if (bossIntroClip == null)
        {
            Debug.LogWarning("LevelManager: boss_intro audio clip was not found in Assets/Resources.");
            return;
        }

        if (bossIntroSource == null)
        {
            GameObject audioObject = new GameObject("BossIntroAudio");
            bossIntroSource = audioObject.AddComponent<AudioSource>();
            bossIntroSource.playOnAwake = false;
            bossIntroSource.loop = false;
            bossIntroSource.spatialBlend = 0f;
        }

        bossIntroSource.PlayOneShot(bossIntroClip, bossIntroVolume);
    }

    void ResumeGameAudioAfterBoss()
    {
        if (!gameMusicPausedForBoss)
            return;

        ResolveGameMusicSource();

        if (gameMusicSource != null)
            gameMusicSource.UnPause();

        gameMusicPausedForBoss = false;
    }

    void ResolveGameMusicSource()
    {
        if (gameMusicSource != null)
            return;

        GameObject audioManager = GameObject.Find("AudioManager");
        if (audioManager != null)
            gameMusicSource = audioManager.GetComponent<AudioSource>();
    }

    void SetEnemySpawnersEnabled(bool enabled)
    {
        if (enemySpawners == null || enemySpawners.Length == 0)
            enemySpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);

        foreach (EnemySpawner spawner in enemySpawners)
        {
            if (spawner == null)
                continue;

            if (enabled)
                spawner.StartSpawning();
            else
                spawner.StopSpawning();
        }
    }

    Vector3 GetBossSpawnPosition(Transform playerTransform)
    {
        Vector2 direction = Random.insideUnitCircle.normalized;
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        Vector2 spawnOffset = direction * bossSpawnDistance;
        return playerTransform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0f);
    }

    Transform ResolvePlayerTransform()
    {
        if (player != null)
            return player;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            return player;
        }

        HumanMove humanMove = FindFirstObjectByType<HumanMove>();
        if (humanMove != null)
        {
            player = humanMove.transform;
            return player;
        }

        return null;
    }

    public void BossDefeated()
    {
        if (!bossSpawned) return;

        ResumeGameAudioAfterBoss();
        bossSpawned = false;

        if (currentLevel >= maxLevel)
        {
            if (obstacleController != null)
                obstacleController.Clear();

            if (wallsRoot != null)
                wallsRoot.SetActive(false);

            ShowLevelMessage("GALAXY CLEANED!", 4f);
            return;
        }

        currentLevel++;
        killsThisLevel = 0;
        SetEnemySpawnersEnabled(true);
        ApplyLevelSetup();
    }

    void ApplyLevelSetup()
    {
        Transform playerTransform = ResolvePlayerTransform();

        if (wallsRoot != null)
        {
            bool useScrapCage = currentLevel == 3;
            wallsRoot.SetActive(useScrapCage);
            if (useScrapCage)
                CenterWallsOnPlayer();
        }

        if (obstacleController != null)
            obstacleController.BuildForLevel(currentLevel, playerTransform);

        ApplySpawnerPacing();
        ShowLevelMessage(GetLevelIntro(), 4.2f);
    }

    void ApplySpawnerPacing()
    {
        if (enemySpawners == null || enemySpawners.Length == 0)
            enemySpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);

        float interval = Mathf.Max(0.8f, 2.75f - (currentLevel - 1) * 0.4f);
        float radius = 13f + currentLevel * 1.25f;
        float minDistance = 7.5f + (currentLevel - 1) * 0.45f;

        EnemyChase.globalSpeedBonus = (currentLevel - 1) * 0.28f;

        foreach (EnemySpawner spawner in enemySpawners)
        {
            if (spawner == null)
                continue;

            spawner.spawnInterval = interval;
            spawner.spawnRadius = radius;
            spawner.minSpawnDistance = Mathf.Min(10f, minDistance);
        }
    }

    string GetLevelIntro()
    {
        return "LEVEL " + currentLevel;
    }

    void CenterWallsOnPlayer()
    {
        Transform playerTransform = ResolvePlayerTransform();
        if (playerTransform == null || wallsRoot == null)
            return;

        Vector3 wallPosition = wallsRoot.transform.position;
        wallPosition.x = playerTransform.position.x;
        wallPosition.y = playerTransform.position.y;
        wallsRoot.transform.position = wallPosition;
    }

    void ShowLevelMessage(string message, float duration = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(LevelMessageRoutine(message, duration));
    }

    public void ShowBossAnnouncement(string message, float duration = 2f)
    {
        ShowLevelMessage(message, duration);
    }

    IEnumerator LevelMessageRoutine(string message, float duration)
    {
        if (levelMessageText == null) yield break;

        levelMessageText.gameObject.SetActive(true);
        levelMessageText.text = message;
        levelMessageText.alignment = TextAlignmentOptions.Center;
        levelMessageText.enableWordWrapping = true;
        levelMessageText.overflowMode = TextOverflowModes.Overflow;
        levelMessageText.fontSize = message.Contains("\n") ? 52f : 86f;
        levelMessageText.lineSpacing = 18f;

        RectTransform rect = levelMessageText.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = message.Contains("\n")
                ? new Vector2(1200f, 300f)
                : new Vector2(900f, 180f);
        }

        yield return new WaitForSeconds(duration);

        levelMessageText.gameObject.SetActive(false);
    }
}
