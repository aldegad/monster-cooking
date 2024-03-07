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
        int playerIndex = GameManager.Instance.GetPlayerIndex(OwnerClientId);

        if (playerIndex > -1 && GameManager.Instance.players[playerIndex].characterId > -1)
        {
            Debug.Log($"InitializeCharacter: owner {OwnerClientId}, playerIndex: {playerIndex} characterId: {GameManager.Instance.players[playerIndex].characterId}");
            GameManager.Instance.SetCharacter(OwnerClientId, GameManager.Instance.players[playerIndex].characterId);
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

        Debug.Log($"player {clientId}'s CharacterId: {characterId}");
        GameManager.Instance.players[playerIndex] = new PlayerData(clientId, characterId);

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