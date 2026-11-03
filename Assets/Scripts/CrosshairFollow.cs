using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairFollow : MonoBehaviour
{
    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Vector3 screenPoint = new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPoint);

        transform.position = worldPos;
    }
}