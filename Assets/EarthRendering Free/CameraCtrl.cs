using UnityEngine;
using UnityEngine.InputSystem;

public class CameraCtrl : MonoBehaviour
{
    public Transform globalLight = null;
    public MeshRenderer earthRenderer = null;
    public MeshRenderer atmosphereRenderer = null;

    public const float MIN_DIST = 150;
    public const float MAX_DIST = 1500;

    float dist = 400;
    Vector2 orbitAngles = new Vector2(0f, 0f); // x = yaw, y = pitch
    Vector2 targetOffCenter = Vector2.zero;
    Vector2 offCenter = Vector2.zero;

    public Vector3 targetPosition = Vector3.zero; // The point  orbiting around
    public float orbitSensitivity = 1f;
    public float minPitch = -89f; // Prevent camera flipping
    public float maxPitch = 89f;

    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Calculate initial orbit angles from current position
        Vector3 toCamera = transform.position - targetPosition;
        dist = toCamera.magnitude;

        // Calculate initial angles
        orbitAngles.x = Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg; // Yaw
        orbitAngles.y = Mathf.Asin(toCamera.y / dist) * Mathf.Rad2Deg; // Pitch
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // --- Scroll wheel zoom ---
        float wheelDelta = Mouse.current.scroll.ReadValue().y;

        if (wheelDelta > 0)
            dist *= 0.87f;
        else if (wheelDelta < 0)
            dist *= 1.15f;

        dist = Mathf.Clamp(dist, MIN_DIST, MAX_DIST);

        // --- Mouse movement ---
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float xMove = mouseDelta.x;
        float yMove = mouseDelta.y;

        float targetRadius = 100;

        // --- Rotate camera (LMB) ---
        if (Mouse.current.leftButton.isPressed)
        {
            if (xMove != 0 || yMove != 0)
            {
                float rotateSensitivity =
                    Mathf.Min(2f, ((dist - targetRadius) / targetRadius) * 1.2f) * orbitSensitivity;

                // Update orbit angles
                orbitAngles.x += xMove * rotateSensitivity; // Yaw (around world up)
                orbitAngles.y -= yMove * rotateSensitivity; // Pitch (clamped to prevent flipping)

                // Clamp pitch to prevent camera flipping
                orbitAngles.y = Mathf.Clamp(orbitAngles.y, minPitch, maxPitch);
            }
        }
        // --- Rotate light (RMB) ---
        else if (Mouse.current.rightButton.isPressed)
        {
            Quaternion lightRotation = globalLight.rotation;
            lightRotation *= Quaternion.AngleAxis(-xMove * 2f, Vector3.up);
            globalLight.rotation = lightRotation;
        }
        // --- Pan camera (MMB) ---
        else if (Mouse.current.middleButton.isPressed)
        {
            const float MOUSE_TRANSLATE_SENSITIVITY = 10f;

            targetOffCenter.x -= xMove * MOUSE_TRANSLATE_SENSITIVITY;
            targetOffCenter.y -= yMove * MOUSE_TRANSLATE_SENSITIVITY;

            float translateMultiply =
                0.5625f * Screen.width / Screen.height *
                Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2f) *
                dist / Screen.height / 2.5f;

            offCenter = targetOffCenter * translateMultiply;
        }

        // --- Calculate camera rotation from orbit angles ---
        Quaternion cameraRotation = Quaternion.Euler(orbitAngles.y, orbitAngles.x, 0f);

        // --- Apply transform ---
        transform.rotation = cameraRotation;

        // Calculate position based on rotation and distance from target
        transform.position = targetPosition + cameraRotation * (Vector3.forward * -dist);

        // Add offset for panning
        transform.position +=
            transform.right * offCenter.x +
            transform.up * offCenter.y;
    }
}