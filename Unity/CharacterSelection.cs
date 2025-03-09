using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelection : MonoBehaviour
{
    public Button selectButton;
    public InputField playerIdInput;
    public Text statusText;
    private ApiClient apiClient;

    void Start()
    {
        apiClient = new ApiClient("http://127.0.0.1:8000/api/v1/");
        selectButton.onClick.AddListener(OnSelectClicked);
        statusText.text = "Enter Player ID and Select";
    }

    void OnSelectClicked()
    {
        string playerId = playerIdInput.text;
        if (string.IsNullOrEmpty(playerId))
        {
            statusText.text = "Please enter a Player ID!";
            return;
        }

        StartCoroutine(LoadPlayer(playerId));
    }

    IEnumerator LoadPlayer(string playerId)
    {
        statusText.text = "Loading...";
        yield return apiClient.Get("waste/" + playerId,
            response =>
            {
                Player player = JsonUtility.FromJson<Player>(response);
                if (player != null)
                {
                    GameManager.Instance.currentPlayer = player;
                    statusText.text = "Selected Player: " + player.player_id;
                    // Load scene or enable game
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

    // Optional: Add Create Player button logic
    public void CreateNewPlayer()
    {
        string playerId = playerIdInput.text;
        Player newPlayer = new Player(playerId, 0f, 0);
        StartCoroutine(GameManager.Instance.CreatePlayer(newPlayer));
    }
}