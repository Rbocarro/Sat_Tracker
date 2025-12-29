using UnityEngine;
using UnityEngine.InputSystem;

public class SatelliteClickManager : MonoBehaviour
{
    Camera mainCam;
    LineRenderer currentActiveLine;

    void Awake()
    {
        mainCam = Camera.main;
    }
    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCam.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var satellite = hit.collider.GetComponent<SatelliteBillboard>();
                if (satellite == null) return;

                LineRenderer newLine = satellite.GetLineRenderer();
                if (newLine == null) return;

                // Disable previous selection
                if (currentActiveLine != null && currentActiveLine != newLine)
                    currentActiveLine.enabled = false;

                // Enable new selection
                newLine.enabled = true;
                currentActiveLine = newLine;

                Debug.Log($"{satellite.tle.NoradNumber} {satellite.tle.Name}");
            }
        }
    }
}
