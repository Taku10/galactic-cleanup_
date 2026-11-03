using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float life = 0.6f;
    void Start() => Destroy(gameObject, life);
}