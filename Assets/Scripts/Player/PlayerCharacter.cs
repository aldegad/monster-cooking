using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCharacter : NetworkBehaviour
{
    //[SerializeField] private GameObject characterContainer;
    [SerializeField] private List<GameObject> character;


    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);

        Debug.Log($"Player Character Spawn: owner {OwnerClientId}");
        InitializeCharacter();
    }

    private void InitializeCharacter()
    {
        int playerIndex = GameManager.Instance.GetPlayerIndex(OwnerClientId);

        if (playerIndex > -1 && GameManager.Instance.players[playerIndex].characterId > -1)
        {
            Debug.Log($"InitializeCharacter: owner {OwnerClientId}, playerIndex: {playerIndex} characterId: {GameManager.Instance.players[playerIndex].characterId}");
            GameManager.Instance.SetCharacter(OwnerClientId, GameManager.Instance.players[playerIndex].characterId);
        }
    }

    // 서버 전용
    public void UpdateCharacter(int characterId)
    {
        UpdateCharacterClientRpc(characterId);
    }

    [ClientRpc]
    private void UpdateCharacterClientRpc(int characterId)
    {
        character[characterId].SetActive(true);
    }
}