using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public EnemySpawner spawner;      
    public float intervalStart = 2.75f;
    public float intervalMin = 0.8f;
    public float intervalDecreasePerLevel = 0.4f;

    public float enemySpeedIncreasePerLevel = 0.28f;

    int lastAppliedLevel = -1;

    void Start()
    {
        ApplyLevelDifficulty();
    }

    void Update()
    {
        ApplyLevelDifficulty();
    }

    void ApplyLevelDifficulty()
    {
        int level = LevelManager.Instance != null ? LevelManager.Instance.currentLevel : 1;
        if (level == lastAppliedLevel)
            return;

        if (spawner != null)
            spawner.spawnInterval = Mathf.Max(intervalMin, intervalStart - intervalDecreasePerLevel * (level - 1));

        EnemyChase.globalSpeedBonus = enemySpeedIncreasePerLevel * (level - 1);
        lastAppliedLevel = level;
    }
}
