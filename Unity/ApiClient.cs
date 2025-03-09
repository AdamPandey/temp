using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ApiClient
{
    private string baseUrl;

    public ApiClient(string baseUrl)
    {
        this.baseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
    }

    public IEnumerator Post(string endpoint, object data, System.Action<string> onSuccess = null, System.Action<string> onError = null)
    {
        string url = baseUrl + endpoint;
        string jsonData = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                onSuccess?.Invoke(response);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    public IEnumerator Get(string endpoint, System.Action<string> onSuccess = null, System.Action<string> onError = null)
    {
        string url = baseUrl + endpoint;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                onSuccess?.Invoke(response);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    public IEnumerator Put(string endpoint, object data, System.Action<string> onSuccess = null, System.Action<string> onError = null)
    {
        string url = baseUrl + endpoint;
        string jsonData = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                onSuccess?.Invoke(response);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    public IEnumerator Delete(string endpoint, System.Action<string> onSuccess = null, System.Action<string> onError = null)
    {
        string url = baseUrl + endpoint;

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                onSuccess?.Invoke(response);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }
}