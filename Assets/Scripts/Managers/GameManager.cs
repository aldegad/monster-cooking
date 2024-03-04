using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [Header("Scenes")]
    [SerializeField] public string mainMenuScene = "MainMenuScene";
    [SerializeField] public string characterCustomazationScene = "CharacterSelectScene";
    [SerializeField] public string gameScene = "TestGameScene";

    [Header("Databases")]
    [SerializeField] private CharacterDatabase characterDatabase;

    public static GameManager Instance { get; private set; }

    // �̰� �ʱ�ȭ�� ServerManager���� ���� ���鶧�� ������ �� �˾Ƽ� ���ٰ���.(StartHost, StartClient)
    public NetworkList<PlayerData> players;

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

    public void StartGame()
    {
        // ������ ������ ĳ���Ͱ� ��� �������� ��������.
        // ������, ���߿� ���̺� ����� �� �ڵ尡 �����Ұž�.
        if (!IsPlayerCharacterReady())
        {
            // ĳ���Ͱ� ������ ĳ���� ���鷯 ����. ȥ��...����. ����.
            SceneManager.LoadScene(characterCustomazationScene, LoadSceneMode.Single);
        }
        else
        {
            // ������ ����� �����!
            SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }
    }

    public bool IsPlayerCharacterReady()
    {
        ulong clientId = NetworkManager.Singleton.LocalClient.ClientId;
        int playerIndex = GetPlayerIndex(clientId);

        

        if (playerIndex < 0) {
            Debug.Log($"player doesn't exist: {clientId}");
            return false; 
        }

        if (players[playerIndex].characterId < 1)
        {
            Debug.Log($"player {clientId}'s character doesn't exist.");
            return false;
        }

        Debug.Log($"player {clientId}'s character exist: {players[playerIndex].characterId}");
        return true;
    }

    public void SetCharacter(int characterId)
    {
        // ĳ���� ���õǸ� ���� ����ȭ������ ���ư� �غ� �ؾ���.
        players.OnListChanged += HandlePlayerCharacterReady;
        // ĳ���͸� ������ �����Ұž�.
        SetCharacterServerRpc(characterId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCharacterServerRpc(int characterId, ServerRpcParams serverPrcParams = default)
    {
        ulong clientId = serverPrcParams.Receive.SenderClientId;
        int i = GetPlayerIndex(clientId);

        Debug.Log($"player {clientId}'s CharacterId: {characterId}");
        players[i] = new PlayerData(OwnerClientId, characterId);
    }

    private void HandlePlayerCharacterReady(NetworkListEvent<PlayerData> changeEvnet)
    {
        // �� ĳ���Ͱ� �غ� �Ǿ���!
        if (IsPlayerCharacterReady())
        {
            // ĳ���� �غ�Ǹ� �̺�Ʈ ���� �������� ���ư�����.
            Debug.Log($"player {changeEvnet.Value.clientId}'s character exist: {changeEvnet.Value.characterId}");
            players.OnListChanged -= HandlePlayerCharacterReady;
            SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }
        // �ȵǾ����� ��� ��ٷ��Ф�.
    }
}
