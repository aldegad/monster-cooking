using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    [SerializeField] private JoinMultiplayerGameUI joinMultiplayerGameUI;

    private void Awake()
    {
        startHostButton.onClick.AddListener(async () =>
        {
            string joinCode = await StartHostWithRelay();
            Debug.Log($"Host Joined: {joinCode}");
            NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.Scene.CharacterSelectScene.ToString(), LoadSceneMode.Single);
        });

        startClientButton.onClick.AddListener(() =>
        {
            joinMultiplayerGameUI.Show();
        });
        joinMultiplayerGameUI.OnJoinCodeEntered += joinMultiplayerGameUI_OnJoinCodeEntered;
    }

    private void OnDestroy()
    {
        joinMultiplayerGameUI.OnJoinCodeEntered -= joinMultiplayerGameUI_OnJoinCodeEntered;
    }

    private async void joinMultiplayerGameUI_OnJoinCodeEntered(string joinCode)
    {
        if (joinCode != null)
        {
            bool isJoined = await StartClientWithRelay(joinCode);

            if (isJoined)
            {
                Debug.Log($"Client Joined: {joinCode}");
            }
            else
            {
                Debug.LogError($"Client Join Failed: {joinCode}");
            }
        }
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
