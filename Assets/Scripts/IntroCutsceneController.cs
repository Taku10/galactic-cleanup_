using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCutsceneController : MonoBehaviour
{
    public const string WatchedKey = "GalacticCleanupIntroCutsceneWatched";

    const string ComicResourcePath = "Cutscenes/galactic_cleanup_intro";
    const string NarrationResourcePath = "Cutscenes/intro_narration";
    const string GladiatorMusicResourcePath = "Cutscenes/gladiator";
    const string RevolutionMusicResourcePath = "Cutscenes/revolution";
    const float TimelineDuration = 73f;
    const float PanelTransitionSeconds = 0.35f;
    const float HyperspaceTransitionDuration = 1.25f;
    const float NarrationVolume = 1.25f;
    const float CutsceneMusicVolume = 0.14f;
    const float PostSlideSixSpeedupSeconds = 4f;
    const int HyperspaceStreakCount = 72;

    static readonly float[] PanelStartTimes =
    {
        0f, 4f, 8f, 12f,
        16f, 20f, 25f, 30f,
        35f, 40f, 46f, 51f,
        56f, 60f, 63f, 67f
    };

    readonly Color buttonColor = new Color(0.05f, 0.08f, 0.12f, 0.92f);
    readonly Color buttonHoverColor = new Color(0.75f, 0.13f, 0.18f, 1f);

    Action finishedCallback;
    GameObject rootObject;
    RectTransform viewportRect;
    RectTransform comicRect;
    RectTransform transitionRoot;
    RawImage comicImage;
    Button skipButton;
    AudioSource narrationSource;
    AudioSource cutsceneMusicSource;
    Coroutine cutsceneMusicRoutine;
    Texture2D comicTexture;
    bool isPlaying;
    bool isFinishing;
    bool originalAudioListenerPause;
    bool audioIsolated;
    float elapsed;

    public static bool HasWatched()
    {
        return PlayerPrefs.GetInt(WatchedKey, 0) == 1;
    }

    public static void MarkWatched()
    {
        PlayerPrefs.SetInt(WatchedKey, 1);
        PlayerPrefs.Save();
    }

    public static void ResetWatched()
    {
        PlayerPrefs.DeleteKey(WatchedKey);
        PlayerPrefs.Save();
    }

    public void Play(Action onFinished)
    {
        finishedCallback = onFinished;
        elapsed = 0f;
        isPlaying = true;
        isFinishing = false;

        BuildUi();
        LoadAssets();
        IsolateCutsceneAudio();
        PlayNarration();
        PlayCutsceneMusic();
        UpdateArtwork(0f);
    }

    void Update()
    {
        if (!isPlaying || isFinishing)
            return;

        elapsed += Time.unscaledDeltaTime;

        float audioTime = narrationSource != null && narrationSource.clip != null && narrationSource.isPlaying
            ? narrationSource.time
            : elapsed;

        float cutsceneTime = GetSyncedCutsceneTime(audioTime);
        UpdateArtwork(cutsceneTime);

        float targetDuration = narrationSource != null && narrationSource.clip != null
            ? narrationSource.clip.length
            : TimelineDuration;

        bool narrationEnded = narrationSource != null
            && narrationSource.clip != null
            && !narrationSource.isPlaying
            && elapsed > 0.5f
            && audioTime >= narrationSource.clip.length - 0.15f;

        if (audioTime >= targetDuration || narrationEnded)
            FinishCutscene();
    }

    void BuildUi()
    {
        if (rootObject != null)
        {
            rootObject.SetActive(true);
            return;
        }

        rootObject = new GameObject("IntroCutsceneOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        rootObject.transform.SetParent(transform, false);
        rootObject.transform.SetAsLastSibling();

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image background = rootObject.GetComponent<Image>();
        background.color = Color.black;
        background.raycastTarget = true;

        GameObject viewportObject = new GameObject("ComicViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(RectMask2D));
        viewportObject.transform.SetParent(rootObject.transform, false);
        viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        GameObject comicObject = new GameObject("ComicImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        comicObject.transform.SetParent(viewportObject.transform, false);
        comicRect = comicObject.GetComponent<RectTransform>();
        comicRect.anchorMin = new Vector2(0.5f, 0.5f);
        comicRect.anchorMax = new Vector2(0.5f, 0.5f);
        comicRect.pivot = new Vector2(0.5f, 0.5f);
        comicImage = comicObject.GetComponent<RawImage>();
        comicImage.color = Color.white;
        comicImage.raycastTarget = false;

        skipButton = CreateSkipButton(rootObject.transform);
    }

    Button CreateSkipButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("SkipCutsceneButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-38f, -32f);
        rect.sizeDelta = new Vector2(190f, 58f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = buttonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(FinishCutscene);

        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHoverColor;
        colors.selectedColor = buttonHoverColor;
        colors.pressedColor = new Color(0.02f, 0.03f, 0.05f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        GameObject labelObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        label.text = "SKIP";
        label.color = Color.white;
        label.fontSize = 28f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        return button;
    }

    void LoadAssets()
    {
        if (comicTexture == null)
            comicTexture = Resources.Load<Texture2D>(ComicResourcePath);

        if (comicTexture == null)
        {
            Debug.LogWarning("IntroCutsceneController: cutscene image was not found at Assets/Resources/" + ComicResourcePath + ".");
            return;
        }

        comicImage.texture = comicTexture;
    }

    void PlayNarration()
    {
        AudioClip narration = Resources.Load<AudioClip>(NarrationResourcePath);
        if (narration == null)
        {
            Debug.LogWarning("IntroCutsceneController: narration audio was not found at Assets/Resources/" + NarrationResourcePath + ".");
            return;
        }

        if (narrationSource == null)
            narrationSource = gameObject.AddComponent<AudioSource>();

        narrationSource.clip = narration;
        narrationSource.playOnAwake = false;
        narrationSource.loop = false;
        narrationSource.ignoreListenerPause = true;
        narrationSource.spatialBlend = 0f;
        narrationSource.volume = NarrationVolume;
        narrationSource.time = 0f;
        narrationSource.Play();
    }

    void PlayCutsceneMusic()
    {
        if (cutsceneMusicRoutine != null)
            StopCoroutine(cutsceneMusicRoutine);

        cutsceneMusicRoutine = StartCoroutine(PlayCutsceneMusicSequence());
    }

    IEnumerator PlayCutsceneMusicSequence()
    {
        AudioClip gladiator = Resources.Load<AudioClip>(GladiatorMusicResourcePath);
        AudioClip revolution = Resources.Load<AudioClip>(RevolutionMusicResourcePath);

        if (gladiator == null && revolution == null)
        {
            Debug.LogWarning("IntroCutsceneController: cutscene music clips were not found in Assets/Resources/Cutscenes.");
            yield break;
        }

        if (cutsceneMusicSource == null)
            cutsceneMusicSource = gameObject.AddComponent<AudioSource>();

        cutsceneMusicSource.playOnAwake = false;
        cutsceneMusicSource.loop = false;
        cutsceneMusicSource.ignoreListenerPause = true;
        cutsceneMusicSource.spatialBlend = 0f;
        cutsceneMusicSource.volume = CutsceneMusicVolume;

        if (gladiator != null)
        {
            cutsceneMusicSource.clip = gladiator;
            cutsceneMusicSource.time = 0f;
            cutsceneMusicSource.Play();

            while (isPlaying && !isFinishing && cutsceneMusicSource.isPlaying)
                yield return null;
        }

        if (!isPlaying || isFinishing || revolution == null)
            yield break;

        cutsceneMusicSource.clip = revolution;
        cutsceneMusicSource.time = 0f;
        cutsceneMusicSource.Play();

        while (isPlaying && !isFinishing && cutsceneMusicSource.isPlaying)
            yield return null;
    }

    void StopOtherAudioSources()
    {
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source == null || source == narrationSource || source == cutsceneMusicSource)
                continue;

            source.Stop();
        }
    }

    void IsolateCutsceneAudio()
    {
        StopOtherAudioSources();

        if (!audioIsolated)
        {
            originalAudioListenerPause = AudioListener.pause;
            audioIsolated = true;
        }

        AudioListener.pause = true;
    }

    void RestoreSceneAudio()
    {
        if (!audioIsolated)
            return;

        AudioListener.pause = originalAudioListenerPause;
        audioIsolated = false;
    }

    void UpdateArtwork(float time)
    {
        if (comicTexture == null || comicRect == null || viewportRect == null)
            return;

        int panelIndex = GetPanelIndex(time);
        float panelStart = PanelStartTimes[panelIndex];
        float panelEnd = panelIndex + 1 < PanelStartTimes.Length ? PanelStartTimes[panelIndex + 1] : TimelineDuration;
        float rawPanelProgress = Mathf.InverseLerp(panelStart, panelEnd, time);
        float transitionSeconds = GetPanelTransitionSeconds(panelIndex);
        float transitionPercent = Mathf.Clamp01(transitionSeconds / Mathf.Max(0.01f, panelEnd - panelStart));
        bool inTransition = panelIndex > 0 && rawPanelProgress < transitionPercent;

        float panelProgress = inTransition
            ? 0f
            : Mathf.InverseLerp(transitionPercent, 1f, rawPanelProgress);
        float smoothProgress = Mathf.SmoothStep(0f, 1f, panelProgress);

        float panelWidth = comicTexture.width / 4f;
        float panelHeight = comicTexture.height / 4f;

        Vector2 viewportSize = viewportRect.rect.size;
        if (viewportSize.x <= 0f || viewportSize.y <= 0f)
            viewportSize = new Vector2(Screen.width, Screen.height);

        float fitScale = Mathf.Min(viewportSize.x / panelWidth, viewportSize.y / panelHeight);
        CameraState state = GetPanelCameraState(panelIndex, smoothProgress, panelWidth, panelHeight);

        if (inTransition)
        {
            CameraState previousState = GetPanelCameraState(panelIndex - 1, 1f, panelWidth, panelHeight);
            CameraState nextState = GetPanelCameraState(panelIndex, 0f, panelWidth, panelHeight);
            float transitionProgress = Mathf.SmoothStep(0f, 1f, rawPanelProgress / transitionPercent);

            state.focus = Vector2.Lerp(previousState.focus, nextState.focus, transitionProgress);
            state.zoom = Mathf.Lerp(previousState.zoom, nextState.zoom, transitionProgress);
        }

        float zoom = state.zoom;
        float finalScale = fitScale * zoom;

        Vector2 imageSize = new Vector2(comicTexture.width * finalScale, comicTexture.height * finalScale);
        comicRect.sizeDelta = imageSize;

        Vector2 focusNormalized = new Vector2(state.focus.x / comicTexture.width, state.focus.y / comicTexture.height);
        comicRect.anchoredPosition = new Vector2(
            (0.5f - focusNormalized.x) * imageSize.x,
            (0.5f - focusNormalized.y) * imageSize.y
        );
    }

    float GetPanelTransitionSeconds(int panelIndex)
    {
        if (panelIndex == 7 || panelIndex == 8)
            return 0f;

        return PanelTransitionSeconds;
    }

    float GetSyncedCutsceneTime(float audioTime)
    {
        if (narrationSource == null || narrationSource.clip == null || narrationSource.clip.length <= 0f)
            return audioTime;

        float timelineTime = Mathf.Clamp01(audioTime / narrationSource.clip.length) * TimelineDuration;
        return ApplyPostSlideSixSpeedup(timelineTime);
    }

    float ApplyPostSlideSixSpeedup(float timelineTime)
    {
        if (timelineTime <= 25f)
            return timelineTime;

        float speedup = Mathf.SmoothStep(0f, PostSlideSixSpeedupSeconds, Mathf.InverseLerp(25f, TimelineDuration, timelineTime));
        return Mathf.Min(TimelineDuration, timelineTime + speedup);
    }

    CameraState GetPanelCameraState(int panelIndex, float progress, float panelWidth, float panelHeight)
    {
        int column = panelIndex % 4;
        int row = panelIndex / 4;

        float panelCenterX = (column + 0.5f) * panelWidth;
        float panelCenterYFromTop = (row + 0.5f) * panelHeight;
        float innerPan = Mathf.Lerp(-0.11f, 0.11f, progress) * panelWidth;
        float innerTilt = Mathf.Sin(progress * Mathf.PI) * 0.04f * panelHeight;

        return new CameraState
        {
            focus = new Vector2(panelCenterX + innerPan, comicTexture.height - panelCenterYFromTop + innerTilt),
            zoom = Mathf.Lerp(0.96f, 1.06f, progress)
        };
    }

    struct CameraState
    {
        public Vector2 focus;
        public float zoom;
    }

    int GetPanelIndex(float time)
    {
        for (int i = PanelStartTimes.Length - 1; i >= 0; i--)
        {
            if (time >= PanelStartTimes[i])
                return i;
        }

        return 0;
    }

    void FinishCutscene()
    {
        if (isFinishing)
            return;

        isFinishing = true;
        isPlaying = false;

        if (narrationSource != null)
            narrationSource.Stop();

        if (cutsceneMusicRoutine != null)
        {
            StopCoroutine(cutsceneMusicRoutine);
            cutsceneMusicRoutine = null;
        }

        if (cutsceneMusicSource != null)
            cutsceneMusicSource.Stop();

        MarkWatched();

        StartCoroutine(PlayHyperspaceTransition());
    }

    public void FinishCutsceneAndLoadGame()
    {
        finishedCallback = () => SceneManager.LoadScene("Game");
        FinishCutscene();
    }

    IEnumerator PlayHyperspaceTransition()
    {
        if (rootObject == null)
        {
            RestoreSceneAudio();
            finishedCallback?.Invoke();
            yield break;
        }

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        if (comicImage != null)
            comicImage.color = new Color(1f, 1f, 1f, 0.18f);

        Image flash = BuildHyperspaceOverlay();
        RectTransform[] streaks = BuildHyperspaceStreaks();
        float[] angles = new float[streaks.Length];
        float[] distanceOffsets = new float[streaks.Length];

        for (int i = 0; i < streaks.Length; i++)
        {
            angles[i] = i * (360f / streaks.Length) + UnityEngine.Random.Range(-2.5f, 2.5f);
            distanceOffsets[i] = UnityEngine.Random.Range(0f, 120f);
        }

        float elapsedTransition = 0f;
        while (elapsedTransition < HyperspaceTransitionDuration)
        {
            elapsedTransition += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTransition / HyperspaceTransitionDuration);
            float pull = Mathf.SmoothStep(0f, 1f, t);
            float flashAlpha = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI) * 0.72f + Mathf.InverseLerp(0.82f, 1f, t));

            if (flash != null)
                flash.color = new Color(0.72f, 0.95f, 1f, flashAlpha);

            for (int i = 0; i < streaks.Length; i++)
            {
                RectTransform streak = streaks[i];
                if (streak == null)
                    continue;

                float angle = angles[i] * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float distance = Mathf.Lerp(25f + distanceOffsets[i], 1450f + distanceOffsets[i], pull);
                float length = Mathf.Lerp(20f, 760f, pull);
                float thickness = Mathf.Lerp(3f, 11f, Mathf.Sin(t * Mathf.PI));

                streak.anchoredPosition = direction * distance;
                streak.sizeDelta = new Vector2(length, thickness);
                streak.localEulerAngles = new Vector3(0f, 0f, angles[i]);

                Image streakImage = streak.GetComponent<Image>();
                if (streakImage != null)
                {
                    float alpha = Mathf.Clamp01((1f - t * 0.65f) * Mathf.SmoothStep(0f, 1f, t * 3f));
                    streakImage.color = new Color(0.78f, 0.94f, 1f, alpha);
                }
            }

            yield return null;
        }

        RestoreSceneAudio();
        finishedCallback?.Invoke();
    }

    void OnDisable()
    {
        RestoreSceneAudio();
    }

    Image BuildHyperspaceOverlay()
    {
        if (transitionRoot != null)
            Destroy(transitionRoot.gameObject);

        GameObject transitionObject = new GameObject("HyperspaceTransition", typeof(RectTransform));
        transitionObject.transform.SetParent(rootObject.transform, false);
        transitionRoot = transitionObject.GetComponent<RectTransform>();
        transitionRoot.anchorMin = Vector2.zero;
        transitionRoot.anchorMax = Vector2.one;
        transitionRoot.offsetMin = Vector2.zero;
        transitionRoot.offsetMax = Vector2.zero;

        GameObject flashObject = new GameObject("HyperspaceFlash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        flashObject.transform.SetParent(transitionRoot, false);

        RectTransform flashRect = flashObject.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;

        Image flash = flashObject.GetComponent<Image>();
        flash.color = new Color(0.72f, 0.95f, 1f, 0f);
        flash.raycastTarget = false;
        return flash;
    }

    RectTransform[] BuildHyperspaceStreaks()
    {
        RectTransform[] streaks = new RectTransform[HyperspaceStreakCount];
        for (int i = 0; i < streaks.Length; i++)
        {
            GameObject streakObject = new GameObject("HyperspaceStreak", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            streakObject.transform.SetParent(transitionRoot, false);

            RectTransform rect = streakObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(16f, 4f);

            Image image = streakObject.GetComponent<Image>();
            image.color = new Color(0.78f, 0.94f, 1f, 0f);
            image.raycastTarget = false;

            streaks[i] = rect;
        }

        return streaks;
    }
}
