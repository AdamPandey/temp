using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Engine : MonoBehaviour
{
    #region Fields
    [Header("Game Settings")]
    public float gameSpeed = 1f;
    public int maxChefs = 5;
    public float wasteReductionRate = 0.1f;

    [Header("Player Settings")]
    public GameObject playerPrefab;
    private GameObject playerInstance;
    private Player currentPlayer;
    private Vector3 playerStartPosition = Vector3.zero;

    [Header("Chef Settings")]
    public GameObject chefPrefab;
    private List<GameObject> chefs = new List<GameObject>();

    [Header("API Settings")]
    private ApiClient apiClient;
    private string baseUrl = "http://127.0.0.1:8000/api/v1/";
    private bool isApiInitialized = false;

    [Header("UI References")]
    public GameObject characterSelectionUI;
    public UnityEngine.UI.InputField playerIdInput;
    public UnityEngine.UI.Text statusText;
    public UnityEngine.UI.Button selectButton;
    #endregion

    #region Unity Methods
    void Awake()
    {
        if (FindObjectOfType<Engine>() != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        InitializeAPI();
    }

    void Start()
    {
        SpawnPlayer();
        SpawnChefs();
        SetupUI();
        StartCoroutine(CheckPlayerData());
    }

    void Update()
    {
        if (playerInstance != null)
        {
            ManageWaste();
        }
    }
    #endregion

    #region Initialization
    private void InitializeAPI()
    {
        apiClient = new ApiClient(baseUrl);
        isApiInitialized = true;
        Debug.Log("API initialized at: " + baseUrl);
    }

    private void SetupUI()
    {
        if (characterSelectionUI != null && selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectClicked);
            statusText.text = "Enter Player ID to Start";
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
            playerInstance.AddComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogError("Player prefab not assigned!");
        }
    }

    private void SpawnChefs()
    {
        for (int i = 0; i < maxChefs; i++)
        {
            Vector3 spawnPos = playerStartPosition + new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
            GameObject chef = Instantiate(chefPrefab, spawnPos, Quaternion.identity);
            chef.AddComponent<ChefAI>();
            chefs.Add(chef);
        }
    }
    #endregion

    #region Player Management
    private IEnumerator CheckPlayerData()
    {
        while (!isApiInitialized) yield return null;
        string defaultId = "player123";
        yield return apiClient.Get("waste/" + defaultId,
            response =>
            {
                currentPlayer = JsonUtility.FromJson<Player>(response);
                if (currentPlayer == null)
                {
                    currentPlayer = new Player(defaultId, 0f, 0);
                    StartCoroutine(CreatePlayer(currentPlayer));
                }
                UpdatePlayerUI();
            },
            error => Debug.LogError("Player Load Error: " + error));
    }

    private IEnumerator CreatePlayer(Player player)
    {
        yield return apiClient.Post("waste", player,
            response => Debug.Log("Player Created: " + response),
            error => Debug.LogError("Player Create Error: " + error));
        UpdatePlayerUI();
    }

    private void UpdatePlayerUI()
    {
        if (statusText != null)
        {
            statusText.text = "Active Player: " + (currentPlayer?.player_id ?? "None");
        }
    }
    #endregion

    #region Game Logic
    private void ManageWaste()
    {
        if (currentPlayer != null)
        {
            foreach (GameObject chef in chefs)
            {
                ChefAI ai = chef.GetComponent<ChefAI>();
                if (ai != null && Vector3.Distance(playerInstance.transform.position, chef.transform.position) < 2f)
                {
                    currentPlayer.waste_quants = Mathf.Max(0f, currentPlayer.waste_quants - wasteReductionRate * Time.deltaTime);
                    StartCoroutine(UpdatePlayerData());
                    break;
                }
            }
        }
    }

    private IEnumerator UpdatePlayerData()
    {
        if (currentPlayer != null)
        {
            yield return apiClient.Put("waste/" + currentPlayer.player_id, currentPlayer,
                response => Debug.Log("Waste Updated: " + response),
                error => Debug.LogError("Waste Update Error: " + error));
        }
    }

    public void RemovePlayer()
    {
        if (currentPlayer != null)
        {
            StartCoroutine(apiClient.Delete("waste/" + currentPlayer.player_id,
                response => Debug.Log("Player Removed: " + response),
                error => Debug.LogError("Delete Error: " + error)));
            currentPlayer = null;
            UpdatePlayerUI();
        }
    }
    #endregion

    #region Character Selection
    private void OnSelectClicked()
    {
        if (playerIdInput != null && !string.IsNullOrEmpty(playerIdInput.text))
        {
            StartCoroutine(LoadSelectedPlayer(playerIdInput.text));
        }
        else
        {
            statusText.text = "Please enter a Player ID!";
        }
    }

    private IEnumerator LoadSelectedPlayer(string playerId)
    {
        statusText.text = "Loading...";
        yield return apiClient.Get("waste/" + playerId,
            response =>
            {
                currentPlayer = JsonUtility.FromJson<Player>(response);
                if (currentPlayer != null)
                {
                    if (playerInstance != null)
                    {
                        Destroy(playerInstance);
                        SpawnPlayer();
                    }
                    UpdatePlayerUI();
                    statusText.text = "Selected: " + playerId;
                }
                else
                {
                    statusText.text = "Player not found! Create new?";
                }
            },
            error =>
            {
                statusText.text = "Error: " + error;
            });
    }

    public void CreateNewSelectedPlayer()
    {
        if (playerIdInput != null && !string.IsNullOrEmpty(playerIdInput.text))
        {
            currentPlayer = new Player(playerIdInput.text, 0f, 0);
            StartCoroutine(CreatePlayer(currentPlayer));
            SpawnPlayer();
        }
    }
    #endregion

    #region Utility
    public void ToggleCharacterSelection(bool active)
    {
        if (characterSelectionUI != null)
        {
            characterSelectionUI.SetActive(active);
        }
    }

    void OnDestroy()
    {
        RemovePlayer();
    }
    #endregion
}