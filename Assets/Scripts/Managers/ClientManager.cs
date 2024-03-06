using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientManager : NetworkBehaviour
{
    [SerializeField] GameObject CharacterSelectUI;

    public override void OnNetworkSpawn()
    {
        // 캐릭터 선택창 오픈!!
        CharacterSelectUI.SetActive(true);
    }
}