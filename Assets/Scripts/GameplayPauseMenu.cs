using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayPauseMenu : MonoBehaviour
{
    readonly Color overlayColor = new Color(0f, 0f, 0f, 0.58f);
    readonly Color panelColor = new Color(0.025f, 0.045f, 0.085f, 0.96f);
    readonly Color buttonColor = new Color(0.08f, 0.18f, 0.28f, 0.98f);
    readonly Color buttonHoverColor = new Color(0.12f, 0.38f, 0.48f, 1f);
    readonly Color dangerButtonColor = new Color(0.62f, 0.12f, 0.16f, 1f);
    readonly Color dangerHoverColor = new Color(0.9f, 0.18f, 0.2f, 1f);
    readonly Color textColor = new Color(0.9f, 0.97f, 1f, 1f);
    readonly List<AudioSource> pausedAudioSources = new List<AudioSource>();

    GameObject pauseOverlay;
    bool isPaused;
    CursorLockMode previousLockState;
    bool previousCursorVisible;

    void Awake()
    {
        BuildPauseOverlay();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            TogglePause();
    }

    void OnDisable()
    {
        ResumeGame();
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        previousCursorVisible = Cursor.visible;
        previousLockState = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
        PauseGameplayAudio();

        if (pauseOverlay != null)
        {
            pauseOverlay.transform.SetAsLastSibling();
            pauseOverlay.SetActive(true);
        }

        isPaused = true;
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        Time.timeScale = 1f;
        RestoreGameplayAudio();

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);

        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousLockState;
        isPaused = false;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        RestoreGameplayAudio();
        SceneManager.LoadScene("MainScene");
    }

    void BuildPauseOverlay()
    {
        if (pauseOverlay != null)
            return;

        pauseOverlay = new GameObject("PauseOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pauseOverlay.transform.SetParent(transform, false);

        RectTransform overlayRect = pauseOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = pauseOverlay.GetComponent<Image>();
        overlayImage.color = overlayColor;
        overlayImage.raycastTarget = true;

        GameObject panel = new GameObject("PausePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(pauseOverlay.transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(620f, 430f);

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = true;

        TMP_Text title = CreateText("PauseTitle", panel.transform, "PAUSED", 62f, TextAlignmentOptions.Center);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 130f);
        titleRect.sizeDelta = new Vector2(560f, 86f);

        TMP_Text subtitle = CreateText("PauseSubtitle", panel.transform, "Take a breather. The galaxy can wait.", 28f, TextAlignmentOptions.Center);
        RectTransform subtitleRect = subtitle.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleRect.anchoredPosition = new Vector2(0f, 72f);
        subtitleRect.sizeDelta = new Vector2(560f, 52f);

        CreateButton(panel.transform, "ResumeButton", "RESUME", new Vector2(0f, -25f), buttonColor, buttonHoverColor, ResumeGame);
        CreateButton(panel.transform, "BackToMenuButton", "BACK TO MENU", new Vector2(0f, -125f), dangerButtonColor, dangerHoverColor, BackToMainMenu);

        pauseOverlay.SetActive(false);
    }

    TMP_Text CreateText(string name, Transform parent, string value, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = value;
        text.color = textColor;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    void CreateButton(Transform parent, string name, string label, Vector2 position, Color normal, Color highlighted, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(430f, 78f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = normal;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = normal;
        colors.highlightedColor = highlighted;
        colors.selectedColor = highlighted;
        colors.pressedColor = new Color(0.02f, 0.04f, 0.07f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.AddListener(action);

        TMP_Text text = CreateText("Text (TMP)", buttonObject.transform, label, 32f, TextAlignmentOptions.Center);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    void PauseGameplayAudio()
    {
        pausedAudioSources.Clear();
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null || !source.isPlaying)
                continue;

            source.Pause();
            pausedAudioSources.Add(source);
        }
    }

    void RestoreGameplayAudio()
    {
        foreach (AudioSource source in pausedAudioSources)
        {
            if (source != null)
                source.UnPause();
        }

        pausedAudioSources.Clear();
    }
}
