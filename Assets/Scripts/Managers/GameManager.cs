using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Cinemachine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] public LoadingScreen loadingScreen;

    [Header("Scenes")]
    [SerializeField] public string mainMenuScene = "MainMenuScene";
    [SerializeField] public string gameScene = "TestGameScene";

    [Header("Databases")]
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private GameObject playerPrefab;

    [Header("Auto Settings")]
    [SerializeField] public GameState gameState = GameState.Menu;
    [SerializeField] public PlayerFollowCamera playerFollowCamera;


    public static GameManager Instance { get; private set; }

    public NetworkList<PlayerData> players;


    public CharacterDatabase CharacterDatabase => characterDatabase;

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            players.OnListChanged += HandlePlayersStateChanged;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsHost)
        {
            players.OnListChanged -= HandlePlayersStateChanged;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public async void StartHost()
    {
        loadingScreen.gameObject.SetActive(true);
        players = new NetworkList<PlayerData>();
        await ServerManager.Instance.StartHost();
        StartGame();
    }

    public Task<bool> StartClient(string joinCode)
    {
        loadingScreen.gameObject.SetActive(true);
        players = new NetworkList<PlayerData>();
        return ServerManager.Instance.StartClient(joinCode);
    }

    public void AddPlayer(ulong clientId)
    {
        players.Add(new PlayerData(clientId));
    }

    public void RemovePlayer(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
        
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

    public int GetPlayerIndex(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            PlayerData player = players[i];

            if (player.clientId != clientId) { continue; }

            return i;
        }
        return -1;
    }

    public PlayerData GetPlayer(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            PlayerData player = players[i];

            if (player.clientId != clientId) { continue; }

            return player;
        }
        return new PlayerData();
    }

    public void UpdateCharacter(int characterId)
    {
        UpdateCharacterServerRpc(characterId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateCharacterServerRpc(int characterId, ServerRpcParams serverPrcParams = default)
    {
        NetworkManager.Singleton.ConnectedClients.TryGetValue(serverPrcParams.Receive.SenderClientId, out NetworkClient client);
        client.PlayerObject.GetComponent<PlayerCharacter>().UpdateCharacter(characterId);
    }

    private void HandlePlayersStateChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<PlayerData>.EventType.Add:
                Debug.Log($"Player added: {changeEvent.Value.clientId}");

                GameObject playerInstance = Instantiate(playerPrefab);
                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(players[changeEvent.Index].clientId);
                break;

            case NetworkListEvent<PlayerData>.EventType.Remove:
                Debug.Log($"Player removed: {changeEvent.Index}");

                break;

            case NetworkListEvent<PlayerData>.EventType.Value:

                // Debug.Log($"Player data changed at {changeEvent.Index}: Character ID = {changeEvent.Value.characterId}");
                break;
        }
    }
}


[Serializable]
public enum GameState
{
    Exploration,
    BuildingUI,
    Building,
    Menu
}