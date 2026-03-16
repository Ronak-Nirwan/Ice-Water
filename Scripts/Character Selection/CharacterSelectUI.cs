using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class CharacterSelectUI : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Transform charactersHolder;
    [SerializeField] private CharacterSelectButton selectButtonPrefab;
    [SerializeField] private PlayerCard[] playerCards;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] public string mapToLoad = "IslandMap";

    public GameObject LoadingPanel;

    public Button startButton;
    public GameObject MapPanel;

    private NetworkList<CharacterSelectState> players;

    private void Awake()
    {
        players = new NetworkList<CharacterSelectState>();
        startButton.gameObject.SetActive(false);
        MapPanel.SetActive(false);
        LoadingPanel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            Character[] allCharacters = characterDatabase.GetAllCharacters();

            foreach (var character in allCharacters)
            {
                var selectButtonInstance = Instantiate(selectButtonPrefab, charactersHolder);
                selectButtonInstance.SetCharacter(this, character);
            }

            players.OnListChanged += HandlePlayersStateChanged;
        }

        if(IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList) 
            {
                HandleClientConnected(client.ClientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient) 
        {
            players.OnListChanged -= HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    void HandleClientConnected(ulong clientId)
    {
        players.Add(new CharacterSelectState(clientId));
    }

    void HandleClientDisconnected(ulong clientId) 
    {
        for (int i = 0; i < players.Count; i++) 
        {
            if (players[i].ClientId == clientId) 
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    public void Select(Character character)
    {
        characterNameText.text = character.DisplayName;
        characterInfoPanel.SetActive(true);
        SelectServerRpc(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]

    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++) 
        {
            if (players[i].ClientId == serverRpcParams.Receive.SenderClientId)
            {
                players[i] = new CharacterSelectState(
                        players[i].ClientId,
                        characterId
                    );
            }
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        for (int i = 0; i < playerCards.Length; i++) 
        {
            if (players.Count > i) 
            {
                playerCards[i].UpdateDisplay(players[i]);
            }
            else 
            {
                playerCards[i].DisableDisplay();
            }
        }
    }

    public void Update()
    {
        if(players.Count == 1 && IsHost)
        {
            startButton.gameObject.SetActive(true);
            MapPanel.SetActive(true);
        }
    }

    public void LoadLevel(string levelName)
    {
        mapToLoad = levelName;
    }

    public void StartGame()
    {
        LoadingPanel.SetActive(true);
        foreach (var p in players)
        {
            SpawnPlayer(p);
        }
        NetworkManager.Singleton.SceneManager.LoadScene(mapToLoad, UnityEngine.SceneManagement.LoadSceneMode.Single);
        Debug.Log("Load Scene Called");
    }

    public async void StartGameNet()
    {
        LoadingPanel.SetActive(true);
        Debug.Log("Loading panel shown");


        await Task.Delay(1000);

        foreach (var p in players)
        {
            SpawnPlayer(p);
        }


        NetworkManager.Singleton.SceneManager.LoadScene(mapToLoad, LoadSceneMode.Single);
        Debug.Log("Network Scene Load Called");
    }

    public void SpawnPlayer(CharacterSelectState playerState)
    {
        Character character = characterDatabase.GetCharacterById(playerState.CharacterId);

        if (character == null)
        {
            Debug.LogError($"Character with ID {playerState.CharacterId} not found in database!");
            return;
        }
        var playerInstance = Instantiate(character.Prefab);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerState.ClientId);
    }


    public NetworkList<CharacterSelectState> GetPlayerStates()
    {
        return players;
    }
}
