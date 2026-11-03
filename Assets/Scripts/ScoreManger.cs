using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    public int Score { get; private set; }
    public int HighScore { get; private set; }

    const string HIGH_SCORE_KEY = "HIGH_SCORE";

    void Awake()
    {
        Instance = this;

        HighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateUI();
    }

    public void AddPoint(int amount = 1)
    {
        Score += amount;

        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, HighScore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "SCORE\n" + Score.ToString("0000");
        if (highScoreText != null) highScoreText.text = "BEST\n" + HighScore.ToString("0000");
        InGameUIStyler.Instance?.RefreshScore(Score, HighScore);
    }
}
