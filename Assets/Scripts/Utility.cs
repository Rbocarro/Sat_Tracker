using UnityEngine;
public static class Utility
{   
    public static Vector3 ConvertToUnityCoords(float latRad, float lonRad, float radius)
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
}


