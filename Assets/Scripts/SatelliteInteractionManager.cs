using SGPdotNET.Observation;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using XCharts.Runtime;
public class SatelliteInteractionManager : MonoBehaviour
{
    Camera mainCam;
    LineRenderer currentActiveOrbitLine;
    LineRenderer currentActiveNadirLine;
    Material currentActiveSatVisualMaterial;

    public RectTransform canvasRecTransform;
    RectTransform SatHoverInfoPanelRectTransform;
    public GameObject SatHoverInfoPanel;
    public TMP_Text SatHoverInfoPanelText;

    [Header("UI")]
    public TMP_Text TimeScaleText;
    public Slider timeScaleSlider;

    public TMP_Text SatStats;
    string cacheSatStats;
    private StringBuilder sb = new StringBuilder(500);
    public Button x1SpeedButton;

    [Header("Graphing")]
    public LineChart altitudeChart;
    public int graphResolution = 50;

    private Satellite selectedSatellite;
    private SatelliteOrbitManager orbitManager;
    void Awake()
    {
        mainCam = Camera.main;
        orbitManager = GetComponent<SatelliteOrbitManager>();
        SatHoverInfoPanelRectTransform =SatHoverInfoPanel.GetComponent<RectTransform>();
        //timeScaleSlider.onValueChanged.AddListener((value) => {GetComponent<SatelliteOrbitManager>().timeMultiplier = value;});
        timeScaleSlider.minValue = 0f;
        timeScaleSlider.maxValue = 200f;
        timeScaleSlider.value = GetComponent<SatelliteOrbitManager>().timeMultiplier;

        timeScaleSlider.onValueChanged.AddListener((value) => {
            GetComponent<SatelliteOrbitManager>().timeMultiplier = value;
            TimeScaleText.text = $"Time Scale: {value:0.##}x";
        });

        x1SpeedButton.onClick.AddListener(() => SetTimeScale(1f));

    }
    void Update()
    {
        if (selectedSatellite != null)
        {
            UpdateRealtimeStats();
            UpdateOrbitVisuals();
        }

        HandleInput();
    }
    void UpdateOrbitVisuals()
    {
        if (currentActiveOrbitLine == null || orbitManager == null) return;

        double totalDurationMinutes = (orbitManager.orbitDurationHours * 60.0f) / selectedSatellite.Tle.MeanMotionRevPerDay;

        // Calculate fresh points based on CURRENT SimulationTime
        Vector3[] points = Utility.CalcualteOrbitVisualPoints(
            selectedSatellite,
            SatelliteOrbitManager.SimulationTime,
            orbitManager.orbitResolution,
            totalDurationMinutes,
            orbitManager.earthRadius
        );

        currentActiveOrbitLine.positionCount = points.Length;
        currentActiveOrbitLine.SetPositions(points);
    }

    void HandleInput()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePos);
        bool hitDetected = Physics.Raycast(ray, out RaycastHit hit);

        if (!hitDetected) { SatHoverInfoPanel.SetActive(false); return; }

        hit.collider.TryGetComponent<SatelliteBillboard>(out SatelliteBillboard satelliteBillboard);

        if (satelliteBillboard == null) { SatHoverInfoPanel.SetActive(false); return; }

        SatHoverInfoPanel.SetActive(true);
        Vector2 anchoredPos = mousePos / canvasRecTransform.localScale.y;
        SatHoverInfoPanelRectTransform.anchoredPosition = anchoredPos;

        SatHoverInfoPanelText.text = $"{satelliteBillboard.sat.Name} <color=green>{satelliteBillboard.sat.Tle.NoradNumber}</color>";
        SatHoverInfoPanelRectTransform.sizeDelta = SatHoverInfoPanelText.GetRenderedValues(false) + new Vector2(4f, 5f);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SelectSatellite(satelliteBillboard);
        }
    }

    void SelectSatellite(SatelliteBillboard satelliteBillboard)
    {
        selectedSatellite = satelliteBillboard.sat;
        GenerateAltitudeGraph(selectedSatellite);

        LineRenderer newOrbitLine = satelliteBillboard.GetOrbitLineRenderer();
        LineRenderer newNadirLine = satelliteBillboard.GetNadirLineRenderer();
        Material newMat = satelliteBillboard.GetMaterial();

        if (newOrbitLine == null || newNadirLine == null) return;

        // Disable previous selection
        if (currentActiveOrbitLine != null && currentActiveOrbitLine != newOrbitLine)
            currentActiveOrbitLine.enabled = false;
        if (currentActiveNadirLine != null && currentActiveNadirLine != newNadirLine)
            currentActiveNadirLine.enabled = false;
        if (currentActiveSatVisualMaterial != null && currentActiveSatVisualMaterial != newMat)
            currentActiveSatVisualMaterial.color = Color.white;

        // Enable new selection
        currentActiveOrbitLine = newOrbitLine;
        currentActiveNadirLine = newNadirLine;
        currentActiveSatVisualMaterial = newMat;

        newOrbitLine.enabled = true;
        newNadirLine.enabled = true;
        currentActiveSatVisualMaterial.color = Color.forestGreen;

        GenerateStaticStatsString();
    }

    void GenerateStaticStatsString()
    {
        sb.Clear();
        sb.Append("<size=200%>").Append(selectedSatellite.Name).Append("</size>")
            .Append("\nNORAD ID: ").Append(selectedSatellite.Tle.NoradNumber)
            .Append("\nInt'l Code: ").Append(selectedSatellite.Tle.IntDesignator)
            .Append("\nEpoch: ").Append(selectedSatellite.Tle.Epoch)
            .Append("\nInclination: ").Append(selectedSatellite.Tle.Inclination.Degrees.ToString("F2")).Append("°")
            .Append("\nEccentricity: ").Append(selectedSatellite.Tle.Eccentricity.ToString("F4"))
            .Append("\nPerigee: ").Append(selectedSatellite.Orbit.Perigee.ToString("F2")).Append("km")
            .Append("\nApogee: ").Append(selectedSatellite.Orbit.Apogee.ToString("F2")).Append("km")
            .Append("\nPeriod: ").Append(selectedSatellite.Orbit.Period.ToString("F2")).Append("m")
            .Append("\nSemi Major Axis: ").Append(selectedSatellite.Orbit.SemiMajorAxis.ToString("F2")).Append("km")
            .Append("\nBSTAR Drag: ").Append(selectedSatellite.Tle.BStarDragTerm)
            .Append("\nRAAN: ").Append(selectedSatellite.Tle.RightAscendingNode.Degrees.ToString("F2")).Append("°");

        cacheSatStats = sb.ToString();
    }
    void UpdateRealtimeStats()
    {
        if (selectedSatellite == null) return;
        sb.Clear();
        sb.Append(cacheSatStats);
        var eci = selectedSatellite.Predict(SatelliteOrbitManager.SimulationTime);
        var geo = eci.ToGeodetic();
        sb.Append("\nAltitude: ");
        sb.Append(geo.Altitude.ToString("F2"));
        sb.Append(" km");
        sb.Append("\nLat: ");
        sb.Append(geo.Latitude.Degrees.ToString("F2"));
        sb.Append("°");
        sb.Append("\nLong: ");
        sb.Append(geo.Longitude.Degrees.ToString("F2"));
        sb.Append("°");
        SatStats.SetText(sb);
    }

    void SetTimeScale(float scale)
    {
        GetComponent<SatelliteOrbitManager>().timeMultiplier = scale;
        timeScaleSlider.value = scale;
    }

    void GenerateAltitudeGraph(Satellite sat)
    {
        if (altitudeChart == null) return;
    }
}
