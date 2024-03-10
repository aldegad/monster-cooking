using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerBase : NetworkBehaviour
{
    [SerializeField] private CinemachineFreeLook playerFollowCamera;
    [SerializeField] private GameObject follow;
    [SerializeField] private GameObject lookAt;

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log($"PlayerBase OnNetworkSpawn OwnerClientId: ${OwnerClientId} / isOwner: {IsOwner}");

        if (!IsOwner) { return; }
        InitializeCamera();
    }
    private void InitializeCamera()
    {
        CinemachineFreeLook playerFollowCameraInstance = Instantiate(playerFollowCamera);
        playerFollowCameraInstance.Follow = follow.transform;
        playerFollowCameraInstance.LookAt = lookAt.transform;
    }
}