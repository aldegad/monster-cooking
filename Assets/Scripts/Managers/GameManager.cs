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

    public static GameManager Instance { get; private set; }

    // NetworkList<PlayerData> players �ʱ�ȭ�� ServerManager���� ���� ���鶧�� ������ �� �˾Ƽ� ���ٰ���.(StartHost, StartClient)
    public NetworkList<PlayerData> players;

    public CharacterDatabase CharacterDatabase => characterDatabase;

    public override void OnNetworkSpawn()
    {
        Debug.Log("GameManagerNetworkSpawned");
        if (IsClient)
        {
            players.OnListChanged += HandlePlayersStateChanged;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsClient)
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
        // netcode�� scene�� �÷��̾�� ��� �����ؾ��Ѵ�. �׷��� �ϴ� �� ���Ӿ����� ���ž�.
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

    public void SpawnPlayer()
    {
        SpawnPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams serverPrcParams = default)
    {
        ulong clientId = serverPrcParams.Receive.SenderClientId;
        int playerIndex = GetPlayerIndex(clientId);

        Vector3 spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));

        GetPlayerObject(clientId).GetComponent<Rigidbody>().MovePosition(spawnPos);
    }


    public void UpdateCharacter(ulong clientId, int characterId)
    {
        GetPlayerObject(clientId).GetComponent<PlayerCharacter>().UpdateCharacter(characterId);
    }
    
    private void HandlePlayersStateChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        /* switch (changeEvent.Type)
        {
            case NetworkListEvent<PlayerData>.EventType.Add:
                // ��Ұ� �߰���
                Debug.Log($"Player added: {changeEvent.Value.clientId}");
                break;
            case NetworkListEvent<PlayerData>.EventType.Remove:
                // ��Ұ� ������
                Debug.Log($"Player removed: {changeEvent.Index}");
                break;
            case NetworkListEvent<PlayerData>.EventType.Value:
                // ��� ���� �����
                Debug.Log($"Player data changed at {changeEvent.Index}: Character ID = {changeEvent.Value.characterId}");
                break;
        } */
    }

    private GameObject GetPlayerObject(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            // �÷��̾� ������Ʈ�� �ִ� ���, �ش� GameObject�� ��ȯ�մϴ�.
            if (client.PlayerObject != null)
            {
                return client.PlayerObject.gameObject;
            }
        }

        return null;
    }
}
