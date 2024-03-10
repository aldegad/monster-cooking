using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCharacter : NetworkBehaviour
{

    [SerializeField] private GameObject characterContainer;
    public override void OnNetworkSpawn()
    {
        InitializeCharacter();
    }

    private void InitializeCharacter()
    {
        InitializeCharacterServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitializeCharacterServerRpc()
    {
        int playerIndex = GameManager.Instance.GetPlayerIndex(OwnerClientId);

        if (playerIndex == -1) { return; }

        int chracterId = GameManager.Instance.players[playerIndex].characterId;
        if (GameManager.Instance.players[playerIndex].characterId > -1)
        {
            Debug.Log($"PlayerCharacter InitializeCharacter: owner {OwnerClientId}, playerIndex: {playerIndex} characterId: {chracterId}");
            UpdateCharacter(chracterId);
        }
        else
        {
            Debug.Log($"PlayerCharacter InitializeCharacter: owner {OwnerClientId}, playerIndex: {playerIndex} characterId(new): {chracterId}");
            UpdateCharacter(0);
        }
    }

    public void UpdateCharacter(int characterId)
    {
        UpdateCharacterServerRpc(OwnerClientId, characterId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateCharacterServerRpc(ulong clientId, int characterId, ServerRpcParams serverPrcParams = default)
    {
        int playerIndex = GameManager.Instance.GetPlayerIndex(clientId);

        Debug.Log($"Player {clientId} change character. CharacterId: {characterId}");
        GameManager.Instance.players[playerIndex] = PlayerData.SetCharacterId(playerIndex, characterId);

        UpdateCharacterClientRpc(characterId);
    }

    [ClientRpc]
    private void UpdateCharacterClientRpc(int characterId)
    {
        foreach (Transform child in characterContainer.transform)
        { 
            Destroy(child.gameObject);
        }

        Instantiate(GameManager.Instance.CharacterDatabase.GetCharacter(characterId).CharacterPrefab, characterContainer.transform);
    }
}