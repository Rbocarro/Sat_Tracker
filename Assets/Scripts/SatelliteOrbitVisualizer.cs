using SGPdotNET.Observation;
using SGPdotNET.TLE;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
public class SatelliteOrbitVisualizer : MonoBehaviour
{
    [Header("Settings")]
    public string tleUrl = "https://celestrak.org/NORAD/elements/gp.php?GROUP=last-30-days&FORMAT=tle";
    public bool loadSatsFromURL=false;
    public float earthRadius = 5.0f;
    public int orbitResolution = 60; // Points per orbit line
    public Material orbitMaterial;
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
    private uint numberofSats=0;

    private void Start()
    {
        SimulationTime = DateTime.UtcNow;
        if(loadSatsFromURL) StartCoroutine(FetchSatellitesFromURL());
        else                FetchSatellitesFromFile();
    }
    private void Update()
    {
        SimulationTime =SimulationTime.AddSeconds(Time.deltaTime * timeMultiplier);
        currentTime.text = SimulationTime.ToString();
    }
    IEnumerator FetchSatellitesFromURL()
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
                CreateOrbitVisuals(sat);
                numberofSats++;
            }
            catch (Exception e) { Debug.LogWarning($"Failed to parse satellite {name}: {e.Message}"); }
            
        }
        Debug.Log(numberofSats + " Satellites Created from URL");
    }
    void FetchSatellitesFromFile()
    {
        string path = "Assets/OrbitFiles/last-30-days.txt";
        string rawData = File.ReadAllText(path).ToString();
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
                CreateOrbitVisuals(sat);
                numberofSats++;
            }
            catch (Exception e) { Debug.LogWarning($"Failed to parse satellite {name}: {e.Message}"); }

        }
        Debug.Log(numberofSats + " Satellites Created From File");
    }


    void CreateOrbitVisuals(Satellite sat)
    {
        //Create a GameObject for satellite's path
        GameObject orbitObj = new GameObject("Orbit_" + sat.Tle.Name);
        orbitObj.transform.SetParent(this.transform);

        Utility.GenerateOrbitPathAtTime(sat, SimulationTime, orbitDurationHours, orbitResolution, earthRadius, orbitObj);

        // Place satellite marker at current position
        GameObject marker = Instantiate(satellitePrefab, orbitObj.transform);
        marker.GetComponent<SatelliteBillboard>().sat = sat;
        marker.gameObject.name = sat.Name;            
    }


}