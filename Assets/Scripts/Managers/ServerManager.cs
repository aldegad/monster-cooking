using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System;

public class ServerManager : NetworkBehaviour
{
    public static ServerManager Instance { get; private set; }

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
        GameManager.Instance.players = new NetworkList<PlayerData>();
        // 연결 승인 처리
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCallback;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCallback;
        // 플레이어 들어오면 캐릭터 씬 이동 후 캐릭터 생성
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        // 디스 커넥트 이벤트 추가
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        string joinCode = await StartHostWithRelay();

        Debug.Log($"Host Joined: {joinCode}");

        GameManager.Instance.StartGame();
    }

    public async Task<bool> StartClient(string joinCode)
    {
        GameManager.Instance.players = new NetworkList<PlayerData>();

        bool isJoined = false;

        if (joinCode != null)
        {
            try
            {
                isJoined = await StartClientWithRelay(joinCode);
                Debug.Log($"Client Joined: {joinCode}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Client Join Failed: {ex}");
            }
        }

        return isJoined;
    }

    private void ApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (GameManager.Instance.players.Count >= 4)
        {
            // 4명이상은 연결 거부
            response.Approved = false;
            return;
        }

        // 연결 승인
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;

        Debug.Log($"Accept client ({request.ClientNetworkId})");
    }
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Connected client {clientId}");
        GameManager.Instance.AddPlayer(clientId);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        GameManager.Instance.RemovePlayer(clientId);
    }

    private async Task<string> StartHostWithRelay(int maxConnections = 5)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
