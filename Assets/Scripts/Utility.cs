using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Propagation;
using System;
using UnityEngine;
public static class Utility
{   
    public static Vector3 ConvertSphericalToUnityCoords(float latRad, float lonRad, float radius)
    {
        // Spherical to Cartesian (Y-up)
        // x = r * cos(lat) * cos(lon)
        // y = r * sin(lat)
        // z = r * cos(lat) * sin(lon)
        float x = radius * Mathf.Cos(latRad) * Mathf.Cos(lonRad);
        float y = radius * Mathf.Sin(latRad);
        float z = radius * Mathf.Cos(latRad) * Mathf.Sin(lonRad);
        return new Vector3(x, y, z);
    }
    public static Vector3[] CalcualteOrbitVisualPoints(Satellite sat, DateTime startTime,int resolution,double totalDurationMinutes, float earthRadiusUnity = 100f)
    {
        Vector3[] points=new Vector3[resolution+1];
        double timeStep=totalDurationMinutes/resolution;

        for(int i = 0; i <= resolution; i++)
        {
            DateTime predictionTime=startTime.AddMinutes(i*timeStep);
            points[i]=GetSatelliteUnityPosition(sat, predictionTime,earthRadiusUnity);
        }
        return points;
    }
    public static Vector3 GetSatelliteUnityPosition(Satellite sat, DateTime time,float earthRadUnity=100f)
    {
        EciCoordinate eciPos = sat.Predict(time);
        GeodeticCoordinate geo = eciPos.ToGeodetic();
        return ConvertGeodeticToUnityPosition(geo, earthRadUnity);
    }
    public static Vector3 ConvertGeodeticToUnityPosition(GeodeticCoordinate geo,float earthRadiusUnity = 100f)
    {
        // SGP.NET returns radians
        float lat = (float)geo.Latitude.Radians;
        float lon = (float)geo.Longitude.Radians;

        float altScale = 1.0f + ((float)geo.Altitude / (float)SgpConstants.EarthRadiusKm);                     
        float currentRadius = earthRadiusUnity * altScale;                                  
        return ConvertSphericalToUnityCoords(lat, lon, currentRadius);
    }

    public static void GenerateOrbitLineRendererPathAtTime(Satellite sat, DateTime time, float orbitDurationHours,int orbitResolution,float earthRadius,GameObject obj)
    {
        double totalDurationMinutes = (orbitDurationHours * 60.0f) / sat.Tle.MeanMotionRevPerDay;// Calculate the orbital period in mins
        DateTime now = DateTime.UtcNow;
        //Linerender Setup
        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.positionCount = orbitResolution + 1;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = new Color(1, 1, 1, 0.45f);
        lr.useWorldSpace = true;
        lr.alignment = LineAlignment.View; // align view with camera
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.enabled = false;
        lr.SetPositions(CalcualteOrbitVisualPoints(sat, now, orbitResolution, totalDurationMinutes, earthRadius));
    }

    public static Vector3 GetNadirPoint(Satellite sat, DateTime time,float rad)
    {
        EciCoordinate eci = sat.Predict(time);

        GeodeticCoordinate geo = eci.ToGeodetic();

        float lat = (float)geo.Latitude.Radians;
        float lon = (float)geo.Longitude.Radians;

        return ConvertSphericalToUnityCoords(lat, lon, rad);
    }
    public static double GetAltitudeKm(Satellite sat, DateTime time)
    {
        EciCoordinate eci = sat.Predict(time);
        GeodeticCoordinate geo = eci.ToGeodetic();
        return geo.Altitude;
    }
    public static void GetLatLon(Satellite sat,DateTime time,out double latitudeDeg,out double longitudeDeg)
    {
        EciCoordinate eci = sat.Predict(time);
        GeodeticCoordinate geo = eci.ToGeodetic();

        latitudeDeg = geo.Latitude.Degrees;
        longitudeDeg = geo.Longitude.Degrees;
    }
}

