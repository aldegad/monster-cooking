using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class CharacterSelectDisplay : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Transform charactersHolder;
    [SerializeField] private CharacterSelectButton selectButtonPrefab;
    [SerializeField] private PlayerCard[] playerCards;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Transform introSpawnPoint;
    [SerializeField] private Button lockInButton;

    private GameObject introInstance;
    private List<CharacterSelectButton> characterSelectButtons = new List<CharacterSelectButton>();

    private NetworkList<CharacterSelectState> players;

    private void Awake()
    {
        players = new NetworkList<CharacterSelectState>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            Character[] allCharacters = characterDatabase.GetAllCharacters();

            foreach (Character character in allCharacters)
            {
                CharacterSelectButton selectButtonInstance = Instantiate(selectButtonPrefab, charactersHolder);
                selectButtonInstance.SetCharacter(this, character);
                characterSelectButtons.Add(selectButtonInstance);
            }

            players.OnListChanged += HandlePlayersStateChanged;
        }

        if(IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        players.Add(new CharacterSelectState(clientId));
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        { 
            if(players[i].ClientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    public void Select(Character character)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != NetworkManager.Singleton.LocalClientId) { continue; }

            if (players[i].IsLockedIn) { return; }

            if (players[i].CharacterId == character.Id) { return; }

            if (IsCharacterTaken(character.Id, false)) { return; }
        }

        characterNameText.text = character.DisplayName;
        characterInfoPanel.SetActive(true);

        if (introInstance != null)
        {
            Destroy(introInstance);
        }

        introInstance = Instantiate(character.IntroPrefab, introSpawnPoint);

        SelectServerRpc(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverPrcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != serverPrcParams.Receive.SenderClientId) { continue; }

            if (!characterDatabase.IsValidCharaterId(characterId)) { return; }

            if (IsCharacterTaken(characterId, true)) { return; }

            players[i] = new CharacterSelectState(
                players[i].ClientId,
                characterId,
                players[i].IsLockedIn
            );
        }
    }

    public void LockedIn()
    {
        LockedInServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LockedInServerRpc(ServerRpcParams serverPrcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != serverPrcParams.Receive.SenderClientId) { continue; }

            if (!characterDatabase.IsValidCharaterId(players[i].CharacterId)) { return; }

            if (IsCharacterTaken(players[i].CharacterId, true)) { return; }

            players[i] = new CharacterSelectState(
                players[i].ClientId,
                players[i].CharacterId,
                true
            );
        }
    }

    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvnet)
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (players.Count > i)
            {
                playerCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                playerCards[i].DisableDisplay();
            }
        }

        foreach (CharacterSelectButton button in characterSelectButtons)
        {
            if (button.IsDisabled) { continue; }

            if (IsCharacterTaken(button.Character.Id, false))
            {
                button.SetDisabled();
            }

            foreach (CharacterSelectState player in players)
            {
                if (player.ClientId != NetworkManager.Singleton.LocalClientId) { continue; }

                if (player.IsLockedIn)
                {
                    lockInButton.interactable = false;
                    break;
                }

                if (IsCharacterTaken(player.CharacterId, false))
                {
                    lockInButton.interactable = false;
                    break;
                }

                lockInButton.interactable = true;

                break;
            }
        }
    }

    private bool IsCharacterTaken(int characterId, bool checkAll)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!checkAll)
            {
                if (players[i].ClientId == NetworkManager.Singleton.LocalClientId) { continue; }
            }

            if (players[i].IsLockedIn && players[i].CharacterId == characterId)
            {
                return true;
            }
        }

        return false;
    }
}
