using UnityEngine;
using UnityEngine.InputSystem;

public class SatelliteClickManager : MonoBehaviour
{
    Camera mainCam;
    LineRenderer currentActiveOrbitLine;
    LineRenderer currentActiveNadirLine;

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

                LineRenderer newOrbitLine = satellite.GetOrbitLineRenderer();
                LineRenderer newNadirLine=satellite.GetNadirLineRenderer();
                if (newOrbitLine == null || newNadirLine==null) return;

                // Disable previous selection
                if (currentActiveOrbitLine != null && currentActiveOrbitLine != newOrbitLine)
                    currentActiveOrbitLine.enabled = false;
                if(currentActiveNadirLine!=null && currentActiveNadirLine!= newNadirLine)
                    currentActiveNadirLine.enabled = false;

                // Enable new selection
                newOrbitLine.enabled = true;
                currentActiveOrbitLine = newOrbitLine;
                newNadirLine.enabled=true;
                currentActiveNadirLine = newNadirLine;

                Debug.Log($"{satellite.sat.Tle.NoradNumber} {satellite.sat.Name}");
            }
        }
    }
}
