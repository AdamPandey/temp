using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private ApiClient apiClient;

    void Start()
    {
        apiClient = new ApiClient("http://127.0.0.1:8000/api/v1/");
        StartCoroutine(TestApiCalls());
    }

    IEnumerator TestApiCalls()
    {
        
        var newPlayer = new
        {
            player_id = "player456",
            waste_quants = 3.0,
            rat_count = 1
        };
        yield return apiClient.Post("waste", newPlayer,
            response => Debug.Log("POST Success: " + response),
            error => Debug.LogError("POST Error: " + error));

        
        yield return apiClient.Get("waste/player123",
            response => Debug.Log("GET Success: " + response),
            error => Debug.LogError("GET Error: " + error));

        
        var updateData = new { waste_quants = 4.0, rat_count = 2 };
        yield return apiClient.Put("waste/player123", updateData,
            response => Debug.Log("PUT Success: " + response),
            error => Debug.LogError("PUT Error: " + error));

        
        yield return apiClient.Delete("waste/player123",
            response => Debug.Log("DELETE Success: " + response),
            error => Debug.LogError("DELETE Error: " + error));
    }
}