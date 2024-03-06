using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientManager : NetworkBehaviour
{
    [SerializeField] GameObject CharacterSelectUI;

    public override void OnNetworkSpawn()
    {
        // ĳ���� ����â ����!!
        CharacterSelectUI.SetActive(true);
    }
}