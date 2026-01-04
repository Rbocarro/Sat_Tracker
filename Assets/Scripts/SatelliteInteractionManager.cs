using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class SatelliteInteractionManager : MonoBehaviour
{
    Camera mainCam;
    LineRenderer currentActiveOrbitLine;
    LineRenderer currentActiveNadirLine;

    public RectTransform canvasRecTransform;
    RectTransform SatHoverInfoPanelRectTransform;
    public GameObject SatHoverInfoPanel;
    public TMP_Text SatHoverInfoPanelText;
    void Awake()
    {
        mainCam = Camera.main;
        SatHoverInfoPanelRectTransform=SatHoverInfoPanel.GetComponent<RectTransform>();
    }
    void Update()
    {

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePos);
        bool hitDetected = Physics.Raycast(ray, out RaycastHit hit);

        if (!hitDetected) { SatHoverInfoPanel.SetActive(false); return; }

        hit.collider.TryGetComponent<SatelliteBillboard>(out SatelliteBillboard satellite);
        
        if (satellite == null){ SatHoverInfoPanel.SetActive(false); return;}

        SatHoverInfoPanel.SetActive(true);
        Vector2 anchoredPos = mousePos / canvasRecTransform.localScale.y;
        SatHoverInfoPanelRectTransform.anchoredPosition = anchoredPos;
        SatHoverInfoPanelText.text = $"{satellite.sat.Name} <color=green>{satellite.sat.Tle.NoradNumber}</color> {satellite.sat.Orbit.Period}";

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            LineRenderer newOrbitLine = satellite.GetOrbitLineRenderer();
            LineRenderer newNadirLine = satellite.GetNadirLineRenderer();
            if (newOrbitLine == null || newNadirLine == null) return;
            // Disable previous selection
            if (currentActiveOrbitLine != null && currentActiveOrbitLine != newOrbitLine)
                currentActiveOrbitLine.enabled = false;
            if (currentActiveNadirLine != null && currentActiveNadirLine != newNadirLine)
                currentActiveNadirLine.enabled = false;

            // Enable new selection
            newOrbitLine.enabled = true;
            currentActiveOrbitLine = newOrbitLine;
            newNadirLine.enabled = true;
            currentActiveNadirLine = newNadirLine;
        }
    }
}
