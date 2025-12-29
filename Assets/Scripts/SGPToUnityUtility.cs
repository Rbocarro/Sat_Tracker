using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Propagation;
using System;
using UnityEngine;
public static class SGPToUnityUtility
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
            points[i]=GetSatellitePosition(sat, predictionTime,earthRadiusUnity);
        }
        return points;
    }
    public static Vector3 GetSatellitePosition(Satellite sat, DateTime time,float earthRadUnity=100f)
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
}

