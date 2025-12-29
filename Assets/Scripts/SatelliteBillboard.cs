using SGPdotNET.Observation;
using UnityEngine;
using System.Collections;
public class SatelliteBillboard : MonoBehaviour
{
    private static Transform mainCamTransform;
    public float baseScale = 0.1f; // satellite billbaord visual scale
    [Range(0f, 20f)]
    public static float orbitLineWidth=2f;
    LineRenderer parentLine;
    public Satellite sat;
    public float distanceFromCam;
    void Start()
    {
        mainCamTransform = Camera.main.transform;
        parentLine = GetComponentInParent<LineRenderer>();
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
        if (parentLine.enabled)
        {
            parentLine.startWidth = parentLine.endWidth = (lw * orbitLineWidth);
            parentLine.alignment = LineAlignment.View;
        }
    }
    public LineRenderer GetLineRenderer()
    {
        return parentLine;
    }
    IEnumerator UpdateSatellitePosition()
    {
        while (true)
        {
            transform.position = SGPToUnityUtility.GetSatellitePosition(sat, SatelliteOrbitVisualizer.SimulationTime, 100);
            //update sat pos based on distance from cam
            yield return new WaitForSeconds(
            distanceFromCam >= 500f ? 5.0f : 0.2f
            );
        }
    }
}