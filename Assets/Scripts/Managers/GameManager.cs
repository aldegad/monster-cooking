using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [Header("Scenes")]
    [SerializeField] public string mainMenuScene = "MainMenuScene";
    [SerializeField] public string gameScene = "TestGameScene";

    [Header("Databases")]
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private GameObject playerPrefab;

    public static GameManager Instance { get; private set; }

    // NetworkList<PlayerData> players 초기화는 ServerManager에서 서버 만들때나 참여할 때 알아서 해줄거임.(StartHost, StartClient)
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
        Debug.Log("StartGame!! Eat Delicious Monsters!!");
        // netcode는 scene을 플레이어들 모두 통일해야한대. 그래서 일단 다 게임씬으로 갈거야.
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

    public void UpdateCharacter(ulong clientId, int characterId)
    {
        // 여러가지 테스트를 더 해볼거야. 일단 이렇게 두자.
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerCharacter>().UpdateCharacter(characterId);
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

    /* public GameObject GetPlayerObject(ulong clientId)
    {
        if (playerObjects.TryGetValue(clientId, out GameObject playerObject))
        {
            return playerObject;
        }

        return null;
    } */
}
