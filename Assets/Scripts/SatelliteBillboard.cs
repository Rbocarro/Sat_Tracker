using SGPdotNET.Observation;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
public class SatelliteBillboard : MonoBehaviour
{
    private static Transform mainCamTransform;
    public float baseScale = 0.1f; // satellite billbaord visual scale
    [Range(0f, 20f)]
    public static float orbitLineWidth=2f;
    LineRenderer parentLine;
    LineRenderer nadirLine;
    public Satellite sat;
    public float distanceFromCam;
    public Texture2D dottedLine;
    public float dotTiling = 20f;

    private uint frameCount = 0;
    private Vector3 maincCamLastPosition;

    GameObject nadirpoint;
    void Start()
    {
        mainCamTransform = Camera.main.transform;
        parentLine = GetComponentInParent<LineRenderer>();
        StartCoroutine(UpdateSatellitePosition());
        maincCamLastPosition = mainCamTransform.position;
        UpdateSatBillboardVisual();

        nadirpoint = new GameObject();
        nadirpoint.transform.SetParent(this.transform);
        SetupNadirLineRenderer();
    }
    void LateUpdate()
    {
       frameCount++;
       float cameraMovementdistance = Vector3.Distance(mainCamTransform.position, maincCamLastPosition);

       if (((cameraMovementdistance >= 0.5f) & (frameCount % 10 == 0)) || (frameCount % 101 == 0))
        {
           UpdateSatBillboardVisual();
           maincCamLastPosition = mainCamTransform.position;
           nadirpoint.transform.position = Utility.GetNadirPoint(sat, SatelliteOrbitManager.SimulationTime,100);
           UpdateNadirLine(); 
        }
    }
    private void UpdateSatBillboardVisual()
    {
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
    public LineRenderer GetOrbitLineRenderer()
    {
        return parentLine;
    }
    public LineRenderer GetNadirLineRenderer()
    {
        return nadirLine;
    }
    IEnumerator UpdateSatellitePosition()
    {
        while (true)
        {
            transform.position = Utility.GetSatellitePosition(sat, SatelliteOrbitManager.SimulationTime, 100);
            //update sat pos based on distance from cam
            yield return new WaitForSeconds(
            distanceFromCam >= 500f ? 5.0f : 0.2f
            );
        }
    }
    private void UpdateNadirLine()
    {
        if (nadirLine == null) return;

        Vector3 satPos = transform.position;
        Vector3 nadirPos = Utility.GetNadirPoint(
            sat,
            SatelliteOrbitManager.SimulationTime,
            100f
        );

        nadirLine.SetPosition(0, satPos);
        nadirLine.SetPosition(1, nadirPos);

        // Width scaling 
        float lw = distanceFromCam / 1000f;
        nadirLine.startWidth = nadirLine.endWidth = lw * orbitLineWidth * 0.5f;

        float length = Vector3.Distance(satPos, nadirPos);

        nadirLine.material.mainTextureScale =
            new Vector2(length * dotTiling, 1f);

    }
    private void SetupNadirLineRenderer()
    {   
        if (this.GetComponent<LineRenderer>() != null)  return;

        nadirLine = this.AddComponent<LineRenderer>();
        // Nadir line setup
        nadirLine.positionCount = 2;
        nadirLine.material = new Material(Shader.Find("Sprites/Default"));
        nadirLine.material.mainTexture = dottedLine;
        nadirLine.textureMode = LineTextureMode.Tile;
        nadirLine.material.color = Color.green;
        nadirLine.useWorldSpace = true;
        nadirLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        nadirLine.alignment = LineAlignment.View;
        nadirLine.enabled = false;
    }
}