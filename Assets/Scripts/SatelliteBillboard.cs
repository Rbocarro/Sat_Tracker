using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Propagation;
using SGPdotNET.TLE;
using UnityEngine;
using System.Collections;
public class SatelliteBillboard : MonoBehaviour
{
    private static Transform mainCamTransform;
    public float baseScale = 0.1f; // satellite billbaord visual scale
    public static float orbitLineWidth=1f;
    static float earthradius = (float)SgpConstants.EarthRadiusKm;
    LineRenderer parentLine;
    public Tle tle;
    Satellite sat;
    public float distanceFromCam;
    void Start()
    {
        mainCamTransform = Camera.main.transform;
        parentLine = GetComponentInParent<LineRenderer>();
        sat = new Satellite(tle);
        StartCoroutine(UpdateSatellitePosition());

    }
    void LateUpdate()
    {
        UpdateSatBillboardVisual();
    }
    private void UpdateSatBillboardVisual()
    {
        if (mainCamTransform == null) return;

        // Make the plane parallel to the camera's forward vector
        transform.rotation = Quaternion.LookRotation(mainCamTransform.forward, mainCamTransform.up);
        transform.Rotate(90, 0, 0);

        // Scaling logic
        distanceFromCam = Vector3.Distance(transform.position, mainCamTransform.position);
        transform.localScale = Vector3.one * (distanceFromCam * baseScale);

        //line width logic
        float lw = distanceFromCam / 1000;
        parentLine.startWidth = parentLine.endWidth = (lw * orbitLineWidth);
        parentLine.alignment= LineAlignment.View;
    }
    public LineRenderer GetLineRenderer()
    {
        return parentLine;
    }
    IEnumerator UpdateSatellitePosition()
    {
        while (true)
        {
            EciCoordinate eci = sat.Predict(SatelliteOrbitVisualizer.SimulationTime);
            GeodeticCoordinate geo = eci.ToGeodetic();

            float lat = (float)geo.Latitude.Radians;
            float lon = (float)geo.Longitude.Radians;

            float altScale = 1.0f + ((float)geo.Altitude / earthradius);
            float radius = 100.0f * altScale;

            transform.position = Utility.ConvertToUnityCoords(lat, lon, radius);

            //update sat pos based on distance from cam
            yield return new WaitForSeconds(
            distanceFromCam >= 500f ? 5.0f : 0.2f
            );
        }
    }
}