using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIStyler : MonoBehaviour
{
    public static InGameUIStyler Instance { get; private set; }

    readonly Color panelColor = new Color(0.02f, 0.04f, 0.08f, 0.78f);
    readonly Color textColor = new Color(0.9f, 0.97f, 1f, 1f);
    readonly Color accentColor = new Color(0.16f, 0.72f, 0.86f, 1f);
    readonly Color dangerColor = new Color(0.9f, 0.08f, 0.12f, 1f);

    TMP_Text scoreText;
    TMP_Text highScoreText;
    TMP_Text livesText;
    TMP_Text rocketsText;
    TMP_Text bossText;
    Image bossFill;
    RectTransform bossFillRect;
    GameObject bossPanel;

    void Awake()
    {
        Instance = this;
        transform.localScale = Vector3.one;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    void Start()
    {
        BuildHud();
    }

    public void RefreshScore(int score, int highScore)
    {
        if (scoreText != null) scoreText.text = "SCORE\n" + score.ToString("0000");
        if (highScoreText != null) highScoreText.text = "BEST\n" + highScore.ToString("0000");
    }

    public void UpdateLives(int lives, int maxLives)
    {
        UpdateLives((float)lives, maxLives);
    }

    public void UpdateLives(float lives, float maxLives)
    {
        if (livesText == null)
            BuildHud();

        if (livesText != null)
            livesText.text = "LIVES\n" + FormatHealth(lives) + " / " + FormatHealth(maxLives);
    }

    public void UpdateRockets(int rockets, int maxRockets)
    {
        if (rocketsText == null)
            BuildHud();

        if (rocketsText != null)
            rocketsText.text = "ROCKETS\n" + rockets + " / " + maxRockets;
    }

    string FormatHealth(float value)
    {
        float clamped = Mathf.Max(0f, value);
        return Mathf.Approximately(clamped, Mathf.Round(clamped))
            ? Mathf.RoundToInt(clamped).ToString()
            : clamped.ToString("0.0");
    }

    public void ShowBossHealth(int currentHealth, int maxHealth)
    {
        if (bossPanel == null)
            BuildHud();

        if (bossPanel != null)
            bossPanel.SetActive(true);

        UpdateBossHealth(currentHealth, maxHealth);
    }

    public void UpdateBossHealth(int currentHealth, int maxHealth)
    {
        if (bossFill == null || bossText == null)
            BuildHud();

        float percent = maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;
        if (bossFill != null) bossFill.fillAmount = percent;
        if (bossFillRect != null)
        {
            bossFillRect.anchorMin = Vector2.zero;
            bossFillRect.anchorMax = new Vector2(percent, 1f);
            bossFillRect.offsetMin = Vector2.zero;
            bossFillRect.offsetMax = Vector2.zero;
        }
        if (bossText != null) bossText.text = "BOSS  " + Mathf.Max(0, currentHealth) + " / " + maxHealth;
    }

    public void HideBossHealth()
    {
        if (bossPanel != null)
            bossPanel.SetActive(false);
    }

    void BuildHud()
    {
        scoreText = FindText("ScoreText");
        highScoreText = FindText("HighScoreText");

        BuildHudBox("ScoreHudBox", new Vector2(28f, -24f), new Vector2(235f, 98f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        BuildHudBox("HighScoreHudBox", new Vector2(278f, -24f), new Vector2(235f, 98f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        BuildHudBox("LivesHudBox", new Vector2(-28f, -24f), new Vector2(250f, 98f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        BuildHudBox("RocketsHudBox", new Vector2(-293f, -24f), new Vector2(250f, 98f), new Vector2(1f, 1f), new Vector2(1f, 1f));

        StyleText(scoreText, new Vector2(42f, -31f), new Vector2(220f, 90f), TextAlignmentOptions.Left);
        StyleText(highScoreText, new Vector2(292f, -31f), new Vector2(220f, 90f), TextAlignmentOptions.Left);

        if (livesText == null)
        {
            livesText = CreateText("LivesText", transform);
            StyleText(livesText, new Vector2(-42f, -31f), new Vector2(230f, 90f), TextAlignmentOptions.Right);
            RectTransform livesRect = livesText.GetComponent<RectTransform>();
            livesRect.anchorMin = new Vector2(1f, 1f);
            livesRect.anchorMax = new Vector2(1f, 1f);
            livesRect.pivot = new Vector2(1f, 1f);
        }

        if (rocketsText == null)
        {
            rocketsText = CreateText("RocketsText", transform);
            StyleText(rocketsText, new Vector2(-307f, -31f), new Vector2(230f, 90f), TextAlignmentOptions.Right);
            RectTransform rocketsRect = rocketsText.GetComponent<RectTransform>();
            rocketsRect.anchorMin = new Vector2(1f, 1f);
            rocketsRect.anchorMax = new Vector2(1f, 1f);
            rocketsRect.pivot = new Vector2(1f, 1f);
        }

        if (bossPanel == null)
            BuildBossPanel();

        HideBossHealth();
    }

    GameObject BuildHudBox(string name, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
    {
        Transform existing = transform.Find(name);
        GameObject box = existing != null
            ? existing.gameObject
            : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        box.transform.SetParent(transform, false);
        box.transform.SetAsFirstSibling();

        RectTransform rect = box.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = box.GetComponent<Image>();
        image.color = panelColor;
        image.raycastTarget = false;

        Transform strip = box.transform.Find("AccentStrip");
        GameObject stripObject = strip != null
            ? strip.gameObject
            : new GameObject("AccentStrip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        stripObject.transform.SetParent(box.transform, false);
        RectTransform stripRect = stripObject.GetComponent<RectTransform>();
        stripRect.anchorMin = new Vector2(0f, 0f);
        stripRect.anchorMax = new Vector2(1f, 0f);
        stripRect.pivot = new Vector2(0.5f, 0f);
        stripRect.offsetMin = Vector2.zero;
        stripRect.offsetMax = new Vector2(0f, 6f);

        Image stripImage = stripObject.GetComponent<Image>();
        stripImage.color = accentColor;
        stripImage.raycastTarget = false;

        return box;
    }

    void BuildBossPanel()
    {
        bossPanel = new GameObject("BossHealthPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bossPanel.transform.SetParent(transform, false);

        RectTransform panelRect = bossPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -34f);
        panelRect.sizeDelta = new Vector2(760f, 94f);

        Image panelImage = bossPanel.GetComponent<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = false;

        GameObject fillBack = new GameObject("BossBarBack", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillBack.transform.SetParent(bossPanel.transform, false);
        RectTransform backRect = fillBack.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.04f, 0.16f);
        backRect.anchorMax = new Vector2(0.96f, 0.48f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;
        fillBack.GetComponent<Image>().color = new Color(0.08f, 0.02f, 0.04f, 0.95f);

        GameObject fillObject = new GameObject("BossBarFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObject.transform.SetParent(fillBack.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        bossFillRect = fillRect;
        bossFill = fillObject.GetComponent<Image>();
        bossFill.color = dangerColor;
        bossFill.type = Image.Type.Simple;
        bossFill.fillMethod = Image.FillMethod.Horizontal;
        bossFill.fillOrigin = 0;

        bossText = CreateText("BossHealthText", bossPanel.transform);
        RectTransform textRect = bossText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.5f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(0f, -4f);
        textRect.offsetMax = Vector2.zero;
        bossText.text = "BOSS";
        bossText.alignment = TextAlignmentOptions.Center;
        bossText.fontSize = 36f;
        bossText.fontStyle = FontStyles.Bold;
        bossText.color = textColor;
        bossText.raycastTarget = false;
    }

    void StyleText(TMP_Text text, Vector2 position, Vector2 size, TextAlignmentOptions alignment)
    {
        if (text == null) return;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        text.color = textColor;
        text.fontSize = 30f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.lineSpacing = -10f;
        text.raycastTarget = false;
    }

    TMP_Text FindText(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == objectName)
                return child.GetComponent<TMP_Text>();
        }

        return null;
    }

    TMP_Text CreateText(string name, Transform parent)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.color = textColor;
        text.fontSize = 30f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }
}
