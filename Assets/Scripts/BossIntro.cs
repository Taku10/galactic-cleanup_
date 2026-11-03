using UnityEngine;
using System.Collections;

public class BossIntro : MonoBehaviour
{
    public float introTime = 2f;
    Vector3 finalScale;

    void Awake()
    {
        finalScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    public void PlayIntro()
    {
        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        float t = 0f;

        while (t < introTime)
        {
            t += Time.deltaTime;
            float percent = t / introTime;

            transform.localScale = Vector3.Lerp(Vector3.zero, finalScale, percent);

            yield return null;
        }

        transform.localScale = finalScale;
    }
}