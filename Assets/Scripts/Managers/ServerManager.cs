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
using System.Linq;
using System.Collections.Generic;

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

    public async Task StartHost()
    {
        // 연결 승인 처리
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCallback;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCallback;

        string joinCode = await StartHostWithRelay();
        Debug.Log($"Host Joined: {joinCode}");
    }

    public async Task<bool> StartClient(string joinCode)
    {
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

    public ClientRpcParams TargetClient(ulong clientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
        return clientRpcParams;
    }
    public ClientRpcParams ExceptTargetClient(ulong clientId)
    {
        // 연결된 모든 클라이언트의 ID를 가져옵니다.
        IReadOnlyList<ulong> allClientIds = NetworkManager.Singleton.ConnectedClientsIds;

        // 제외할 클라이언트를 제외한 ID 목록을 생성합니다.
        ulong[] targetClientIds = allClientIds.Where(id => id != clientId).ToArray();

        // 특정 클라이언트를 제외한 나머지 클라이언트에게만 메시지를 전송하기 위한 ClientRpcParams를 설정합니다.
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClientIds
            }
        };

        return clientRpcParams;
    }

    private void ApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count >= 4)
        {
            // 4명이상은 연결 거부
            response.Approved = false;
            return;
        }

        // 연결 승인
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Pending = false;
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
