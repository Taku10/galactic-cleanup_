using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameFeelController : MonoBehaviour
{
    public static GameFeelController Instance { get; private set; }

    readonly Color damageFlashColor = new Color(0.95f, 0.05f, 0.08f, 0.38f);
    readonly Color powerUpFlashColor = new Color(0.1f, 0.85f, 1f, 0.24f);

    Image flashImage;
    Coroutine flashRoutine;
    Coroutine slowMotionRoutine;

    void Awake()
    {
        Instance = this;
        BuildFlashOverlay();
    }

    public static void PlayerHit()
    {
        CameraFollow2D.Shake(0.28f, 0.22f);
        if (Instance != null)
            Instance.Flash(Instance.damageFlashColor, 0.18f);
    }

    public static void EnemyDeath(Vector3 position)
    {
        CameraFollow2D.Shake(0.08f, 0.08f);
    }

    public static void BossPhase()
    {
        CameraFollow2D.Shake(0.36f, 0.35f);
        if (Instance != null)
            Instance.Flash(new Color(1f, 0.25f, 0.08f, 0.26f), 0.25f);
    }

    public static void BossDeath()
    {
        CameraFollow2D.Shake(0.72f, 0.65f);
        if (Instance != null)
        {
            Instance.Flash(new Color(1f, 0.78f, 0.18f, 0.34f), 0.35f);
            Instance.StartSlowMotion(0.32f, 0.55f);
        }
    }

    public static void PowerUpCollected()
    {
        CameraFollow2D.Shake(0.12f, 0.12f);
        if (Instance != null)
            Instance.Flash(Instance.powerUpFlashColor, 0.2f);
    }

    void BuildFlashOverlay()
    {
        if (flashImage != null)
            return;

        GameObject flashObject = new GameObject("ScreenFlash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        flashObject.transform.SetParent(transform, false);

        RectTransform rect = flashObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        flashImage = flashObject.GetComponent<Image>();
        flashImage.color = Color.clear;
        flashImage.raycastTarget = false;
    }

    void Flash(Color color, float duration)
    {
        if (flashImage == null)
            BuildFlashOverlay();

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine(color, duration));
    }

    IEnumerator FlashRoutine(Color color, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float percent = 1f - Mathf.Clamp01(elapsed / duration);
            Color faded = color;
            faded.a *= percent;
            flashImage.color = faded;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        flashImage.color = Color.clear;
        flashRoutine = null;
    }

    void StartSlowMotion(float scale, float duration)
    {
        if (slowMotionRoutine != null)
            StopCoroutine(slowMotionRoutine);

        slowMotionRoutine = StartCoroutine(SlowMotionRoutine(scale, duration));
    }

    IEnumerator SlowMotionRoutine(float scale, float duration)
    {
        float previousScale = Time.timeScale;
        Time.timeScale = Mathf.Min(previousScale, scale);

        yield return new WaitForSecondsRealtime(duration);

        if (Time.timeScale <= scale + 0.01f)
            Time.timeScale = previousScale;

        slowMotionRoutine = null;
    }
}
