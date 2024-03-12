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
        Initialize();
    }

    private void Initialize()
    {
        // cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // audio
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        foreach (var listener in listeners)
        {
            listener.enabled = false;
        }
        gameObject.GetComponent<AudioListener>().enabled = true;

        // camera
        CinemachineFreeLook playerFollowCameraInstance = Instantiate(playerFollowCamera);
        playerFollowCameraInstance.Follow = follow.transform;
        playerFollowCameraInstance.LookAt = lookAt.transform;
    }
}