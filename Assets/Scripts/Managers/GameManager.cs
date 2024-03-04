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

    // 이거 초기화는 ServerManager에서 서버 만들때나 참여할 때 알아서 해줄거임.(StartHost, StartClient)
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
        // 지금은 무조건 캐릭터가 없어서 이쪽으로 가야하지.
        // 하지만, 나중에 세이브 만들면 이 코드가 유용할거야.
        if (!IsPlayerCharacterReady())
        {
            // 캐릭터가 없으면 캐릭터 만들러 간다. 혼자...간다. 제발.
            SceneManager.LoadScene(characterCustomazationScene, LoadSceneMode.Single);
        }
        else
        {
            // 있으면 월드로 고우고우!
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
        // 캐릭터 셋팅되면 원래 게임화면으로 돌아갈 준비를 해야해.
        players.OnListChanged += HandlePlayerCharacterReady;
        // 캐릭터를 서버에 셋팅할거야.
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
        // 내 캐릭터가 준비가 되었어!
        if (IsPlayerCharacterReady())
        {
            // 캐릭터 준비되면 이벤트 떼고 게임으로 돌아가야해.
            Debug.Log($"player {changeEvnet.Value.clientId}'s character exist: {changeEvnet.Value.characterId}");
            players.OnListChanged -= HandlePlayerCharacterReady;
            SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }
        // 안되었으면 계속 기다려ㅠㅠ.
    }
}
