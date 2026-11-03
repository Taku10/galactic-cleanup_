using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class GamOverManger : MonoBehaviour
{
    public static GamOverManger Instance;
    public static bool IsGameOver { get; private set; }

    public GameObject restartButton; // drag RestartButton here
    public GameObject wallsRoot;     // drag Walls here
    public GameObject crosshairRoot; // optional: drag Crosshair root object here

    static string pendingBossQuote;
    TMP_Text bossQuoteText;

    static readonly string[] bossQuotes =
    {
        "The boss says: \"Why so serious?\"",
        "The boss says: \"I am inevitable.\"",
        "The boss says: \"There is no escape.\"",
        "The boss says: \"You underestimate the dark side.\"",
        "The boss says: \"Long live the king.\""
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsGameOver = false;

        if (restartButton != null)
        {
            PrepareRestartButton();
            restartButton.SetActive(false);
        }
    }

    public static void ShowBossQuote()
    {
        pendingBossQuote = bossQuotes[Random.Range(0, bossQuotes.Length)];
    }

    void PrepareRestartButton()
    {
        if (restartButton == null) return;

        RectTransform restartRect = restartButton.GetComponent<RectTransform>();
        if (restartRect != null)
        {
            restartRect.anchorMin = new Vector2(0.5f, 0.5f);
            restartRect.anchorMax = new Vector2(0.5f, 0.5f);
            restartRect.pivot = new Vector2(0.5f, 0.5f);
            restartRect.sizeDelta = new Vector2(420f, 92f);
        }

        Image image = restartButton.GetComponent<Image>();
        if (image != null)
            image.color = new Color(0.74f, 0.16f, 0.21f, 1f);

        TMP_Text label = restartButton.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = "RESTART";
            label.color = new Color(0.9f, 0.97f, 1f, 1f);
            label.fontSize = 40f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;

            RectTransform labelRect = label.GetComponent<RectTransform>();
            if (labelRect != null)
            {
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
            }
        }

        var button = restartButton.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveListener(Restart);
            button.onClick.AddListener(Restart);
        }

        foreach (var group in restartButton.GetComponentsInParent<CanvasGroup>(true))
        {
            group.interactable = true;
            group.blocksRaycasts = true;
            if (group.alpha <= 0f) group.alpha = 1f;
        }
    }

    void Update()
    {
        // Fallback restart path in case button wiring is missing.
        if (!IsGameOver) return;

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            Restart();
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Stop shoot input so clicking UI does not fire weapons.
        foreach (var shooter in FindObjectsOfType<Shoot2D>())
            shooter.enabled = false;

        // Hide/disable crosshair when player dies.
        if (crosshairRoot != null)
        {
            crosshairRoot.SetActive(false);
        }
        else
        {
            foreach (var crosshair in FindObjectsOfType<CrosshairController2D>())
                crosshair.gameObject.SetActive(false);

            foreach (var follower in FindObjectsOfType<CrosshairFollow>())
                follower.gameObject.SetActive(false);
        }

        // Destroy walls
        if (wallsRoot != null)
            Destroy(wallsRoot);

        if (restartButton != null)
        {
            PrepareRestartButton();
            ShowBossQuoteText();
            ShowRestartButton();
        }
    }

    void ShowBossQuoteText()
    {
        if (string.IsNullOrEmpty(pendingBossQuote) || restartButton == null)
            return;

        if (bossQuoteText == null)
            bossQuoteText = CreateBossQuoteText();

        if (bossQuoteText == null)
            return;

        bossQuoteText.text = pendingBossQuote;
        bossQuoteText.gameObject.SetActive(true);
    }

    void ShowRestartButton()
    {
        if (restartButton == null)
            return;

        restartButton.SetActive(true);

        var button = restartButton.GetComponent<Button>();
        if (button != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    TMP_Text CreateBossQuoteText()
    {
        Transform parent = restartButton.transform.parent;
        if (parent == null)
            return null;

        GameObject quoteObject = new GameObject("BossQuoteText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        quoteObject.transform.SetParent(parent, false);

        RectTransform rect = quoteObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 130f);
        rect.sizeDelta = new Vector2(1000f, 120f);

        TMP_Text text = quoteObject.GetComponent<TMP_Text>();
        text.color = new Color(1f, 0.75f, 0.18f, 1f);
        text.fontSize = 36f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    public void Restart()
    {
        IsGameOver = false;
        pendingBossQuote = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
