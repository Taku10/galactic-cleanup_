using UnityEngine;

public static class BossKillAudio
{
    static AudioClip evilLaughClip;
    static float lastPlayTime = -10f;

    public static void PlayEvilLaugh()
    {
        if (Time.unscaledTime - lastPlayTime < 1f)
            return;

        if (evilLaughClip == null)
            evilLaughClip = Resources.Load<AudioClip>("evil_laugh");

        if (evilLaughClip == null)
        {
            Debug.LogWarning("BossKillAudio: evil_laugh audio clip was not found in Assets/Resources.");
            return;
        }

        GameObject audioObject = new GameObject("BossEvilLaughAudio");
        Object.DontDestroyOnLoad(audioObject);

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 1f;
        source.PlayOneShot(evilLaughClip);

        Object.Destroy(audioObject, evilLaughClip.length + 0.25f);
        lastPlayTime = Time.unscaledTime;
    }
}
