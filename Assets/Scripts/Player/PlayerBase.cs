using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerBase : NetworkBehaviour
{
    [SerializeField] private CinemachineFreeLook playerFollowCamera;

    private void Awake()
    {
        InitializeCamera();
    }
    private void InitializeCamera()
    {
        CinemachineFreeLook playerFollowCameraInstance = Instantiate(playerFollowCamera);
        playerFollowCameraInstance.Follow = transform.Find("CameraFocus").transform;
        playerFollowCameraInstance.LookAt = transform.Find("CameraFocus").transform;
    }

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log($"Player Character OnNetworkSpawn OwnerClientId: ${OwnerClientId} / isOwner: {IsOwner}");
    }
}