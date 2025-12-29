using SGPdotNET.Observation;
using SGPdotNET.TLE;
using SGPdotNET.CoordinateSystem;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using SGPdotNET.Propagation;
public class SatelliteOrbitVisualizer : MonoBehaviour
{
    [Header("Settings")]
    public string tleUrl = "https://celestrak.org/NORAD/elements/gp.php?GROUP=last-30-days&FORMAT=tle";
    public float earthRadius = 5.0f;
    public int orbitResolution = 60; // Points per orbit line
    public Material orbitMaterial;
    [Range(0f, 20f)]
    public float orbitLineWidth = 2.0f;
    public float orbitDurationHours = 24.0f;
    [Tooltip("Controls the speed of all satellites. 0 is paused.")]
    [Range(0f, 200f)]
    public float timeMultiplier = 1.0f;

    [Header("Prefabs")]
    public GameObject satellitePrefab;

    [Header("UI elements")]
    public TMP_Text currentTime;
    public static DateTime SimulationTime;

    //Internal
    private const float EarthRadiusKm = (float)SgpConstants.EarthRadiusKm;
    private uint numberofSats=0;

    private void Start()
    {
        SimulationTime = DateTime.UtcNow;
        StartCoroutine(FetchAndDrawSatellites());
    }
    private void Update()
    {
        SatelliteBillboard.orbitLineWidth = orbitLineWidth;
        SimulationTime =SimulationTime.AddSeconds(Time.deltaTime * timeMultiplier);
        currentTime.text = SimulationTime.ToString();
    }
    IEnumerator FetchAndDrawSatellites()
    {
        UnityWebRequest request = UnityWebRequest.Get(tleUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching TLE info: " + request.error);
            yield break;
        }

        string rawData = request.downloadHandler.text;
        string[] lines = rawData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // TLE info come in sets of 3 lines
        for (int i = 0; i < lines.Length; i += 3)
        {
            if (i + 2 >= lines.Length) break;

            string name = lines[i].Trim();
            string line1 = lines[i + 1];
            string line2 = lines[i + 2];

            try
            {
                Tle tle = new Tle(name, line1, line2);
                Satellite sat = new Satellite(tle);
                CreateOrbitVisuals(sat, tle);
                numberofSats++;
            }
            catch (Exception e) { Debug.LogWarning($"Failed to parse satellite {name}: {e.Message}"); }
            
        }
        Debug.Log(numberofSats + " Satellites Created");
    }
    void CreateOrbitVisuals(Satellite sat,Tle tle)
    {
        //Create a GameObject for satellite's path
        GameObject orbitObj = new GameObject("Orbit_" + sat.Tle.Name);
        orbitObj.transform.SetParent(this.transform);
        
        LineRenderer lr = orbitObj.AddComponent<LineRenderer>();
        lr.positionCount = orbitResolution + 1;
        lr.startWidth = lr.endWidth = orbitLineWidth;
        lr.material = orbitMaterial != null ? orbitMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.useWorldSpace = true;
        lr.alignment = LineAlignment.View; // align view with camera
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        lr.enabled = false;
        
        
        double totalDurationMinutes = (orbitDurationHours * 60.0f)/ sat.Tle.MeanMotionRevPerDay;// Calculate the orbital period in mins
        double timeStep = totalDurationMinutes / orbitResolution;                               // Calculate the total time span in minutes hours to minutes

        DateTime now = DateTime.UtcNow;
        Vector3[] points = new Vector3[orbitResolution + 1];

        for (int i = 0; i <= orbitResolution; i++)
        {
            DateTime predictionTime = now.AddMinutes(i * timeStep);

            // Predict ECI position and convert to Geodetic (Lat/Lon/Alt)
            EciCoordinate eciPos = sat.Predict(predictionTime);
            GeodeticCoordinate geo = eciPos.ToGeodetic();

            // SGP.NET returns radians
            float lat = (float)geo.Latitude.Radians;
            float lon = (float)geo.Longitude.Radians;

            float altScale = 1.0f + ((float)geo.Altitude / EarthRadiusKm);                      // Calculate height.geo.Altitude is in km, Earth radius is 6378km
            float currentRadius = earthRadius * altScale;                                       // Scale the altitude relative to earthRadius

            points[i] = Utility.ConvertToUnityCoords(lat, lon, currentRadius);
        }

        lr.SetPositions(points);

        // Place satellite marker at current position
        GameObject marker = Instantiate(satellitePrefab, orbitObj.transform);
        marker.GetComponent<SatelliteBillboard>().tle = tle;
        marker.gameObject.name =tle.Name;            
    }
}