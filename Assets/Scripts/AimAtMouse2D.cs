using UnityEngine;
using UnityEngine.InputSystem;

public class AimAtMouse2D : MonoBehaviour
{
    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (cam == null)
            Debug.LogError("AimAtMouse2D: No Main Camera found. Tag your camera as MainCamera.");
    }

    void Update()
    {
        if (cam == null) return;
        if (Mouse.current == null) return;

        // Screen position (pixels)
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        // IMPORTANT: give ScreenToWorldPoint a Z distance from the camera
        Vector3 screenPoint = new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z);

        Vector3 mouseWorld = cam.ScreenToWorldPoint(screenPoint);

        Vector2 dir = (Vector2)(mouseWorld - transform.position);
        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Rotate so the ship FRONT points at mouse.
        // If it points sideways, change -90f to +90f.
        angle -= 90f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}