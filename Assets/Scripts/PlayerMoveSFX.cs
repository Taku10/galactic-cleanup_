using UnityEngine;

public class PlayerMoveSFX : MonoBehaviour
{
    public AudioSource moveSource;         // drag player's AudioSource here
    public float startThreshold = 0.1f;    // how much input counts as "moving"

    HumanMove moveScript;

    void Awake()
    {
        moveScript = GetComponent<HumanMove>();
    }

    void Update()
    {
        if (!moveSource || !moveScript) return;

        bool isMoving = moveScript.MoveInput.sqrMagnitude > startThreshold * startThreshold;

        if (isMoving)
        {
            if (!moveSource.isPlaying) moveSource.Play();
        }
        else
        {
            if (moveSource.isPlaying) moveSource.Stop();
        }
    }
}