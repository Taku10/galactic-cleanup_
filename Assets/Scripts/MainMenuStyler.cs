using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuStyler : MonoBehaviour
{
    readonly Color backgroundColor = new Color(0.015f, 0.025f, 0.06f, 1f);
    readonly Color panelColor = new Color(0.035f, 0.055f, 0.11f, 0.86f);
    readonly Color buttonColor = new Color(0.08f, 0.18f, 0.28f, 0.95f);
    readonly Color buttonHoverColor = new Color(0.12f, 0.38f, 0.48f, 1f);
    readonly Color primaryButtonColor = new Color(0.74f, 0.16f, 0.21f, 1f);
    readonly Color primaryHoverColor = new Color(0.95f, 0.24f, 0.25f, 1f);
    readonly Color textColor = new Color(0.9f, 0.96f, 1f, 1f);
    readonly Color mutedTextColor = new Color(0.62f, 0.76f, 0.88f, 1f);
    readonly Color crawlTextColor = new Color(1f, 0.82f, 0.22f, 1f);

    const float CreditsStartY = -2600f;
    const float CreditsEndY = 2850f;
    const float CreditsScrollSpeed = 78f;
    const float CreditsCrawlTilt = 0f;

    GameObject creditsPanelObject;
    RectTransform creditsCrawlRect;
    AudioSource creditsMusicSource;
    AudioClip creditsMusicClip;
    readonly List<AudioSource> pausedAudioSources = new List<AudioSource>();
    readonly Dictionary<string, Sprite> instructionIconSprites = new Dictionary<string, Sprite>();
    bool creditsWereVisible;
    bool otherAudioPausedForCredits;

    void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        transform.localScale = Vector3.one;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        BuildBackground();
        StyleStartScreen();
        StyleInstructions();
        StyleCredits();
        WireMenuButtons();
    }

    void Update()
    {
        if (creditsPanelObject == null || creditsCrawlRect == null)
            return;

        bool creditsVisible = creditsPanelObject.activeInHierarchy;
        if (creditsVisible && !creditsWereVisible)
            ResetCreditsCrawl();

        UpdateCreditsMusic(creditsVisible);

        if (creditsVisible)
        {
            Vector2 position = creditsCrawlRect.anchoredPosition;
            position.y += CreditsScrollSpeed * Time.unscaledDeltaTime;

            if (position.y > CreditsEndY)
                position.y = CreditsStartY;

            creditsCrawlRect.anchoredPosition = position;
            creditsCrawlRect.localEulerAngles = new Vector3(CreditsCrawlTilt, 0f, 0f);
        }

        creditsWereVisible = creditsVisible;
    }

    void OnDisable()
    {
        if (creditsMusicSource != null)
            creditsMusicSource.Stop();

        RestoreOtherAudioAfterCredits();
    }

    void BuildBackground()
    {
        if (transform.Find("StyledBackdrop") != null)
            return;

        RectTransform backdrop = CreatePanel("StyledBackdrop", transform, backgroundColor);
        backdrop.SetAsFirstSibling();

        Image image = backdrop.GetComponent<Image>();
        image.raycastTarget = false;

        RectTransform nebula = CreatePanel("DeepSpaceBand", backdrop, new Color(0.07f, 0.12f, 0.22f, 0.42f));
        nebula.anchorMin = new Vector2(0f, 0.18f);
        nebula.anchorMax = new Vector2(1f, 0.55f);
        nebula.offsetMin = Vector2.zero;
        nebula.offsetMax = Vector2.zero;
        nebula.Rotate(0f, 0f, -8f);
        nebula.GetComponent<Image>().raycastTarget = false;

        RectTransform redSignal = CreatePanel("WarningSignal", backdrop, new Color(0.48f, 0.08f, 0.12f, 0.26f));
        redSignal.anchorMin = new Vector2(0.08f, 0.08f);
        redSignal.anchorMax = new Vector2(0.92f, 0.2f);
        redSignal.offsetMin = Vector2.zero;
        redSignal.offsetMax = Vector2.zero;
        redSignal.GetComponent<Image>().raycastTarget = false;

        for (int i = 0; i < 120; i++)
        {
            GameObject star = new GameObject("Star", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            star.transform.SetParent(backdrop, false);

            RectTransform rect = star.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(Random.value, Random.value);
            rect.anchorMax = rect.anchorMin;
            rect.anchoredPosition = Vector2.zero;

            float size = Random.Range(2f, 6f);
            rect.sizeDelta = new Vector2(size, size);

            Image starImage = star.GetComponent<Image>();
            float alpha = Random.Range(0.25f, 0.9f);
            starImage.color = new Color(0.75f, 0.9f, 1f, alpha);
            starImage.raycastTarget = false;
        }
    }

    void StyleStartScreen()
    {
        GameObject panel = FindObject("StartScreenPanel");
        if (panel == null) return;

        StyleFullPanel(panel, new Color(0f, 0f, 0f, 0f));

        TMP_Text title = FindText("TitleText");
        if (title != null)
        {
            title.text = "GALACTIC CLEANUP";
            title.color = textColor;
            title.fontSize = 92f;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.characterSpacing = 5f;

            RectTransform rect = title.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 370f);
            rect.sizeDelta = new Vector2(1450f, 140f);

            AddShadow(title.gameObject, new Color(0.75f, 0.05f, 0.08f, 0.75f), new Vector2(0f, -6f));
        }

        TMP_Text subtitle = EnsureText(panel.transform, "SubtitleText");
        subtitle.text = "CLEAN THE GALAXY. OUTRUN THE CHAOS.";
        subtitle.color = mutedTextColor;
        subtitle.fontSize = 36f;
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.characterSpacing = 1.5f;
        RectTransform subtitleRect = subtitle.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleRect.anchoredPosition = new Vector2(0f, 230f);
        subtitleRect.sizeDelta = new Vector2(1250f, 64f);

        EnsureButton(panel.transform, "CreditsButton");

        HideObject("WelcomeCaptainText");
        HideObject("LeaderboardButton");
        HideObject("LeaderboardPanel");
        HideObject("NamePromptPanel");

        StyleButton("StartButton", "START RUN", primaryButtonColor, primaryHoverColor, new Vector2(0f, 45f));
        StyleButton("InstructionsButton", "HOW TO PLAY", buttonColor, buttonHoverColor, new Vector2(0f, -65f));
        StyleButton("CreditsButton", "CREDITS", buttonColor, buttonHoverColor, new Vector2(0f, -175f));
        StyleButton("QuitButton", "QUIT", new Color(0.05f, 0.08f, 0.13f, 0.95f), buttonHoverColor, new Vector2(0f, -285f));
    }

    void StyleInstructions()
    {
        GameObject panel = FindObject("InstructionsPanel");
        if (panel == null) return;

        StyleFullPanel(panel, new Color(0f, 0f, 0f, 0f));

        TMP_Text title = FindText("InstructionText");
        if (title != null)
        {
            title.text = "HOW TO PLAY";
            title.color = textColor;
            title.fontSize = 64f;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.characterSpacing = 3f;
            title.lineSpacing = 0f;

            RectTransform rect = title.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 365f);
            rect.sizeDelta = new Vector2(1250f, 95f);
        }

        TMP_Text controls = EnsureText(panel.transform, "InstructionControlsText");
        controls.text = "Move with WASD or arrow keys. Aim with the mouse.";
        controls.color = mutedTextColor;
        controls.fontSize = 34f;
        controls.fontStyle = FontStyles.Bold;
        controls.alignment = TextAlignmentOptions.Center;
        controls.enableWordWrapping = false;
        RectTransform controlsRect = controls.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0.5f, 0.5f);
        controlsRect.anchorMax = new Vector2(0.5f, 0.5f);
        controlsRect.anchoredPosition = new Vector2(0f, 292f);
        controlsRect.sizeDelta = new Vector2(1450f, 58f);

        HideInstructionRow("HealInstruction");
        HideInstructionRow("SpeedInstruction");
        HideInstructionRow("RapidInstruction");
        HideInstructionRow("ShieldInstruction");
        HideInstructionRow("RocketInstruction");
        HideInstructionRow("TeleportInstruction");

        StyleInstructionRow(panel.transform, "MoveInstruction", "MOVE", "WASD or arrow keys", new Color(0.16f, 0.7f, 1f, 1f), ">>", 190f);
        StyleInstructionRow(panel.transform, "FireInstruction", "FIRE", "Left click to shoot where you aim", new Color(1f, 0.82f, 0.16f, 1f), "!", 95f);
        StyleInstructionRow(panel.transform, "ToolsInstruction", "TOOLS", "Space or right click fires rockets. T teleports.", new Color(0.76f, 0.45f, 1f, 1f), "T", 0f);
        StyleInstructionRow(panel.transform, "PowerupsInstruction", "POWER-UPS", "Pick up health, shield, speed, rapid fire, and rockets.", new Color(0.28f, 0.95f, 0.42f, 1f), "+", -95f);
        StyleInstructionRow(panel.transform, "GoalInstruction", "GOAL", "Survive 5 levels, then defeat the boss.", new Color(1f, 0.36f, 0.08f, 1f), "5", -190f);

        TMP_Text objective = EnsureText(panel.transform, "InstructionObjectiveText");
        objective.text = "Keep moving. Use rockets and teleport when the screen gets crowded.";
        objective.color = crawlTextColor;
        objective.fontSize = 32f;
        objective.fontStyle = FontStyles.Bold;
        objective.alignment = TextAlignmentOptions.Center;
        objective.enableWordWrapping = true;
        RectTransform objectiveRect = objective.GetComponent<RectTransform>();
        objectiveRect.anchorMin = new Vector2(0.5f, 0.5f);
        objectiveRect.anchorMax = new Vector2(0.5f, 0.5f);
        objectiveRect.anchoredPosition = new Vector2(0f, -300f);
        objectiveRect.sizeDelta = new Vector2(1450f, 72f);

        StyleButton("BackButton", "BACK", buttonColor, buttonHoverColor, new Vector2(0f, -415f));
    }

    void StyleInstructionRow(Transform parent, string rowName, string title, string description, Color iconColor, string symbol, float y)
    {
        Image icon = EnsureImage(parent, rowName + "Icon");
        icon.sprite = GetInstructionIconSprite(rowName, iconColor);
        icon.color = Color.white;
        icon.raycastTarget = false;

        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(-540f, y);
        iconRect.sizeDelta = new Vector2(78f, 78f);

        TMP_Text symbolText = EnsureText(parent, rowName + "Symbol");
        symbolText.text = symbol;
        symbolText.color = new Color(0.02f, 0.04f, 0.08f, 1f);
        symbolText.fontSize = symbol.Length > 1 ? 24f : 32f;
        symbolText.fontStyle = FontStyles.Bold;
        symbolText.alignment = TextAlignmentOptions.Center;
        symbolText.raycastTarget = false;

        RectTransform symbolRect = symbolText.GetComponent<RectTransform>();
        symbolRect.anchorMin = new Vector2(0.5f, 0.5f);
        symbolRect.anchorMax = new Vector2(0.5f, 0.5f);
        symbolRect.pivot = new Vector2(0.5f, 0.5f);
        symbolRect.anchoredPosition = iconRect.anchoredPosition;
        symbolRect.sizeDelta = iconRect.sizeDelta;

        TMP_Text rowText = EnsureText(parent, rowName + "Text");
        rowText.text = title + "\n<size=30>" + description + "</size>";
        rowText.color = textColor;
        rowText.fontSize = 36f;
        rowText.fontStyle = FontStyles.Bold;
        rowText.alignment = TextAlignmentOptions.Left;
        rowText.lineSpacing = 3f;
        rowText.enableWordWrapping = true;
        rowText.raycastTarget = false;

        RectTransform textRect = rowText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(150f, y);
        textRect.sizeDelta = new Vector2(1180f, 86f);
    }

    void HideInstructionRow(string rowName)
    {
        HideObject(rowName + "Icon");
        HideObject(rowName + "Symbol");
        HideObject(rowName + "Text");
    }

    Sprite GetInstructionIconSprite(string key, Color color)
    {
        if (instructionIconSprites.TryGetValue(key, out Sprite sprite))
            return sprite;

        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float outerRadius = size * 0.46f;
        float innerRadius = size * 0.34f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance > outerRadius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float edgeFade = Mathf.InverseLerp(outerRadius, outerRadius - 3f, distance);
                float glow = Mathf.InverseLerp(outerRadius, innerRadius, distance);
                Color pixel = Color.Lerp(new Color(1f, 1f, 1f, 1f), color, glow);
                pixel.a = Mathf.Clamp01(edgeFade);
                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 96f);
        instructionIconSprites[key] = sprite;
        return sprite;
    }

    void StyleCredits()
    {
        GameObject panel = FindObject("CreditsPanel");
        if (panel == null)
            panel = CreatePanel("CreditsPanel", transform, new Color(0f, 0f, 0f, 0f)).gameObject;

        creditsPanelObject = panel;
        StyleFullPanel(panel, new Color(0f, 0f, 0f, 0f));

        TMP_Text title = EnsureText(panel.transform, "CreditsTitleText");
        title.text = "GALACTIC CLEANUP";
        title.color = crawlTextColor;
        title.fontSize = 78f;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.characterSpacing = 4f;

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 385f);
        titleRect.sizeDelta = new Vector2(1450f, 110f);

        TMP_Text intro = EnsureText(panel.transform, "CreditsIntroText");
        intro.text = "A galaxy overrun by space junk, bad decisions, and one very angry boss...";
        intro.color = mutedTextColor;
        intro.fontSize = 34f;
        intro.fontStyle = FontStyles.Bold;
        intro.alignment = TextAlignmentOptions.Center;
        intro.characterSpacing = 1.2f;

        RectTransform introRect = intro.GetComponent<RectTransform>();
        introRect.anchorMin = new Vector2(0.5f, 0.5f);
        introRect.anchorMax = new Vector2(0.5f, 0.5f);
        introRect.anchoredPosition = new Vector2(0f, 310f);
        introRect.sizeDelta = new Vector2(1500f, 70f);

        RectTransform viewport = EnsureCreditViewport(panel.transform);

        TMP_Text body = EnsureText(panel.transform, "CreditsText");
        body.transform.SetParent(viewport, false);
        body.text = BuildCreditsText();
        body.color = crawlTextColor;
        body.fontSize = 50f;
        body.fontStyle = FontStyles.Bold;
        body.alignment = TextAlignmentOptions.TopGeoAligned;
        body.lineSpacing = 20f;
        body.characterSpacing = 1.4f;
        body.enableWordWrapping = false;
        body.overflowMode = TextOverflowModes.Overflow;

        RectTransform bodyRect = body.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.sizeDelta = new Vector2(1200f, 4400f);
        bodyRect.localEulerAngles = new Vector3(CreditsCrawlTilt, 0f, 0f);
        bodyRect.localScale = new Vector3(1f, 1f, 1f);
        creditsCrawlRect = bodyRect;
        ResetCreditsCrawl();

        EnsureButton(panel.transform, "CreditsBackButton");
        StyleButton("CreditsBackButton", "BACK", buttonColor, buttonHoverColor, new Vector2(0f, -425f));

        panel.SetActive(false);
    }

    RectTransform EnsureCreditViewport(Transform parent)
    {
        Transform existing = parent.Find("CreditsCrawlViewport");
        GameObject viewportObject = existing != null
            ? existing.gameObject
            : new GameObject("CreditsCrawlViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(RectMask2D));

        viewportObject.transform.SetParent(parent, false);

        RectTransform rect = viewportObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -115f);
        rect.sizeDelta = new Vector2(1550f, 760f);
        rect.localEulerAngles = Vector3.zero;

        return rect;
    }

    void ResetCreditsCrawl()
    {
        if (creditsCrawlRect == null)
            return;

        creditsCrawlRect.anchoredPosition = new Vector2(0f, CreditsStartY);
        creditsCrawlRect.localEulerAngles = new Vector3(CreditsCrawlTilt, 0f, 0f);
    }

    void UpdateCreditsMusic(bool creditsVisible)
    {
        if (creditsVisible)
        {
            EnsureCreditsMusic();
            PauseOtherAudioForCredits();

            if (creditsMusicSource != null && creditsMusicClip != null && !creditsMusicSource.isPlaying)
            {
                creditsMusicSource.time = 0f;
                creditsMusicSource.Play();
            }

            return;
        }

        if (creditsMusicSource != null && creditsMusicSource.isPlaying)
            creditsMusicSource.Stop();

        RestoreOtherAudioAfterCredits();
    }

    void EnsureCreditsMusic()
    {
        if (creditsMusicClip == null)
            creditsMusicClip = Resources.Load<AudioClip>("star_wars_credits");

        if (creditsMusicClip == null)
        {
            Debug.LogWarning("MainMenuStyler: star_wars_credits audio clip was not found in Assets/Resources.");
            return;
        }

        if (creditsMusicSource != null)
            return;

        creditsMusicSource = gameObject.AddComponent<AudioSource>();
        creditsMusicSource.clip = creditsMusicClip;
        creditsMusicSource.playOnAwake = false;
        creditsMusicSource.loop = true;
        creditsMusicSource.spatialBlend = 0f;
        creditsMusicSource.volume = 0.75f;
    }

    void PauseOtherAudioForCredits()
    {
        if (otherAudioPausedForCredits)
            return;

        pausedAudioSources.Clear();
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null || source == creditsMusicSource || !source.isPlaying)
                continue;

            source.Pause();
            pausedAudioSources.Add(source);
        }

        otherAudioPausedForCredits = true;
    }

    void RestoreOtherAudioAfterCredits()
    {
        if (!otherAudioPausedForCredits)
            return;

        foreach (AudioSource source in pausedAudioSources)
        {
            if (source != null)
                source.UnPause();
        }

        pausedAudioSources.Clear();
        otherAudioPausedForCredits = false;
    }

    string BuildCreditsText()
    {
        return string.Join("\n\n", new string[]
        {
            "CREDITS",
            "Game Director\nTakunda Madziwa",
            "Lead Programmer\nTakunda Madziwa",
            "Gameplay Design\nTakunda Madziwa",
            "Narrator\nTakunda Madziwa (haha kidding)",
            "Level Design\nTakunda Madziwa",
            "Boss Design\nTakunda Madziwa",
            "Enemy Systems\nTakunda Madziwa",
            "Weapon Systems\nTakunda Madziwa",
            "Power-Up Systems\nTakunda Madziwa",
            "Coffee Consumer\nTakunda Madziwa",
            "User Interface Design\nTakunda Madziwa",
            "Menu Design\nTakunda Madziwa",
            "Game Balancing\nTakunda Madziwa",
            "Quality Assurance\nTakunda Madziwa",
            "Bug Fixing\nTakunda Madziwa",
            "Crying Department\nAlso Takunda",
            "Presentation Preparation\nTakunda Madziwa",
            "Project Testing\nTakunda Madziwa",
            "Special Thanks\nFamily, Friends, and Everyone Who Tested This",
            "Thanks For Playing"
        });
    }

    void WireMenuButtons()
    {
        MainMenuManager manager = FindFirstObjectByType<MainMenuManager>();
        if (manager == null) return;

        GameObject creditsPanel = FindObject("CreditsPanel");
        if (creditsPanel != null)
            manager.creditsPanel = creditsPanel;

        if (manager.startScreenPanel == null)
            manager.startScreenPanel = FindObject("StartScreenPanel");

        if (manager.instructionsPanel == null)
            manager.instructionsPanel = FindObject("InstructionsPanel");

        Button instructionsBackButton = GetButton("BackButton");
        if (instructionsBackButton != null)
        {
            instructionsBackButton.onClick.RemoveAllListeners();
            instructionsBackButton.onClick.AddListener(manager.ShowStartScreen);
        }

        Button creditsButton = GetButton("CreditsButton");
        if (creditsButton != null)
        {
            creditsButton.onClick.RemoveAllListeners();
            creditsButton.onClick.AddListener(manager.ShowCredits);
        }

        Button creditsBackButton = GetButton("CreditsBackButton");
        if (creditsBackButton != null)
        {
            creditsBackButton.onClick.RemoveAllListeners();
            creditsBackButton.onClick.AddListener(manager.ShowStartScreen);
        }
    }

    void StyleFullPanel(GameObject panel, Color color)
    {
        Image image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
            image.raycastTarget = false;
        }

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void StyleButton(string objectName, string label, Color normal, Color highlighted, Vector2 position)
    {
        GameObject buttonObject = FindObject(objectName);
        if (buttonObject == null) return;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(430f, 78f);

        Image image = buttonObject.GetComponent<Image>();
        if (image != null)
            image.color = normal;

        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = highlighted;
            colors.selectedColor = highlighted;
            colors.pressedColor = new Color(0.02f, 0.04f, 0.07f, 1f);
            colors.disabledColor = new Color(0.1f, 0.1f, 0.12f, 0.45f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        TMP_Text text = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = label;
            text.color = textColor;
            text.fontSize = 32f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.characterSpacing = 2f;
            text.raycastTarget = false;
        }

        AddShadow(buttonObject, new Color(0f, 0f, 0f, 0.55f), new Vector2(0f, -5f));
    }

    RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        panel.GetComponent<Image>().color = color;
        return rect;
    }

    TMP_Text EnsureText(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.GetComponent<TMP_Text>();

        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        return textObject.GetComponent<TMP_Text>();
    }

    Image EnsureImage(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.GetComponent<Image>();

        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        return imageObject.GetComponent<Image>();
    }

    GameObject EnsureButton(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.gameObject;

        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = buttonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        GameObject labelObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        label.text = name;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        return buttonObject;
    }

    TMP_Text FindText(string objectName)
    {
        GameObject textObject = FindObject(objectName);
        return textObject != null ? textObject.GetComponent<TMP_Text>() : null;
    }

    Button GetButton(string objectName)
    {
        GameObject buttonObject = FindObject(objectName);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }

    GameObject FindObject(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == objectName)
                return child.gameObject;
        }

        return null;
    }

    void HideObject(string objectName)
    {
        GameObject target = FindObject(objectName);
        if (target != null)
            target.SetActive(false);
    }

    void AddShadow(GameObject target, Color color, Vector2 distance)
    {
        Shadow shadow = target.GetComponent<Shadow>();
        if (shadow == null)
            shadow = target.AddComponent<Shadow>();

        shadow.effectColor = color;
        shadow.effectDistance = distance;
        shadow.useGraphicAlpha = true;
    }
}
