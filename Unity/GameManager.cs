using UnityEngine;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private ApiClient apiClient;
    private Player currentPlayer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        apiClient = new ApiClient("http://127.0.0.1:8000/api/v1/");
        StartCoroutine(LoadOrCreatePlayer("player123")); // Load or create default player
    }

    IEnumerator LoadOrCreatePlayer(string playerId)
    {
        yield return apiClient.Get("waste/" + playerId,
            response =>
            {
                currentPlayer = JsonUtility.FromJson<Player>(response);
                Debug.Log("Loaded Player: " + response);
            },
            error =>
            {
                Debug.LogError("Load Failed: " + error);
                currentPlayer = new Player(playerId, 0f, 0);
                StartCoroutine(CreatePlayer(currentPlayer));
            });
    }

    IEnumerator CreatePlayer(Player player)
    {
        yield return apiClient.Post("waste", player,
            response => Debug.Log("Created Player: " + response),
            error => Debug.LogError("Create Failed: " + error));
    }

    public void UpdateWaste(float amount)
    {
        if (currentPlayer != null)
        {
            currentPlayer.waste_quants += amount;
            StartCoroutine(apiClient.Put("waste/" + currentPlayer.player_id, currentPlayer,
                response => Debug.Log("Updated Waste: " + response),
                error => Debug.LogError("Update Failed: " + error)));
        }
    }

    public void RemovePlayer()
    {
        if (currentPlayer != null)
        {
            StartCoroutine(apiClient.Delete("waste/" + currentPlayer.player_id,
                response => Debug.Log("Deleted Player: " + response),
                error => Debug.LogError("Delete Failed: " + error)));
            currentPlayer = null;
        }
    }
}