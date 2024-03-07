using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerBase : NetworkBehaviour
{
    private void Awake()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log($"Player Character OnNetworkSpawn OwnerClientId: ${OwnerClientId} / isOwner: {IsOwner}");
    }
}