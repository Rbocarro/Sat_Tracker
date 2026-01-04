using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public struct IssResponse
{
    public IssPosition iss_position;
    public string message;
    public long timestamp;
}

[System.Serializable]
public struct IssPosition
{
    public string latitude;
    public string longitude;
}
public class IssTracker : MonoBehaviour
{
    public GameObject issModel; 
    public float earthRadius ; // Scale of Earth
    private string url = "http://api.open-notify.org/iss-now.json";

    void Start()
    {
        StartCoroutine(UpdateISSPosition());
    }
    IEnumerator UpdateISSPosition()
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                IssResponse data = JsonUtility.FromJson<IssResponse>(request.downloadHandler.text);

                float lat = float.Parse(data.iss_position.latitude);
                float lon = float.Parse(data.iss_position.longitude);
                Debug.Log("ISS pos"+lat+" "+lon);             
                issModel.transform.localPosition=Utility.ConvertSphericalToUnityCoords(lat, lon, earthRadius);
            }
            yield return new WaitForSeconds(5f); // Update every 5 seconds
        }
    }
}